using System;
using System.Net.Sockets;
using AutoMapper;
using CatalogService.Application;
using CatalogService.Application.Products;
using CatalogService.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
//using FluentValidation.AspNetCore;

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

builder.Services.AddAutoMapper(typeof(ProductProfile));
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));

//builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddScoped<IProductAppService, ProductAppService>();

builder.Services.AddHealthChecks();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8000); // Expose port 8000
});


var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate(); // apply migrations
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();


//docker build -f src/Services/CatalogService/CatalogService.Dockerfile -t catalogservice:dev.
//docker run -p 8000:8000 catalogservice: dev
//dotnet ef migrations add InitialCreate --project "C:\Users\abhay_nigam\MicroservicesSolution\src\Services\CatalogService\CatalogService.Infrastructure\CatalogService.Infrastructure.csproj" --startup-project "C:\Users\abhay_nigam\MicroservicesSolution\src\Services\CatalogService\CatalogService.API\CatalogService.API.csproj"
//docker-compose up --build


//dotnet ef migrations add InitialCreate --project "C:\Users\abhay_nigam\MicroservicesSolution\src\Services\OrderService\OrderService.Infrastructure\OrderService.Infrastructure.csproj" --startup-project "C:\Users\abhay_nigam\MicroservicesSolution\src\Services\OrderService\OrderService.API\OrderService.API.csproj"