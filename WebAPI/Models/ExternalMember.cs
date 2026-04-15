namespace WebAPI.Models
{
    public class ExternalMember
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public int RosaryId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
