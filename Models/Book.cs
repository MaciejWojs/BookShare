using System.ComponentModel.DataAnnotations;
using System;

namespace BookShare.Models {
    public class Book {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        [StringLength(200, ErrorMessage = "Tytuł nie może być dłuższy niż 200 znaków.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Autor jest wymagany.")]
        [StringLength(150, ErrorMessage = "Autor nie może być dłuższy niż 150 znaków.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "ISBN jest wymagany.")]
        [StringLength(20, ErrorMessage = "ISBN nie może być dłuższy niż 20 znaków.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "Nieprawidłowy format ISBN.")]
        public string ISBN { get; set; }

        [StringLength(1000, ErrorMessage = "Opis nie może być dłuższy niż 1000 znaków.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana.")]
        [Range(0.01, 9999.99, ErrorMessage = "Cena musi być w zakresie od 0.01 do 9999.99.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Liczba sztuk w magazynie jest wymagana.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stan magazynowy nie może być ujemny.")]
        public int StockQuantity { get; set; }

        public int? CategoryId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Relacja 1:N z OrderItem
        public List<OrderItem> OrderItems { get; set; } = new();

        // (opcjonalnie) Nawigacja do kategorii
        public Category? Category { get; set; }
    }
}
