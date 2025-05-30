using Microsoft.EntityFrameworkCore;
using UserDomain.Models;

namespace UserDomain.repositories;

public class UserDataContext : DbContext
{
    public UserDataContext(DbContextOptions<UserDataContext> options) : base(options)
    {
        // Ensure database is created
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure soft delete filter
        modelBuilder.Entity<User>().HasQueryFilter(p => !p.Deleted);

        modelBuilder.Entity<User>().ToTable("Users");
    }
}