namespace CineVault.API.Entities;

// TODO 4 Зв'язати коментарі з користувачами та відгуками
public sealed class Review
{
    public int Id { get; set; }
    public required int MovieId { get; set; }
    public required int UserId { get; set; }
    public required int Rating { get; set; }
    // TODO 4 Можливість ставити відгук без коментаря
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Movie? Movie { get; set; }
    public User? User { get; set; }
}