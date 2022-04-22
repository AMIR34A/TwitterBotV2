namespace DataLayer.Models
{
    public class Information
    {
        public int Id { get; set; }
        public long? ChatIdChannel { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
