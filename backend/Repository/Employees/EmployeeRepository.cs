using ACS.Models;
using Microsoft.EntityFrameworkCore;

namespace ACS.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ACSDbContext _dbContext;

        public EmployeeRepository(ACSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ResultStatus AddEmployee(EmployeeDTO employee)
        {
            try
            {

                Employee getEmployee = _dbContext.Employees.FirstOrDefault(x => x.EmployeeName.ToLower() == employee.EmployeeName.ToLower());
                Employee newEmployee = new Employee();

                if (getEmployee == null)
                {
                    newEmployee.EmployeeName = employee.EmployeeName;

                    var getTag = _dbContext.Employees.FirstOrDefault(x => x.TagId == employee.TagId && x.TagId != null);
                    List<Device> devices = new List<Device>();

                    if (employee.Devices.Count > 0)
                    {
                        newEmployee.Devices = new List<Device>();

                        employee.Devices.ForEach(d =>
                        {
                            Device getDevice = _dbContext.Devices.FirstOrDefault(x => x.DeviceId == d.DeviceId);

                            if (d.Selected)
                            {
                                newEmployee.Devices.Add(getDevice);
                            }
                        });
                    }

                    if (getTag == null)
                    {
                        newEmployee.TagId = employee.TagId;
                    }
                    else
                    {
                        return ResultStatus.TagAlreadyExists;
                    }

                    _dbContext.Employees.Add(newEmployee);
                    _dbContext.SaveChanges();

                    return ResultStatus.EmployeeAdded;
                }
                else
                {
                    return ResultStatus.EmployeeAlreadyExists;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeDTO> GetAllEmployees(int page)
        {
            try
            {
                if (page == 1)
                {
                    return _dbContext.Employees.OrderBy(x => x.Id).Take(11).Select(x => ParseEmployee(x)).ToList(); ; ;
                }

                return _dbContext.Employees.OrderBy(x => x.Id).Skip((page - 1) * 10).Take(11).Select(x => ParseEmployee(x)).ToList(); ;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeDTO> SearchEmployee(string employeeName)
        {
            try
            {
                return _dbContext.Employees.Where(x => x.EmployeeName.ToLower().Contains(employeeName.ToLower())).Select(x => ParseEmployee(x)).ToList();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ResultStatus DeleteEmployee(int employeeId)
        {
            try
            {
                Employee getEmployee = _dbContext.Employees.AsNoTracking().FirstOrDefault(x => x.Id == employeeId);

                if (getEmployee != null)
                {
                    _dbContext.Employees.Remove(getEmployee);
                    _dbContext.SaveChanges();

                    return ResultStatus.EmployeeDeleted;
                }
                else
                {
                    return ResultStatus.Err;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public ResultStatus UpdateEmployee(EmployeeDTO employee)
        {
            try
            {
                Employee getEmployee = _dbContext.Employees.Include(x => x.Devices).FirstOrDefault(x => x.Id == employee.Id);

                if (getEmployee != null)
                {
                    Employee getEmployeeByName = _dbContext.Employees.AsNoTracking().FirstOrDefault(x => x.EmployeeName.ToLower() == employee.EmployeeName.ToLower() && x.Id != employee.Id);

                    if (employee.TagId == null)
                    {
                        getEmployee.TagId = null;
                    }
                    else
                    {
                        var getTag = _dbContext.Employees.AsNoTracking().FirstOrDefault(x => x.TagId == employee.TagId && x.Id != employee.Id);

                        if (getTag == null)
                        {
                            getEmployee.TagId = employee.TagId;
                        }
                        else
                        {
                            return ResultStatus.TagAlreadyExists;
                        }
                    }

                    if (employee.Devices.Count > 0)
                    {
                        employee.Devices.ForEach(d =>
                        {
                            Device getDevice = _dbContext.Devices.FirstOrDefault(x => x.DeviceId == d.DeviceId);

                            if (d.Selected)
                            {
                                getEmployee.Devices.Add(getDevice);
                            }
                            else
                            {
                                getEmployee.Devices.Remove(getDevice);
                            }
                        });
                    }

                    if (getEmployeeByName == null)
                    {
                        getEmployee.EmployeeName = employee.EmployeeName;
                    }
                    else
                    {
                        return ResultStatus.EmployeeAlreadyExists;
                    }

                    _dbContext.SaveChanges();
                    return ResultStatus.EmployeeUpdated;
                }

                return ResultStatus.Err;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public void ToggleEmployeeAccess(int employeeId)
        {
            try
            {
                Employee getEmployee = _dbContext.Employees.FirstOrDefault(x => x.Id == employeeId);

                if (getEmployee != null)
                {
                    getEmployee.isAllowed = !getEmployee.isAllowed;
                }

                _dbContext.SaveChanges();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private static EmployeeDTO ParseEmployee(Employee em)
        {
            return new EmployeeDTO() { Id = em.Id, EmployeeName = em.EmployeeName, TagId = em?.TagId, isAllowed = em.isAllowed };
        }
    }
}