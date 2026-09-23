using Microsoft.EntityFrameworkCore;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> Users => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>()
            .HasKey(user => user.UserId); 

        modelBuilder.Entity<UserAccount>()
            .HasIndex(user => user.Username)
            .IsUnique();
    }
}