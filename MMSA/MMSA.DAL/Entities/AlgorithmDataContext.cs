using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MMSA.DAL.Entities;

public partial class AlgorithmDataContext : IdentityDbContext<ApplicationUser>
{
    public AlgorithmDataContext()
    {
    }

    public AlgorithmDataContext(DbContextOptions<AlgorithmDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<PageContent> PageContents { get; set; }

    public virtual DbSet<SubPage> SubPages { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ProgrammingTopic> ProgrammingTopics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("Page");

            entity.Property(e => e.PageName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PageContent>(entity =>
        {
            entity.ToTable("PageContent");

            entity.Property(e => e.CodeType)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Text).IsUnicode(false);
            entity.Property(e => e.TextType)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SubPage>(entity =>
        {
            entity.ToTable("SubPage");

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Page).WithMany(p => p.SubPages)
                .HasForeignKey(d => d.PageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubPage_Page");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.Property(t => t.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.HasIndex(t => t.FamilyId);

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgrammingTopic>(entity =>
        {
            entity.ToTable("ProgrammingTopic");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasData(
                new ProgrammingTopic { Id = 1, Name = "Змінні та типи даних" },
                new ProgrammingTopic { Id = 2, Name = "Введення та виведення даних" },
                new ProgrammingTopic { Id = 3, Name = "Арифметичні операції" },
                new ProgrammingTopic { Id = 4, Name = "Оператори порівняння" },
                new ProgrammingTopic { Id = 5, Name = "Логічні оператори" },
                new ProgrammingTopic { Id = 6, Name = "Умовний оператор if / else" },
                new ProgrammingTopic { Id = 7, Name = "Оператор вибору switch / case" },
                new ProgrammingTopic { Id = 8, Name = "Цикл for" },
                new ProgrammingTopic { Id = 9, Name = "Цикл while" },
                new ProgrammingTopic { Id = 10, Name = "Масиви" },
                new ProgrammingTopic { Id = 11, Name = "Рядки" },
                new ProgrammingTopic { Id = 12, Name = "Функції та процедури" },
                new ProgrammingTopic { Id = 13, Name = "Рекурсія" },
                new ProgrammingTopic { Id = 14, Name = "Анонімні функції" });
        });

        modelBuilder.Entity<PageContent>()
            .HasMany(content => content.Topics)
            .WithMany(topic => topic.PageContents)
            .UsingEntity<Dictionary<string, object>>(
                "PageContentTopic",
                right => right.HasOne<ProgrammingTopic>().WithMany().HasForeignKey("ProgrammingTopicId"),
                left => left.HasOne<PageContent>().WithMany().HasForeignKey("PageContentId"));

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
