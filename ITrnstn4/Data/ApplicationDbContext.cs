using ITrnstn4.Models;
using Microsoft.EntityFrameworkCore;

namespace ITrnstn4.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .Property(u => u.Nickname)
            .HasMaxLength(20)
            .IsRequired();

            modelBuilder.Entity<User>()
            .HasCheckConstraint("CK_User_Nickname_Length", "LEN(Nickname) >= 2");

            modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .HasMaxLength(255)
            .IsRequired();

            modelBuilder.Entity<User>()
            .HasCheckConstraint("CK_User_Password_Validation",
                "LEN(Password) >= 6 AND Password LIKE '%[A-Z]%' AND Password LIKE '%[a-z]%' AND Password LIKE '%[0-9]%'");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}
