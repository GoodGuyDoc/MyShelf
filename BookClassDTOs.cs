
/// <summary>
/// Represents a safe/general book change for patching books on the system. 
/// Contains information such as Category and Status of the book.
/// Author + Title has been delegated to a separate DTO class as it is a business decision rather
/// than a technical decision
/// </summary>
public class GeneralBookPatchDto
{
    public string? Category { get; set; }
    public Status? Status { get; set; }
}

/// <summary>
/// Used to update author or title for a book separated business logic and will have a un-exposed endpoint (internal)
/// </summary>
internal class InternalBookPatchDTO
{
    public string? Author { get; set; }
    public string? Title { get; set; }
}