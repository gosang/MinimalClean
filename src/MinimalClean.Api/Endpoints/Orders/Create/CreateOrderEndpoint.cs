using FluentValidation;
using MinimalClean.Api.Common;

namespace MinimalClean.Api.Endpoints.Orders.Create;

public class CreateOrderEndpoint : EndpointBase
{
    public static string Pattern => "/orders";
    public static Delegate Handler =>
        async (CreateOrderRequest req,
               IValidator<CreateOrderRequest> validator,
               CreateOrderHandler handler,
               HttpContext ctx,
               CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(req, ct);
            if (!validation.IsValid) return validation.ToBadRequest();

            var id = await handler.Handle(new CreateOrderCommand(req.CustomerName, req.Total), ct);
            var uri = $"{ctx.Request.Scheme}://{ctx.Request.Host}{Pattern}/{id}";
            return Created(uri, new { id });
        };
}
