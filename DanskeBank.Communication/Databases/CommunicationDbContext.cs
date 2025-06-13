using Microsoft.EntityFrameworkCore;
using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Databases;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerModel> Customers { get; set; }
    public DbSet<TemplateModel> Templates { get; set; }
    public DbSet<UserModel> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerModel>().HasIndex(c => c.Id).IsUnique();
        modelBuilder.Entity<TemplateModel>().HasIndex(t => t.Id).IsUnique();
        modelBuilder.Entity<UserModel>().HasIndex(u => u.Id).IsUnique();
    }
}