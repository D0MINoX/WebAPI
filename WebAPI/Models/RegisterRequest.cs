namespace WebAPI.Models
{
    public class RegisterRequest
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Username { get; set; }
        public required string Password{ get; set; }
        public  int Role { get; set; }
    }
}
