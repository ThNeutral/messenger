using Microsoft.EntityFrameworkCore;
using server.internals.dbMigrations.tables;

namespace server.internals.dbMigrations
{
    public class MessengerDBContext : DbContext
    {
        public MessengerDBContext _instance;
        public MessengerDBContext(DbContextOptions<MessengerDBContext> options): base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UserStatus)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(e => e.Token)
                    .WithOne(t => t.User)
                    .HasForeignKey<Token>(t => t.UserID);

                entity.HasOne(e => e.ProfilePicture)
                    .WithOne(pp => pp.User)
                    .HasForeignKey<ProfilePicture>(pp => pp.UserID);
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.JWToken).IsRequired() .HasMaxLength(100);
                entity.Property(e => e.ExpiresAt).IsRequired().HasColumnType("BigInt");
            });

            modelBuilder.Entity<ProfilePicture>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.Base64EncodedImage).IsRequired().HasColumnType("Text");
            });
        }

    }
}
