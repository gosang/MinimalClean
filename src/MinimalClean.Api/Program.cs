using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinimalClean.Api.Common;
using MinimalClean.Api.Endpoints.Orders.Create;
using MinimalClean.Api.Endpoints.Orders.GetById;
using MinimalClean.Api.Endpoints.Orders.List;
using MinimalClean.Api.Middleware;
using MinimalClean.Application.Abstractions;
using MinimalClean.Application.Orders.Handlers;
using MinimalClean.Domain.Orders.Events;
using MinimalClean.Infrastructure.Events;
using MinimalClean.Infrastructure.Persistence;
using MinimalClean.Infrastructure.Persistence.Idempotency;
using MinimalClean.Infrastructure.Persistence.Inbox;
using MinimalClean.Infrastructure.Persistence.Outbox;
using MinimalClean.Infrastructure.Persistence.Repositories;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Versioning
//builder.Services.AddApiVersioning(o =>
//{
//    o.DefaultApiVersion = new ApiVersion(1, 0);
//    o.AssumeDefaultVersionWhenUnspecified = true;
//    o.ReportApiVersions = true;
//})
//.AddApiExplorer(o =>
//{
//    o.GroupNameFormat = "'v'VVV";
//    o.SubstituteApiVersionInUrl = true;
//});

builder.Services.AddSingleton(new ConnectionFactory
{
    HostName = "localhost",   // or your broker host
    UserName = "guest",
    Password = "guest"
});

// Resilience
builder.Services.AddResiliencePipeline("outboxPublisher", pipeline =>
{
    pipeline.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 5,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    });
});

// Infrastructure
// DbContext (replace with your connection string)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("AppDb")); // Use SQL Server or PostgreSQL in real apps

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddSingleton<DomainEventDispatcher>();

// Handlers
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddScoped<ListOrdersHandler>();

// Validators
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderValidator>();

// Filters
builder.Services.AddScoped<ValidationFilter<CreateOrderRequest>>();
builder.Services.AddScoped<IdempotencyFilter>();

// Middleware
builder.Services.AddTransient<ErrorHandlingMiddleware>();

// Endpoints (stateless singletons)
//builder.Services.AddSingleton<IEndpoint, CreateOrderEndpoint>();
//builder.Services.AddSingleton<IEndpoint, GetOrderByIdEndpoint>();
//builder.Services.AddSingleton<IEndpoint, ListOrdersEndpoint>();
builder.Services.AddScoped<IEndpoint, CreateOrderEndpoint>();
builder.Services.AddScoped<IEndpoint, GetOrderByIdEndpoint>();
builder.Services.AddScoped<IEndpoint, ListOrdersEndpoint>();

// Domain event handlers
//builder.Services.AddScoped<OrderCreatedHandler>();
//builder.Services.AddScoped<IDomainEventHandler<OrderCreated>, OrderCreatedHandler>();
//builder.Services.AddScoped(typeof(IDomainEventHandler<>), typeof(OrderCreatedHandler)); // register concrete handlers explicitly

var assembly = typeof(OrderCreatedHandler).Assembly;

builder.Services.Scan(scan => scan
    .FromAssemblies(assembly)
    .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Outbox
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<OutboxCleanupWorker>();

// Inbox
builder.Services.AddHostedService<InboxConsumer>();
builder.Services.AddHostedService<InboxCleanupWorker>();

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

//var versionSet = app.NewApiVersionSet()
//    .HasApiVersion(1.0)
//    .Build();

//var api = app.MapGroup("/api/v{version:apiVersion}")
//             .WithApiVersionSet(versionSet)
//             .MapToApiVersion(1.0);

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
