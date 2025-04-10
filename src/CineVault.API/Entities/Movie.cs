namespace CineVault.API.Entities;

public sealed class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Actor> Actors { get; set; } = [];
    // TODO 10 Налаштувати підтримку "м'якого видалення" (Soft Delete) для сутностей. Додати поле IsDeleted (property) до кожної сутності
    public bool IsDeleted { get; set; }
}