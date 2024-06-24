using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SwiftUpdate.Models
{
    public class SwiftUpdateContext : DbContext
    {
        public SwiftUpdateContext(DbContextOptions<SwiftUpdateContext> options)
        : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<ApplicationModel> Applications { get; set; }
        public DbSet<SessionModel> Sessions { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity
            modelBuilder.Entity<UserModel>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<UserModel>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Configure the Application entity
            modelBuilder.Entity<ApplicationModel>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ApplicationModel>()
                .Property(a => a.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");


            modelBuilder.Entity<SessionModel>()
               .Property(a => a.CreatedAt)
               .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<SessionModel>()
                .Property(a => a.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
