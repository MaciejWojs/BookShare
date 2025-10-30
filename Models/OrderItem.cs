using System;
using System.ComponentModel.DataAnnotations;

namespace BookShare.Models {
    public class OrderItem {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Id zamówienia jest wymagane.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Id książki jest wymagane.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Ilość jest wymagana.")]
        [Range(1, 1000, ErrorMessage = "Ilość musi być większa od zera.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Cena jednostkowa jest wymagana.")]
        [Range(0.01, 9999.99, ErrorMessage = "Cena jednostkowa musi być większa od 0.")]
        public decimal UnitPrice { get; set; }

        // 🔗 Nawigacje
        public Order? Order { get; set; }
        public Book? Book { get; set; }

        // 💰 Właściwość pomocnicza (nie zapisywana w DB)s
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
