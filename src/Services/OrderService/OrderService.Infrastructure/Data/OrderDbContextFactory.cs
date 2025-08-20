using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrderService.Infrastructure.Data
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=OrderDb;Username=postgres;Password=yourpassword");

            return new OrderDbContext(optionsBuilder.Options);


        //    IConfigurationRoot configuration = new ConfigurationBuilder()
        //   .SetBasePath(Directory.GetCurrentDirectory())
        //   .AddJsonFile("appsettings.json")
        //   .Build();

        //var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();

        //// Get PostgreSQL connection string
        //var connectionString = configuration.GetConnectionString("OrderConnection");

        //optionsBuilder.UseNpgsql(connectionString);

        //return new OrderDbContext(optionsBuilder.Options);
    }
}
}
