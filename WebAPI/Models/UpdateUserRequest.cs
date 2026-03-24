namespace WebAPI.Models
{
    public class UpdateUserRequest
    {
        public int Id { get; set; }
        public int Role { get; set; }
        public bool CanSendSMS { get; set; }
    }
}
