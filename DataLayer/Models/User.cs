using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models
{
    public class User
    {
        public int Id { get; set; }
        [Index("IX_ChatIdUnique", IsUnique = true)]
        public long ChatId { get; set; }
        [MaxLength(32)]
        [Column(TypeName = "varchar")]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? LastUsing { get; set; }
        public UserStep Step { get; set; }
        public ICollection<Information> Informations { get; set; }
    }
}
