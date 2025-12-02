using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShare.Models {
    public class User : IdentityUser {
        // NOWE: kolumna Username w DB była NOT NULL — dodajemy właściwość i walidację.
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        [Column("Username")]
        [StringLength(256, ErrorMessage = "Nazwa użytkownika nie może być dłuższa niż 256 znaków.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rola użytkownika jest wymagana.")]
        public UserRole Role { get; set; } = UserRole.Customer;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 1_000_000, ErrorMessage = "Saldo musi być większe lub równe 0.")]
        public decimal Balance { get; set; } = 0m;

        // Relacja 1:N z Order
        public List<Order> Orders { get; set; } = new();

        // Relacja M:N z Book przez UserBook
        public List<UserBook> UserBooks { get; set; } = new();
    }

}
