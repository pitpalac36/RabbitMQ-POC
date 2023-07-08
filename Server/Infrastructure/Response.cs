using Server.Domain;
namespace Server.Infrastructure;

public enum ResponseType
{
    Res,
    Ack,
}

public class Response
{
    public Guid OrderId { get; set; }
    public ResponseType ResponseType { get; set; }
    public RequestType RequestType { get; set; }

    public bool Success { get; set; } 
    public string? ErrorMessage { get; set; } 

    // for response to get
    public List<Movie>? Movies { get; set; }
    // for response to buy
    public Ticket? Ticket { get; set; }
    // for response to cancel
    public Movie? Movie { get; set; }
}

public class InternalResponse
{
    public Request Request { get; set; }
    public Response? Response { get; set; }
    public bool ShouldResend { get; set; }
}