using OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace OrderService.API
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            db.Database.Migrate(); // Applies any pending migrations
        }
    }
}
