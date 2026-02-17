public enum Status
{
    NotStarted,
    Reading,
    Finished,
}

public class Book
{
    public int Id { get; set; }
    public int Year { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Category { get; set; }
    public Status Status { get; set; }

    public Book() { }

    public Book(int id, int year, string title, string author, string category, Status status)
    {
        Title = title;
        Author = author;
        Year = year;
        Category = category;
        Id = id;
        Status = status;
    }

}



