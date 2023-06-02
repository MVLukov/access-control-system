using ACS.Models;
using Microsoft.EntityFrameworkCore;

namespace ACS
{
    public class ACSDbContext : DbContext
    {
        public DbSet<TagHistory> TagsHistory { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }

        public ACSDbContext(DbContextOptions<ACSDbContext> options) : base(options)
        {

        }
    }
}