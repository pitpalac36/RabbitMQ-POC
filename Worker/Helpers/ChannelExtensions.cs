using RabbitMQ.Client;

namespace Worker.Helpers;

public static class ChannelExtensions
{
    public static void SendSerialized<T>(this IModel channel, T body)
    {
        channel.BasicPublish("", routingKey: channel.CurrentQueue, body: Serializer.Serialize(body));
    }

}