using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using server.internals.dbMigrations.tables;

namespace server.internals.dbMigrations
{
    public class MessengerDBContext : DbContext
    {
        public MessengerDBContext _instance;
        public MessengerDBContext(DbContextOptions<MessengerDBContext> options): base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<ProfilePicture> ProfilePictures { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatToUser> ChatToUsers { get; set; }

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

                entity.HasOne(e => e.Token)
                    .WithOne(t => t.User)
                    .HasForeignKey<Token>(t => t.UserID);

                entity.HasOne(e => e.ProfilePicture)
                    .WithOne(pp => pp.User)
                    .HasForeignKey<ProfilePicture>(pp => pp.UserID);

                entity.HasMany(e => e.Messages)
                    .WithOne(t => t.User)
                    .HasForeignKey(m =>  m.UserID);
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

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(e => e.ChatID);
                entity.Property(e => e.ChatName).IsRequired().HasMaxLength(100);
                entity.HasMany(e => e.Messages)
                      .WithOne(t => t.Chat)
                      .HasForeignKey(m => m.ChatID);
            });

            modelBuilder.Entity<ChatToUser>(entity =>
            {
                entity.HasKey(e => new { e.ChatID, e.UserID });
                entity.HasOne(e => e.User).WithMany(u => u.ChatsToUsers).HasForeignKey(e => e.UserID);
                entity.HasOne(e => e.Chat).WithMany(c => c.ChatsToUsers).HasForeignKey(e => e.ChatID);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageID);
                entity.Property(e => e.SendTime).IsRequired().HasColumnType("BigInt");
                entity.Property(e => e.Content).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsRedirect).IsRequired();
            });

            modelBuilder.Entity<WatchedBy>(entity =>
            {
                entity.HasKey(e => new { e.MessageID, e.UserID });
                entity.HasOne(e => e.User).WithMany(u => u.WatchedBies).HasForeignKey(e => e.UserID);
                entity.HasOne(e => e.Message).WithMany(m => m.WatchedBies).HasForeignKey(e => e.MessageID);
            });
        }

    }
}
