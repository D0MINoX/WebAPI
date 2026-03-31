using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class UserConsent
    {
        public int Id { get; set; }
        public int UserId { get; set; }
      
        public string ConsentType { get; set; }
   
        public string DocumentVersion { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; }
    }
}
