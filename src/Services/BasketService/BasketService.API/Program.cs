using BasketService.Application.Interfaces;
using BasketService.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//    var configuration = builder.Configuration.GetConnectionString("Redis");
//    return ConnectionMultiplexer.Connect(configuration);
//});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
    configuration.AbortOnConnectFail = false; // prevents immediate failure
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8002); // Expose port 8000
});
var app = builder.Build();
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//    var conn = builder.Configuration["Redis:ConnectionString"]
//               ?? builder.Configuration["Redis__ConnectionString"] // from env
//               ?? "localhost:6379";
//    var cfg = ConfigurationOptions.Parse(conn, true);
//    return ConnectionMultiplexer.Connect(cfg);
//});

//builder.Services.AddScoped<IDatabase>(sp =>
//    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());


////builder.Services.AddDbContext<BasketDbContext>(options =>
////    options.UseNpgsql(builder.Configuration.GetConnectionString("BasketDb")));
////builder.Services.AddScoped<IBasketRepository, BasketRepository>();
////// Redis connection
////builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
////    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));

//builder.Services.AddScoped<IBasketRepository, BasketRepository>();
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(8002); // Expose port 8000
//});
//var app = builder.Build();
//app.UseCors("AllowAll");
//app.UseSwagger();
//app.UseSwaggerUI();
////using (var scope = app.Services.CreateScope())
////{
////    var db = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
////    db.Database.Migrate();
////}
//app.UseAuthorization();
//app.MapControllers();
//app.Run();
