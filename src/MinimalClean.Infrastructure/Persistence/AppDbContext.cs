using Microsoft.EntityFrameworkCore;
using MinimalClean.Domain.Orders;

namespace MinimalClean.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Idempotency.IdempotencyRecord> Idempotency => Set<Idempotency.IdempotencyRecord>();
}