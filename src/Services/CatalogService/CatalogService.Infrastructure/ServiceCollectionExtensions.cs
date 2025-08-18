using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var cs = config.GetConnectionString("CatalogDb") ?? "Data Source=catalog.db";
            services.AddDbContext<CatalogDbContext>(opt => opt.UseNpgsql(cs));
            services.AddScoped<IProductRepository, ProductRepository>();
            return services;
        }
    }
}
