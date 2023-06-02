using System.ComponentModel.DataAnnotations;

namespace ACS
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DataType("varchar(24)")]
        [MaxLength(24)]
        public string Username { get; set; }
        [Required]
        [DataType("varchar(128)")]
        public string PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
    }
}