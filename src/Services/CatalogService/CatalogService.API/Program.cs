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

app.UseSwagger();
app.UseSwaggerUI();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();


//docker build -f src/Services/CatalogService/CatalogService.Dockerfile -t catalogservice:dev.
//docker run -p 8000:8000 catalogservice: dev