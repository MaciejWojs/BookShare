using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShare.Models
{
    public class UserBook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int BookId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset AcquiredAt { get; set; } = DateTimeOffset.UtcNow;

        [StringLength(500, ErrorMessage = "Uwagi nie mogą być dłuższe niż 500 znaków.")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("BookId")]
        public Book Book { get; set; } = null!;
    }
}