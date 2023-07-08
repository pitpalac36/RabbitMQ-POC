using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Server.Helper;
using Server.Infrastructure;
using System.Collections.Concurrent;

namespace Server.Application
{
    public class ResponseProcessor
    {
        private IMessageProducer producer;
        private IModel requestChannel;
        private IModel responseChannel;
        private ConcurrentDictionary<Guid, InternalResponse> pending;

        public ResponseProcessor(IMessageProducer producer, IModel requestChannel, IModel responseChannel, ConcurrentDictionary<Guid, InternalResponse> pending)
        {
            this.producer = producer;
            this.requestChannel = requestChannel;
            this.responseChannel = responseChannel;
            this.pending = pending;
        }

        public void Start()
        {
            Thread t = new Thread(new ThreadStart(Run));
            t.Start();
        }

        void Run()
        {
            Console.WriteLine("Waiting for responses...");

            // wait for responses
            var consumer = new EventingBasicConsumer(responseChannel);
            consumer.Received += (model, eventArgs) =>
            {
                Console.WriteLine("Some response came");
                var body = eventArgs.Body.ToArray();
                Response received = Serializer.Deserialize<Response>(body);
                Console.WriteLine("Got a response: " + received.OrderId);

                // acknowledgement
                if (received.ResponseType == ResponseType.Ack)
                {
                    Console.WriteLine("Received Ack");
                    new Thread(() =>
                    {
                        Thread.Sleep(10000);
                        InternalResponse order = pending.GetValueOrDefault(received.OrderId, new InternalResponse{ ShouldResend = false });
                        lock (order)
                        {
                            if (order.ShouldResend)
                            {
                                producer.SendMessage(order.Request, requestChannel, "requestQueue");
                            }
                        }
                    }
                    ).Start();

                }

                // response
                else
                {
                    Console.WriteLine("Received Response");
                    var order = pending[received.OrderId];
                    lock (order)
                    {
                        order.ShouldResend = false;
                        order.Response = received;
                        Monitor.Pulse(order);
                    }
                }
            };
            responseChannel.BasicConsume(queue: "responseQueue", autoAck: true, consumer: consumer);
        }
    }
}
