using MinimalClean.Infrastructure.Persistence.Idempotency;

namespace MinimalClean.Api.Common;

public class IdempotencyFilter : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";
    private readonly IIdempotencyStore _store;
    private readonly ILogger<IdempotencyFilter> _logger;

    public IdempotencyFilter(IIdempotencyStore store, ILogger<IdempotencyFilter> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var key = ctx.HttpContext.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(key))
            return Results.BadRequest(new { error = $"Missing {HeaderName} header." });

        var existing = await _store.GetAsync(key, ctx.HttpContext.RequestAborted);
        if (existing is not null)
        {
            _logger.LogInformation("Idempotent replay for key {Key}", key);
            // Optionally return previously stored response. Here we return 200 with resource id.
            return Results.Ok(new { id = existing.ResultResourceId, replay = true });
        }

        var result = await next(ctx);

        // Capture resource id from Created(...) payload if present
        if (result is IResult r)
        {
            // naive extraction: Created returns anonymous { id }
            // you could wrap Created payload in a known type to read reliably
            // fallback: store just the key
            await _store.SaveAsync(new IdempotencyRecord { Key = key }, ctx.HttpContext.RequestAborted);
        }

        return result;
    }
}