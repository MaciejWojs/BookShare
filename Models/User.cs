namespace BookShare.Models {
    public class User {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Balance { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
