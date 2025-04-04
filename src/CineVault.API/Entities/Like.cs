namespace CineVault.API.Entities;

// TODO 5 Додати можливість лайкати коментарі тільки зареєстрованим користувачам
public sealed class Like
{
    public int Id { get; set; }
    public required int ReviewId { get; set; }
    public required int UserId { get; set; }
    public Review? Review { get; set; }
    public User? User { get; set; }
}