namespace Worker.Model;

public class Ticket
{
    public Guid Id { get; set; }
    public Movie Movie { get; set; }

    public Ticket(Guid id, Movie movie)
    {
        Id = id;
        Movie = movie;
    }
}
