using ACS.Models;

namespace ACS.Repository
{
    public interface IHistoryRepository
    {
        IEnumerable<TagHistory> GetHistory(int page, DateTime date);
        IEnumerable<TagHistory> GetEmployeeHistory(int employeeId, int page, DateTime date);
    }
}