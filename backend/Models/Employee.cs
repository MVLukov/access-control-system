using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(24)]
        [Column(TypeName = "varchar(24)")]
        public string EmployeeName { get; set; }
        public bool isAllowed { get; set; }
        [MaxLength(8)]
        [Column(TypeName = "varchar(8)")]
        public string TagId { get; set; }
        public ICollection<Device> Devices { get; set; }
        [Timestamp]
        public DateTime CreatedAt { get; set; }
    }
}