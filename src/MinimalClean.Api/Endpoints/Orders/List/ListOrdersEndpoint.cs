using MinimalClean.Api.Common;

namespace MinimalClean.Api.Endpoints.Orders.List;

public class ListOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v{version:apiVersion}/orders", HandleAsync)
           .WithName("ListOrders")
           .Produces(200);
    }

    public async Task<IResult> HandleAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool? desc,
        ListOrdersHandler handler,
        CancellationToken ct)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;
        var result = await handler.Handle(new ListOrdersQuery(pageNumber, pageSize, sortBy, desc ?? false), ct);
        return Results.Ok(result);
    }
}