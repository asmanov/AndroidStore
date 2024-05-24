namespace AndroidStore.Models
{
    public record User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; } = null;
        public string? Password { get; set; }
        public string? Username { get; set;}
    }
}
