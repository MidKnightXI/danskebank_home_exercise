using Microsoft.EntityFrameworkCore;
using DanskeBank.Communication.Databases.Entities;

namespace DanskeBank.Communication.Databases;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<TemplateEntity> Templates { get; set; }
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerEntity>().HasIndex(c => c.Id).IsUnique();
        modelBuilder.Entity<TemplateEntity>().HasIndex(t => t.Id).IsUnique();
        modelBuilder.Entity<UserEntity>().HasIndex(u => u.Id).IsUnique();
        modelBuilder.Entity<UserEntity>().HasIndex(u => u.Email).IsUnique();
    }
}