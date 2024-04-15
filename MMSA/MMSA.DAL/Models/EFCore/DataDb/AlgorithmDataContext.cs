using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MMSA.DAL.Models.EFCore.DataDb;

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

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Page>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Page");

            entity.Property(e => e.PageName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
