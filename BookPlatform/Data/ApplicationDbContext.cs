using Microsoft.EntityFrameworkCore;
using BookPlatform.Models;

namespace BookPlatform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<BadWord> BadWords { get; set; }
        public DbSet<ReadingProgress> ReadingProgresses { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Nickname).IsUnique();
                entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasColumnType("varchar(50)");
                entity.Property(e => e.RegistrationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Level).HasDefaultValue(1.0f);
            });

            // Book configuration with relationships
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnType("varchar(50)");
                entity.Property(e => e.Rating).HasDefaultValue(0f);
                entity.Property(e => e.DateWritten).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationship: Book -> Author (User)
                entity.HasOne(b => b.Author)
                    .WithMany(u => u.WrittenBooks)
                    .HasForeignKey(b => b.AuthorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship: Book -> Editor (User)
                entity.HasOne(b => b.Editor)
                    .WithMany(u => u.EditedBooks)
                    .HasForeignKey(b => b.EditorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PriceAtPurchase).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Book)
                    .WithMany(b => b.OrderItems)
                    .HasForeignKey(oi => oi.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BadWord configuration
            modelBuilder.Entity<BadWord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Word).IsUnique();
                entity.Property(e => e.AddedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(bw => bw.AddedByAdmin)
                    .WithMany(u => u.AddedBadWords)
                    .HasForeignKey(bw => bw.AddedByAdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ReadingProgress configuration (composite key)
            modelBuilder.Entity<ReadingProgress>(entity =>
            {
                entity.HasKey(rp => new { rp.UserId, rp.BookId });
                entity.Property(rp => rp.LastReadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(rp => rp.LastPage).HasDefaultValue(0);
                entity.Property(rp => rp.IsCompleted).HasDefaultValue(false);

                entity.HasOne(rp => rp.User)
                    .WithMany(u => u.ReadingProgresses)
                    .HasForeignKey(rp => rp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Book)
                    .WithMany(b => b.ReadingProgresses)
                    .HasForeignKey(rp => rp.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Recommendation configuration
            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GeneratedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.RelevanceScore).HasDefaultValue(0f);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Recommendations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.RecommendedBook)
                    .WithMany(b => b.Recommendations)
                    .HasForeignKey(r => r.RecommendedBookId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}