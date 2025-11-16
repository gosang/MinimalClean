using MinimalClean.Api.Common;

namespace MinimalClean.Api.Endpoints.Orders.GetById;

public class GetOrderByIdEndpoint : IEndpoint
{
    private readonly GetOrderByIdHandler _handler;
    private readonly ILogger<GetOrderByIdEndpoint> _logger;

    public GetOrderByIdEndpoint(GetOrderByIdHandler handler, ILogger<GetOrderByIdEndpoint> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:guid}", HandleAsync)
           .WithName("GetOrderById")
           .Produces(200)
           .Produces(404);
    }

    private async Task<IResult> HandleAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Fetching order {OrderId}", id);

        var result = await _handler.Handle(new GetOrderByIdQuery(id), ct);
        if (result is null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return Results.NotFound($"Order {id} not found.");
        }

        _logger.LogInformation("Order {OrderId} retrieved successfully", id);
        return Results.Ok(result);
    }
}

//// static approach
//public class GetOrderByIdEndpoint : EndpointBase
//{
//    public static string Pattern => "/orders/{id:guid}";
//    public static Delegate Handler =>
//        async (Guid id, GetOrderByIdHandler handler, CancellationToken ct) =>
//        {
//            var result = await handler.Handle(new GetOrderByIdQuery(id), ct);
//            return result is null ? NotFound($"Order {id} not found.") : Ok(result);
//        };
//}