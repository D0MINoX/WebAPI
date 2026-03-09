namespace WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname {  get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; } 
        public  int Role {  get; set; }
        public virtual ICollection<UsersRosary> UserRosaries { get; set; }

    }
}