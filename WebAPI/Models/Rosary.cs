namespace WebAPI.Models
{
    public class Rosary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Parish { get; set; }
        public virtual ICollection<UsersRosary> UsersRosary { get; set; }
    }
}
