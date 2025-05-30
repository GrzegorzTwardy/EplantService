using EShopDomain.Models;
using Microsoft.EntityFrameworkCore;
using UserDomain.Models;

namespace EShopDomain.Repositories;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        // Ensure database is created
        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure soft delete filter
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.Deleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.Deleted);
        modelBuilder.Entity<User>().HasQueryFilter(p => !p.Deleted);

        // Configure tables and relationships
        modelBuilder.Entity<Category>().ToTable("Categories");
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<User>().ToTable("Users");

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category);
    }
}