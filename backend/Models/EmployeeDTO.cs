namespace ACS.Models
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public bool isAllowed { get; set; } = false;
        public string TagId { get; set; } = string.Empty;
        public List<DeviceDTO> Devices { get; set; } = new List<DeviceDTO>();
    }
}