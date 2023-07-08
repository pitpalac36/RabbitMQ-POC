using RabbitMQ.Client;
using Server.Helper;

namespace Server.Application
{
    public class RabbitMQProducer : IMessageProducer
    {
        public void SendMessage<T>(T message, IModel channel, string queue)
        {
            channel.BasicPublish(exchange: "", routingKey: queue, body: Serializer.Serialize(message));
        }
    }
}