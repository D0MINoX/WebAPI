using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class Rosary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Column("Parish")]
        public int ParishValue { get; set; }
        [ForeignKey("ParishValue")]
        public virtual Parish Parish { get; set; }
        public virtual ICollection<UsersRosary> UsersRosary { get; set; }
    }
}
