using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShare.Models {
    public class Order {
        [Key]
        public int Id { get; set; }

        // musi być typu string, żeby pasowało do IdentityUser.Id (string)
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Numer zamówienia jest wymagany.")]
        [Range(1, int.MaxValue, ErrorMessage = "Numer zamówienia musi być większy niż 0.")]
        public int OrderNumber { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Kwota zamówienia jest wymagana.")]
        [Range(0.01, 1_000_000, ErrorMessage = "Kwota zamówienia musi być większa niż 0.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Status zamówienia jest wymagany.")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Relacja 1:N z OrderItem
        public List<OrderItem> OrderItems { get; set; } = new();

        // Nawigacja do użytkownika (relacja N:1)
        public User? User { get; set; }
    }
}
