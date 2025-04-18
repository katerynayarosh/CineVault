﻿namespace CineVault.API.Entities;

public sealed class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Review> Reviews { get; set; } = [];
    // TODO 10 Налаштувати підтримку "м'якого видалення" (Soft Delete) для сутностей. Додати поле IsDeleted (property) до кожної сутності
    public bool IsDeleted { get; set; }
}