namespace WebAPI.Models
{
    public class ErrorReport
    {
        public int Id { get; set; }
        public string? UserPhone { get; set; }
        public string ErrorMessage { get; set; }
        public string Status { get; set; } = "Nowe";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}