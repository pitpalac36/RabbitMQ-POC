using Server.Application;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using Server.Infrastructure;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class Controller : ControllerBase
    {
        private IMessageProducer messageProducer;
        private IModel requestChannel;
        private IModel responseChannel;
        private ConcurrentDictionary<Guid, InternalResponse> pending;

        public Controller(IMessageProducer messageProducer, IModel requestChannel, IModel responseChannel, ConcurrentDictionary<Guid, InternalResponse> pending)
        {
            this.messageProducer = messageProducer;
            this.requestChannel = requestChannel;
            this.responseChannel = responseChannel;
            this.pending = pending;
        }

        [HttpPost]
        public async Task<IActionResult> BuyTicket(Guid movieId)
        {
            // create message
            var request = new Request
            {
                OrderId = Guid.NewGuid(),
                RequestType = RequestType.Buy,
                MovieId = movieId,
            };
            messageProducer.SendMessage(request, requestChannel, "requestQueue");

            var internalResponse = new InternalResponse
            {
                Request = request,
                ShouldResend = true,
            };

          //  new Thread(() =>
          //  {
          //      Thread.Sleep(5000);

          //      var response = new Response
          //      {
          //          OrderId = request.OrderId,
          //          ResponseType = ResponseType.Res,
          //          RequestType = RequestType.Buy,
          //          Success = true,
          //          TicketId = Guid.NewGuid(),
          //      };
          //      Console.WriteLine("Sending response: " + response.OrderId);
          //      messageProducer.SendMessage(response, responseChannel, "responseQueue") ;
          //  }).Start();
            
            lock (internalResponse)
            {
                Console.WriteLine("Putting order in queue");
                pending[request.OrderId] = internalResponse;
                Monitor.Wait(internalResponse);
                Console.WriteLine("Removing order from queue");
                pending.Remove(request.OrderId, out internalResponse);
            }

            return Ok(internalResponse.Response);
        }

        [HttpPost]
        public async Task<IActionResult> CancelTicket(Guid ticketId)
        {
            // create message
            var request = new Request
            {
                OrderId = Guid.NewGuid(),
                RequestType = RequestType.Cancel,
                TicketId = ticketId,
            };

            var response = new InternalResponse
            {
                Request = request,
                ShouldResend = false,
            };

            lock (response)
            {
                pending[request.OrderId] = response;
                messageProducer.SendMessage(request, requestChannel, "requestQueue");
                Monitor.Wait(response);
                pending.Remove(request.OrderId, out response);
            }

            return Ok(response.Response);
        }

        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            // create message
            var request = new Request
            {
                OrderId = Guid.NewGuid(),
                RequestType = RequestType.Get,
            };
            messageProducer.SendMessage(request, requestChannel, "requestQueue");

            var response = new InternalResponse
            {
                Request = request,
                ShouldResend = false,
            };

            lock (response)
            {
                pending[request.OrderId] = response;
                Monitor.Wait(response);
                pending.Remove(request.OrderId, out response);
            }

            return Ok(response.Response);
        }
    }
}