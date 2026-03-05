namespace WebAPI.Models
{
    public class UsersRosary
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RosaryId { get; set; }

        // Właściwości nawigacyjne - to one pozwalają na "ur.Rosary.Name"
        public virtual User User { get; set; }
        public virtual Rosary Rosary { get; set; }
    }
}
