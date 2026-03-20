namespace WebAPI.Models
{
    public class RosaryMessages
    {
        public int Id { get; set; }
        public int RosaryId { get; set; }
        public string AuthorName { get; set; }
        public string MessageTitle { get; set; }
        public string MessageBody { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
