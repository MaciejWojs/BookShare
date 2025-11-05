using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShare.Models {
    public class Category {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa kategorii jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa kategorii nie może być dłuższa niż 100 znaków.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Opis kategorii nie może być dłuższy niż 500 znaków.")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Relacja 1:N z Book
        public List<Book> Books { get; set; } = new();
    }
}
