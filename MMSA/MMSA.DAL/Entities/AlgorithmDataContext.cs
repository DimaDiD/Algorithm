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

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<PageContent> PageContents { get; set; }

    public virtual DbSet<SubPage> SubPages { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ProgrammingTopic> ProgrammingTopics { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<ProgrammingLanguage> ProgrammingLanguages { get; set; }

    public virtual DbSet<CourseLanguage> CourseLanguages { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Course");

            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Pages)
                .WithOne(p => p.Course)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(new Course
            {
                Id = 1,
                Title = "Методи розробки алгоритмів",
                Slug = "metody-rozrobky-alhorytmiv",
                Description = "Задачі з поясненнями та готовими прикладами коду кількома мовами програмування.",
                IsPublished = true,
                CreatedAtUtc = new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc)
            });
        });

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

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercise");

            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasIndex(e => new { e.PageId, e.SubPageId });

            entity.HasMany(e => e.Questions)
                .WithOne(q => q.Exercise)
                .HasForeignKey(q => q.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Question");

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(e => e.Text)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(e => e.CodeLanguage).HasMaxLength(20);

            entity.HasMany(e => e.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.ToTable("QuestionOption");

            entity.Property(e => e.Text)
                .HasMaxLength(1000)
                .IsRequired();
        });

        modelBuilder.Entity<ProgrammingLanguage>(entity =>
        {
            entity.ToTable("ProgrammingLanguage");

            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.HighlightLanguage)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasData(
                new ProgrammingLanguage { Id = 1, Code = "csharp", Name = "C#", HighlightLanguage = "csharp" },
                new ProgrammingLanguage { Id = 2, Code = "cpp", Name = "C++", HighlightLanguage = "cpp" },
                new ProgrammingLanguage { Id = 3, Code = "pascal", Name = "Pascal", HighlightLanguage = "delphi" });
        });

        modelBuilder.Entity<CourseLanguage>(entity =>
        {
            entity.ToTable("CourseLanguage");

            entity.HasKey(e => new { e.CourseId, e.ProgrammingLanguageId });

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Languages)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProgrammingLanguage)
                .WithMany()
                .HasForeignKey(e => e.ProgrammingLanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
