using Bootstrap;
using Microsoft.EntityFrameworkCore;
using SQLServer;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new DirectoryNotFoundException("Assembly Location");

builder.Configuration.GetConfiguration(basePath)
    .AddJsonFile("appsettings.json", optional: false);

// Add services to the container.
builder.Services.AddControllers();

await builder.Services.AddAplicationAsync(builder.Configuration);
builder.Services.AddAllHealthChecks(builder.Configuration);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ensure database exists and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAngularApp");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Partial class declaration for WebApplicationFactory integration testing
public partial class Program { }
