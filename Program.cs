using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BookDb>(opt => opt.UseInMemoryDatabase("Book"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "BookTrackerAPI";
    config.Title = "BookTrackAPI v1";
    config.Version = "v1";
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "BookTrackerAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}


/*
this part prepopulates some books.
we want to use the {} notation because it is for declaration for this to work we needed to add the blank constructor
see Book.cs   



*/
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookDb>();

    // Only add books if the database is currently empty
    if (!db.Books.Any())
    {
        db.Books.AddRange(
           new Book
           {
               Id = 1,
               Year = 2001,
               Title = "The Great Gatsby",
               Author = "James Url John",
               Category = "Fantasy",
               Status = Status.Reading
           },
            new Book
            {
                Id = 2,
                Year = 2001,
                Title = "Harry Potter",
                Author = "J.K Rowling",
                Category = "Fantasy",
                Status = Status.Finished
            }
        );
        db.SaveChanges();
    }
}

app.MapGet("/bookitems", async (BookDb db) =>
    await db.Books.ToListAsync());

app.MapGet("/bookitems/{id}", async (int id, BookDb db) =>
    await db.Books.FindAsync(id)
        is Book book // Checks if there is a book at the end of the GET req if there is assign the value to temp variable book
        ? Results.Ok(book) // These lines translate to did the book exist? if so return 200 (ok) else return 404 (not found)
        : Results.NotFound());




app.MapGet("/bookitems/Finished", async (BookDb db) =>
    await db.Books.Where(t => t.Status == Status.Finished).ToListAsync());


app.Run();