namespace Server.Domain;

public class Ticket
{
    public Guid Id { get; set; }
    public Movie Movie { get; set; }

    public Ticket(Movie movie)
    {
        Id = Guid.NewGuid();
        Movie = movie;
    }
}