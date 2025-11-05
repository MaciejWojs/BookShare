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

        public DbSet<Book> Books { get; set; }

        // protected override void OnModelCreating(ModelBuilder builder) {
        //     base.OnModelCreating(builder);
        //     // ...dodatkowa konfiguracja modelu jeśli potrzeba...

        //     // Jawne mapowanie relacji Order -> User, żeby EF użył właściwości UserId (string)
        //     builder.Entity<Order>(entity =>
        //     {
        //         entity.HasKey(o => o.Id);

        //         entity.HasOne(o => o.User)
        //               .WithMany() // jeśli masz kolekcję Orders w User -> .WithMany(u => u.Orders)
        //               .HasForeignKey(o => o.UserId)
        //               .OnDelete(DeleteBehavior.SetNull); // lub Cascade/Restrict zgodnie z domeną
        //     });
        // }
    }
}