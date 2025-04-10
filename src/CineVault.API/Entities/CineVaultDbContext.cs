using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Entities;

public sealed class CineVaultDbContext : DbContext
{
    public required DbSet<Movie> Movies { get; set; }
    public required DbSet<Review> Reviews { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<Like> Likes { get; set; }
    public required DbSet<Actor> Actors { get; set; }

    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // TODO 12 Увімкнути логування SQL-запитів у DbContext для дебагінгу та перевірки правильності запитів. Включити детальне логування (EnableSensitiveDataLogging).
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO 4 Зробити три зміни на власний розсуд в структурі бази даних та створити міграцію
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.CreatedAt).HasColumnName("Created_At");
        });
        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.CreatedAt).HasColumnName("Created_At");
        });
        modelBuilder.Entity<Like>(entity =>
        {
            entity.Property(l => l.ReviewId).HasColumnName("Review_ID");
        });
        modelBuilder.Entity<Like>(entity =>
        {
            entity.Property(l => l.UserId).HasColumnName("User_ID");
        });
        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.UserId).HasColumnName("User_ID");
        });

        // TODO 6 Додати необхідні сутності (Entity, якщо необхідно та налаштувати зв’язки між ними), тобто для сутностей з відсутніми звязками з іншими сутностями
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // TODO 7 Додати обмеження для унікального поля, щоб у базі даних не зберігалися дублікати (наприклад, назв фільмів, поєднання актор-фільм та в інших логічних випадках). Описати чому вони повинні бути унікальними
        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.Title)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // TODO 8 Задати логічну та достатню довжину полів, використовуючи Fluent API
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.Property(a => a.FullName).HasMaxLength(100);
            entity.Property(a => a.Biography).HasMaxLength(1000);
        });
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(u => u.Title).HasMaxLength(100);
            entity.Property(u => u.Description).HasMaxLength(1000);
            entity.Property(u => u.Genre).HasMaxLength(100);
            entity.Property(u => u.Director).HasMaxLength(100);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).HasMaxLength(100);
            entity.Property(u => u.Email).HasMaxLength(100);
            entity.Property(u => u.Password).HasMaxLength(100);
        });
        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Comment).HasMaxLength(1000);
        });

        // TODO 10 Налаштувати фільтри на рівні DbContext, щоб виключати видалені записи з запитів
        modelBuilder.Entity<Actor>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Movie>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Like>().HasQueryFilter(l => !l.IsDeleted);
    }
}