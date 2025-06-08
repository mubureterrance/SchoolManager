using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SchoolManager.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SchoolManagementDbContext>
    {
        public SchoolManagementDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<SchoolManagementDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new SchoolManagementDbContext(builder.Options);
        }
    }
}
