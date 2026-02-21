using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
/// <summary>
/// Core database class for the system. It is used to interact with the in-memory database and contains a DbSet of books.
/// </summary>
/// <note>This will be changed soon to work with the planned sqlite database. </note>
class BookDb : DbContext
{
    public BookDb(DbContextOptions<BookDb> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();
}