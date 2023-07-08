using System.ComponentModel.DataAnnotations;

namespace Server.Domain;

public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Price { get; set; }
    [DataType(DataType.Date)]
    public DateTime RunDate { get; set; }
    public int FreeSeats { get; set; }

    public Movie(string title, int price, DateTime date, int freeSeats)
    {
        Id = Guid.NewGuid();
        Title = title;
        Price = price;
        RunDate = date;
        FreeSeats = freeSeats;
    }
}