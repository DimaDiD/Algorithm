using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MMSA.DAL.Entities;

public partial class AlgorithmDataContext : DbContext
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-98FH96A\\SQLEXPRESS01;Database=AlgorithmData;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
