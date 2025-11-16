using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinimalClean.Api.Common;
using MinimalClean.Api.Endpoints.Orders.Create;
using MinimalClean.Api.Endpoints.Orders.GetById;
using MinimalClean.Api.Middleware;
using MinimalClean.Infrastructure.Persistence;
using MinimalClean.Infrastructure.Persistence.Repositories;

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

builder.Services.AddScoped<IEndpoint, CreateOrderEndpoint>();
builder.Services.AddScoped<IEndpoint, GetOrderByIdEndpoint>();

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

// Map endpoints
using (var scope = app.Services.CreateScope())
{
    var endpoints = scope.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();
    foreach (var endpoint in endpoints)
    {
        endpoint.MapEndpoint(app);
    }
}

//// static approach
//// Route groups (optional, for versioning or grouping)
//var orders = app.MapGroup("/orders");

//// Create order
//orders.MapPost("", CreateOrderEndpoint.Handler)
//      .WithName("CreateOrder")
//      .Produces(201)
//      .Produces(400);

//// Get by id
//orders.MapGet("{id:guid}", GetOrderByIdEndpoint.Handler)
//      .WithName("GetOrderById")
//      .Produces(200)
//      .Produces(404);

app.Run();
