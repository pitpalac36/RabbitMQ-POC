using Server.Application;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using Server.Infrastructure;

public class Program
{
    public static void Main(string[] args)
    {
        // connect to RabbitMQ
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = factory.CreateConnection();

        // channel will allow us to interact with the RabbitMQ API
        using var requestChannel = connection.CreateModel();
        using var responseChannel = connection.CreateModel();

        IMessageProducer producer = new RabbitMQProducer();

        // declare queues
        requestChannel.QueueDeclare("requestQueue", durable: false, exclusive: false);
        responseChannel.QueueDeclare("responseQueue", durable: false, exclusive: false);

        ConcurrentDictionary<Guid, InternalResponse> pending = new ConcurrentDictionary<Guid, InternalResponse>(Environment.ProcessorCount * 2, 100);

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // inject producer
        builder.Services.AddSingleton(implementationFactory: sp => producer);

        // inject channels
        builder.Services.AddSingleton(implementationFactory: sp => requestChannel);
        builder.Services.AddSingleton(implementationFactory: sp => responseChannel);

        // inject pending dictionary
        builder.Services.AddSingleton(implementationFactory: sp => pending);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        // start processor
        var processor = new ResponseProcessor(producer, requestChannel, responseChannel, pending);
        processor.Start();

        app.Run();
    }

}

