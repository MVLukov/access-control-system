using ACS.Models;
using Microsoft.EntityFrameworkCore;

namespace ACS.Repository
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ACSDbContext _dbContext;

        public DeviceRepository(ACSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<DeviceDTO> GetAllDevices(int page)
        {
            try
            {
                if (page == 1)
                {
                    return _dbContext.Devices.Include(x => x.EmployeesList).OrderBy(x => x.CreatedAt).Take(11).Select(x => ParseDevice(x)).ToList();
                }
                else if (page == 0)
                {
                    return _dbContext.Devices.Include(x => x.EmployeesList).OrderBy(x => x.CreatedAt).Select(x => ParseDevice(x)).ToList();
                }
                else
                {
                    return _dbContext.Devices.Include(x => x.EmployeesList).OrderBy(x => x.CreatedAt).Skip((page - 1) * 10).Take(11).Select(x => ParseDevice(x)).ToList();
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public IEnumerable<DeviceDTO> GetAllDevicesOfEmployee(int employeeId)
        {
            try
            {
                List<Device> getAllDevices = _dbContext.Devices.Include(x => x.EmployeesList).ToList();
                List<DeviceDTO> parsedDevices = new List<DeviceDTO>();

                getAllDevices.ForEach(x =>
                {
                    if (x.EmployeesList.FirstOrDefault(em => em.Id == employeeId) != null)
                    {
                        parsedDevices.Add(new DeviceDTO() { DeviceId = x.DeviceId, DeviceName = x.DeviceName, Selected = true });

                    }
                    else
                    {
                        parsedDevices.Add(new DeviceDTO() { DeviceId = x.DeviceId, DeviceName = x.DeviceName, Selected = false });
                    }

                });
                return parsedDevices;
            }
            catch (System.Exception)
            {

                throw;
            }
        }
        public ResultStatus AddDevice(DeviceDTO device)
        {
            try
            {
                Device getDeviceById = _dbContext.Devices.AsNoTracking().FirstOrDefault(x => x.DeviceId == device.DeviceId);
                Device getDeviceByName = _dbContext.Devices.AsNoTracking().FirstOrDefault(x => x.DeviceName == device.DeviceName);

                if (getDeviceById != null)
                {
                    return ResultStatus.DeviceIdAlreadyExists;
                }

                if (getDeviceByName != null)
                {
                    return ResultStatus.DeviceNameAlreadyExists;
                }

                _dbContext.Devices.Add(new Device() { DeviceId = device.DeviceId, DeviceName = device.DeviceName });
                _dbContext.SaveChanges();
                return ResultStatus.DeviceAdded;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ResultStatus UpdateDevice(DeviceDTO device)
        {
            try
            {
                Device getDeviceById = _dbContext.Devices.FirstOrDefault(x => x.DeviceId == device.DeviceId);
                Device getDeviceByName = _dbContext.Devices.FirstOrDefault(x => x.DeviceName == device.DeviceName);

                if (getDeviceById != null)
                {
                    if (getDeviceByName == null)
                    {
                        getDeviceById.DeviceName = device.DeviceName;
                        _dbContext.SaveChanges();

                        return ResultStatus.DeviceUpdated;
                    }
                    else
                    {
                        return ResultStatus.DeviceNameAlreadyExists;
                    }
                }

                return ResultStatus.Err;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ResultStatus DeleteDevice(string deviceId)
        {
            try
            {

                Device getDevice = _dbContext.Devices.FirstOrDefault(x => x.DeviceId == deviceId);

                if (getDevice != null)
                {
                    _dbContext.Devices.Remove(getDevice);
                    _dbContext.SaveChanges();
                    return ResultStatus.DeviceDeleted;
                }

                return ResultStatus.Err;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private static DeviceDTO ParseDevice(Device device)
        {
            return new DeviceDTO() { DeviceId = device.DeviceId, DeviceName = device.DeviceName, Selected = false };
        }
    }
}