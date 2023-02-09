using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HalykLife.Models;

public partial class TestContext : DbContext
{
    public TestContext()
    {
    }
    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RCurrency> RCurrencies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().
            SetBasePath(AppDomain.CurrentDomain.BaseDirectory).
            AddJsonFile("appsettings.json").
            Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Test"));

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RCurrency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__R_CURREN__3214EC27AE62F65E");

            entity.ToTable("R_CURRENCY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ADate)
                .HasColumnType("date")
                .HasColumnName("A_DATE");
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CODE");
            entity.Property(e => e.Title)
                .HasMaxLength(60)
                .IsUnicode(false)
                .UseCollation("Cyrillic_General_CI_AS")
                .HasColumnName("TITLE");
            entity.Property(e => e.Value)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("VALUE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
