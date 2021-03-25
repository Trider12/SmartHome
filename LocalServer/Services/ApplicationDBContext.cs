using LocalServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalServer.Services
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        { }

        public DbSet<TemperatureData> Data { get; set; }
    }
}