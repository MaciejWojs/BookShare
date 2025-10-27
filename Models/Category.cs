namespace BookShare.Models {
    public class Category {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
