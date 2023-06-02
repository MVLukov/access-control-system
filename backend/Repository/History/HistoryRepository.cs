using ACS.Models;

namespace ACS.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly ACSDbContext _dbContext;
        public HistoryRepository(ACSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<TagHistory> GetEmployeeHistory(int employeeId, int page, DateTime date)
        {
            try
            {
                if (page == 1)
                {
                    if (date == DateTime.MinValue)
                    {
                        return _dbContext.TagsHistory.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.Timestamp).Take(16).ToList();
                    }

                    return _dbContext.TagsHistory.Where(x => x.Timestamp.Date == date.Date && x.EmployeeId == employeeId).OrderByDescending(x => x.Timestamp).Take(16).ToList();
                }

                if (date == DateTime.MinValue)
                {
                    return _dbContext.TagsHistory.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.Timestamp).Skip((page - 1) * 15).Take(16).ToList();
                }

                return _dbContext.TagsHistory.Where(x => x.Timestamp.Date == date.Date && x.EmployeeId == employeeId).OrderByDescending(x => x.Timestamp).Skip((page - 1) * 16).Take(16).ToList();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public IEnumerable<TagHistory> GetHistory(int page, DateTime date)
        {
            try
            {
                if (page == 1)
                {
                    if (date == DateTime.MinValue)
                    {
                        return _dbContext.TagsHistory.OrderByDescending(x => x.Timestamp).Take(16).ToList();
                    }

                    return _dbContext.TagsHistory.Where(x => x.Timestamp.Date == date.Date).OrderByDescending(x => x.Timestamp).Take(16).ToList();
                }

                if (date == DateTime.MinValue)
                {
                    return _dbContext.TagsHistory.OrderByDescending(x => x.Timestamp).Skip((page - 1) * 15).Take(16).ToList();
                }


                return _dbContext.TagsHistory.Where(x => x.Timestamp.Date == date.Date).OrderByDescending(x => x.Timestamp).Skip((page - 1) * 16).Take(16).ToList();
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
