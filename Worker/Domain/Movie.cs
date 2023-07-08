using System.ComponentModel.DataAnnotations;

namespace Worker.Model;

public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Price { get; set; }
    [DataType(DataType.Date)]
    public DateTime RunDate { get; set; }
    public int FreeSeats { get; set; }

    public Movie(Guid id, String title, Int32 price, DateTime run_date, Int32 free_seats)
    {
        Id = id;
        Title = title;
        Price = price;
        RunDate = run_date;
        FreeSeats = free_seats;
    }
}
