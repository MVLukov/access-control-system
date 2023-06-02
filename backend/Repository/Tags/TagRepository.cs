using ACS.Hubs;
using ACS.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ACS.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly ACSDbContext _dbContext;
        private IHubContext<TagHub> _hub;
        public TagRepository(ACSDbContext dbContext, IHubContext<TagHub> hub)
        {
            _dbContext = dbContext;
            _hub = hub;
        }

        public ResultStatus AuthorizeTag(string tagId, string deviceId)
        {
            try
            {
                Employee getEmployeeByTagId = _dbContext.Employees.Include((x => x.Devices)).FirstOrDefault(x => x.TagId == tagId);

                if (getEmployeeByTagId == null)
                {
                    Device getDevice = _dbContext.Devices.AsNoTracking().FirstOrDefault(x => x.DeviceId == deviceId);

                    _dbContext.TagsHistory.Add(new TagHistory() { TagId = tagId, Declined = true, DeviceId = deviceId, DeviceName = getDevice?.DeviceName });
                    _dbContext.SaveChanges();
                    _hub.Clients.All.SendAsync("tagRead", tagId);

                    return ResultStatus.TagDenied;
                }
                else
                {
                    // Device getDeviceOfEmployee = getEmployeeByTagId.Devices.FirstOrDefault(x => x.DeviceId == deviceId);
                    Device getDevice = _dbContext.Devices.Include(x => x.EmployeesList).AsNoTracking().FirstOrDefault(x => x.DeviceId == deviceId);

                    if (getEmployeeByTagId.isAllowed && getDevice?.EmployeesList.FirstOrDefault(x => x.Id == getEmployeeByTagId.Id) != null)
                    {
                        _dbContext.TagsHistory.Add(new TagHistory() { TagId = tagId, Declined = false, DeviceId = deviceId, DeviceName = getDevice.DeviceName, EmployeeName = getEmployeeByTagId.EmployeeName, EmployeeId = getEmployeeByTagId.Id });
                        _dbContext.SaveChanges();
                        _hub.Clients.All.SendAsync("tagRead", tagId);

                        return ResultStatus.TagAuthorized;
                    }
                    else
                    {
                        _dbContext.TagsHistory.Add(new TagHistory() { TagId = tagId, Declined = true, DeviceId = deviceId, DeviceName = getDevice?.DeviceName, EmployeeName = getEmployeeByTagId.EmployeeName, EmployeeId = getEmployeeByTagId.Id });
                        _dbContext.SaveChanges();
                        _hub.Clients.All.SendAsync("tagRead", tagId);

                        return ResultStatus.TagDenied;
                    }
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}