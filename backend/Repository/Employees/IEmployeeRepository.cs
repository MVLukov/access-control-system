using ACS.Models;

namespace ACS.Repository
{
    public interface IEmployeeRepository
    {
        IEnumerable<EmployeeDTO> GetAllEmployees(int page);
        IEnumerable<EmployeeDTO> SearchEmployee(string employeeName);
        ResultStatus AddEmployee(EmployeeDTO employee);
        ResultStatus UpdateEmployee(EmployeeDTO employee);
        ResultStatus DeleteEmployee(int employeeId);
        void ToggleEmployeeAccess(int employeeId);
    }
}