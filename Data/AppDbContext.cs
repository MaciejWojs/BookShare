using BookShare.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookShare.Data {
    // AppDbContext musi dziedziczyć po IdentityDbContext<User>
    public class AppDbContext : IdentityDbContext<User> {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Rejestruj swoje encje tutaj
        public DbSet<Category> Categories { get; set; }

        // ...nowe: DbSet dla Order
        public DbSet<Order> Orders { get; set; }
        
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<UserBook> UserBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            // ...dodatkowa konfiguracja modelu jeśli potrzeba...

            // Jawne mapowanie relacji Order -> User, żeby EF użył właściwości UserId (string)
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.SetNull); // lub Cascade/Restrict zgodnie z domeną
            });

            // Konfiguracja relacji UserBook (Many-to-Many między User a Book)
            builder.Entity<UserBook>(entity =>
            {
                entity.HasKey(ub => ub.Id);

                // Unikalna kombinacja User + Book (jeden użytkownik nie może mieć duplikatu tej samej książki)
                entity.HasIndex(ub => new { ub.UserId, ub.BookId })
                      .IsUnique()
                      .HasDatabaseName("IX_UserBook_UserId_BookId");

                entity.HasOne(ub => ub.User)
                      .WithMany(u => u.UserBooks)
                      .HasForeignKey(ub => ub.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ub => ub.Book)
                      .WithMany(b => b.UserBooks)
                      .HasForeignKey(ub => ub.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}