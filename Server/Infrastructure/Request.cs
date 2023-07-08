namespace Server.Infrastructure;

public enum RequestType
{
    Get,
    Buy,
    Cancel
}

public class Request
{
    public Guid OrderId { get; set; }
    public RequestType RequestType { get; set; }
    public Guid? MovieId { get; set; }
    public Guid? TicketId { get; set; }
}