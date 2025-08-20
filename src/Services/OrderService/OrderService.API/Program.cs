using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Application.OrderService;
//using OrderService.Application.Services;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// PostgreSQL Connection
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb")));

// Dependency Injection
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderServiceApp>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8001); // Expose port 8000
});



var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
//app.Services.CreateScope().ServiceProvider
//   .GetRequiredService<OrderDbContext>()
//   .Database.Migrate();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate(); // apply migrations
}

//app.MapHealthChecks("/health");


//app.UseSwagger();
//app.UseSwaggerUI();
app.MapControllers();
app.Run();
