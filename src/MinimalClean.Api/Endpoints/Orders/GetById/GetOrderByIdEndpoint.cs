using MinimalClean.Api.Common;

namespace MinimalClean.Api.Endpoints.Orders.GetById;

public class GetOrderByIdEndpoint : EndpointBase
{
    public static string Pattern => "/orders/{id:guid}";
    public static Delegate Handler =>
        async (Guid id, GetOrderByIdHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetOrderByIdQuery(id), ct);
            return result is null ? NotFound($"Order {id} not found.") : Ok(result);
        };
}