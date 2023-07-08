using RabbitMQ.Client;

namespace Server.Application
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message, IModel channel, string queue);
    }
}
