namespace CineVault.API.Entities;

// TODO 5 Необхідно додати нову сутність Actor, яка буде представляти акторів у базі даних. Сутність повина мати поля FullName, BirthDate, Biography. Встановити зв’язок типу "багато до багатьох" між сутністю Movie (фільм) та новою сутністю Actor.
public sealed class Actor
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? Biography { get; set; }
    public ICollection<Movie> Movies { get; set; } = [];
    // TODO 10 Налаштувати підтримку "м'якого видалення" (Soft Delete) для сутностей. Додати поле IsDeleted (property) до кожної сутності
    public bool IsDeleted { get; set; }
}