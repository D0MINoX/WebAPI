namespace WebAPI.Models
{
    public class ExternalMember
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public int RosaryId { get; set; }
    }
}
