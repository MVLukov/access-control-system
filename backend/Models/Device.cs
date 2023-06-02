using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACS.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "varchar(4)")]
        public string DeviceId { get; set; }
        [Required]
        [Column(TypeName = "varchar(24)")]
        public string DeviceName { get; set; }
        public ICollection<Employee> EmployeesList { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}