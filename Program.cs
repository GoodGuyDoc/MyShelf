using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

BookAdminService bookAdminService = new BookAdminService();


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

/// <summary>
/// Retrieves all books from the database.
/// </summary>
/// <returns>A list of all Book objects currently stored in the database.</returns>
app.MapGet("/bookitems", async (BookDb db) =>
    await db.Books.ToListAsync());

/// <summary>
/// Retrieves a specific book by its ID.
/// </summary>
/// <param name="id">The unique identifier of the book to retrieve.</param>
/// <returns>Returns the Book object if found (200 OK), otherwise returns 404 Not Found.</returns>
app.MapGet("/bookitems/{id}", async (int id, BookDb db) =>
    await db.Books.FindAsync(id)
        is Book book // Checks if there is a book at the end of the GET req if there is assign the value to temp variable book
        ? Results.Ok(book) // These lines translate to did the book exist? if so return 200 (ok) else return 404 (not found)
        : Results.NotFound());

/// <summary>
/// Retrieves all books with a status of 'Finished'.
/// </summary>
/// <returns>A list of all books where Status equals Status.Finished.</returns>
/// <note>This endpoint should be placed after more specific routes to avoid routing conflicts.</note>
app.MapGet("/bookitems/Finished", async (BookDb db) =>
    await db.Books.Where(t => t.Status == Status.Finished).ToListAsync());

/// <summary>
/// Creates a new book in the database.
/// </summary>
/// <param name="book">The Book object to be created. Should contain all required properties.</param>
/// <returns>Returns 201 Created with the new book and its location if successful, 409 Conflict on concurrency issues, or 500 Problem if a database error occurs.</returns>
app.MapPost("/bookitems", async (Book book, BookDb db) =>
{
    db.Books.Add(book);
    try
    {
        await db.SaveChangesAsync();
        return Results.Created($"/bookitems/{book.Id}", book);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict();
    }
    catch (DbUpdateException)
    {
        return Results.Problem("There was an issue while saving to the database");
    }
});


/// <summary>
/// Performs a full replacement update on an existing book. All properties of the book will be updated.
/// </summary>
/// <param name="id">The unique identifier of the book to update.</param>
/// <param name="inputBook">The new Book object with all updated values.</param>
/// <returns>Returns 204 No Content if successful, 404 Not Found if book doesn't exist, 409 Conflict on concurrency issues, or 500 Problem if a database error occurs.</returns>
/// <note>This is a full replacement operation. Use PATCH endpoint for partial updates (e.g., updating only the status).</note>
app.MapPut("/bookitems/{id}", async (int id, Book inputBook, BookDb db) => //Have to pass in the book (updated) as well as the id for the query
{

    //await db.Books.FindAsync(id) is Book book ? Results.Ok(book) : Results.NotFound(); Can not do this because it only works in expression context such as a lambda and return statement. The reason it works for get is because you are using it as a return.
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();
    book.Status = inputBook.Status;
    book.Title = inputBook.Title;
    book.Author = inputBook.Author;
    book.Year = inputBook.Year;
    book.Category = inputBook.Category;

    /* 
    Alternative approach:
    db.Entry(book).CurrentValues.SetValues(inputBook); this uses the EF system to easily update all values and is less verbose
    */

    try
    {
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict();
    }
    catch (DbUpdateException)
    {
        return Results.Problem("There was an issue while saving to the database");
    }
}
);


/// <summary>
/// Performs a partial update on an existing book. Only provided fields in the DTO will be updated.
/// </summary>
/// <param name="id">The unique identifier of the book to update.</param>
/// <param name="bookPatchDTO">A GeneralBookPatchDto object containing only the fields to be updated. Null/empty values are ignored.</param>
/// <returns>Returns 204 No Content if successful, 404 Not Found if book doesn't exist, 409 Conflict on concurrency issues, or 500 Problem if a database error occurs.</returns>
/// <note>Use this endpoint for partial updates such as changing just the status or category without affecting other properties.</note>
app.MapPatch("/bookitems/{id}", async (int id, GeneralBookPatchDto bookPatchDTO, BookDb db) => //Have to pass in the book (updated) as well as the id for the query
{

    //await db.Books.FindAsync(id) is Book book ? Results.Ok(book) : Results.NotFound(); Can not do this because it only works in expression context such as a lambda and return statement. The reason it works for get is because you are using it as a return.
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();
    if (!string.IsNullOrEmpty(bookPatchDTO.Category))
    {
        book.Category = bookPatchDTO.Category;
    }

    if (bookPatchDTO.Status.HasValue)
    {
        book.Status = bookPatchDTO.Status.Value; // The reason we have to use .Value is because the DTO class property is nullable and so we have to extract the actual value out.
    }

    try
    {
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict();
    }
    catch (DbUpdateException)
    {
        return Results.Problem("There was an issue while saving to the database");
    }
}
);

/// <summary>
/// Deletes a book from the database by its ID.
/// </summary>
/// <param name="id">The unique identifier of the book to delete.</param>
/// <returns>Returns 204 No Content if successful, 404 Not Found if book doesn't exist, 409 Conflict on concurrency issues, or 500 Problem if a database error occurs.</returns>
app.MapDelete("/bookitems/{id}", async (int id, BookDb db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();

    try
    {
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DBConcurrencyException)
    {
        return Results.Conflict();
    }
    catch (DbUpdateException)
    {
        return Results.Problem("There was an issue deleting this entry.");
    }
});


app.Run();