using Microsoft.EntityFrameworkCore;
using CerealApi.Models;

namespace CerealApi.Data
{
    public class CerealContext : DbContext
    {
        public CerealContext(DbContextOptions<CerealContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // Improves performance
        }

        public DbSet<Cereal> Cereals { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;


        // Optionally override OnModelCreating to configure constraints, indexes, etc.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // For example, enforce unique name or not null constraints if you wish:
            // modelBuilder.Entity<Cereal>()
            //    .HasIndex(c => c.Name)
            //    .IsUnique();
        }
    }
}
