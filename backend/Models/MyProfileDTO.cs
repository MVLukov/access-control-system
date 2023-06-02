using System.ComponentModel.DataAnnotations;

namespace ACS
{
    public class MyProfileDTO
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}