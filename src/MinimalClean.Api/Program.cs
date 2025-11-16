using FluentValidation;
using MinimalClean.Api.Endpoints.Orders.Create;
using MinimalClean.Api.Endpoints.Orders.GetById;
using MinimalClean.Api.Middleware;
using MinimalClean.Infrastructure.Persistence;
using MinimalClean.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext (replace with your connection string)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("AppDb")); // Use SQL Server or PostgreSQL in real apps

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Handlers
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();

// Validators
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderValidator>();

// Middleware
builder.Services.AddTransient<ErrorHandlingMiddleware>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Route groups (optional, for versioning or grouping)
var orders = app.MapGroup("/orders");

// Create order
orders.MapPost("", CreateOrderEndpoint.Handler)
      .WithName("CreateOrder")
      .Produces(201)
      .Produces(400);

// Get by id
orders.MapGet("{id:guid}", GetOrderByIdEndpoint.Handler)
      .WithName("GetOrderById")
      .Produces(200)
      .Produces(404);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
