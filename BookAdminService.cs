using System.Data.Common;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// This class is used to separate the business logic of updating sensitive data (author and title) from the general book patching.
/// It will be used if the app goes from self-hosted to a multi-user environment where we want to have more control over who can update what data.
/// </summary>
internal class BookAdminService
{
    internal async Task<IResult> UpdateSensDataAsync(int id, InternalBookPatchDTO bookPatchDTO, BookDb db)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null)
        {
            return Results.NotFound();
        }
        if (!string.IsNullOrEmpty(bookPatchDTO.Author))
        {
            book.Author = bookPatchDTO.Author;
        }
        if (!string.IsNullOrEmpty(bookPatchDTO.Title))
        {
            book.Title = bookPatchDTO.Title;
        }

        try
        {
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (DbUpdateConcurrencyException) //This usually happens if the database has been modified since it was loaded into memory, essentially a race condition error
        {
            return Results.Conflict();
        }
        catch (DbUpdateException) //generic exception for all db update issues. 
        {
            return Results.Problem($"There was an issue while saving to the database");
        }

    }
}


