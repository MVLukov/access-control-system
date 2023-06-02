using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACS.Models
{
    public class TagHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(8)]
        [Column(TypeName = "varchar(8)")]
        public string TagId { get; set; }
        [Required]
        [MaxLength(4)]
        [Column(TypeName = "varchar(4)")]
        public string DeviceId { get; set; }
        [Column(TypeName = "varchar(24)")]
        public string DeviceName { get; set; }
        [Column(TypeName = "varchar(24)")]
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public bool Declined { get; set; }
        [DataType(DataType.Date)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}