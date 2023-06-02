using ACS.Models;

namespace ACS.Repository
{
    public interface IDeviceRepository
    {
        IEnumerable<DeviceDTO> GetAllDevices(int page);
        IEnumerable<DeviceDTO> GetAllDevicesOfEmployee(int employeeId);
        ResultStatus AddDevice(DeviceDTO device);
        ResultStatus UpdateDevice(DeviceDTO device);
        ResultStatus DeleteDevice(string deviceId);
    }
}