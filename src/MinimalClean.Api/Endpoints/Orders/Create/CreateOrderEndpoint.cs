using FluentValidation;
using MinimalClean.Api.Common;

namespace MinimalClean.Api.Endpoints.Orders.Create;

public class CreateOrderEndpoint : IEndpoint
{
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly CreateOrderHandler _handler;
    private readonly ILogger<CreateOrderEndpoint> _logger;

    public CreateOrderEndpoint(
        IValidator<CreateOrderRequest> validator,
        CreateOrderHandler handler,
        ILogger<CreateOrderEndpoint> logger)
    {
        _validator = validator;
        _handler = handler;
        _logger = logger;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", HandleAsync)
           .WithName("CreateOrder")
           .Produces(201)
           .Produces(400);
    }

    private async Task<IResult> HandleAsync(CreateOrderRequest req, HttpContext ctx, CancellationToken ct)
    {
        _logger.LogInformation("Handling CreateOrder request for customer {Customer}", req.CustomerName);

        var validation = await _validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for CreateOrder request: {Errors}",
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
            return validation.ToBadRequest();
        }

        var id = await _handler.Handle(new CreateOrderCommand(req.CustomerName, req.Total), ct);
        _logger.LogInformation("Order {OrderId} created successfully for {Customer}", id, req.CustomerName);

        var uri = $"{ctx.Request.Scheme}://{ctx.Request.Host}/orders/{id}";
        return Results.Created(uri, new { id });
    }
}

// static approach
//public class CreateOrderEndpoint : EndpointBase
//{
//    public static string Pattern => "/orders";
//    public static Delegate Handler =>
//        async (CreateOrderRequest req,
//               IValidator<CreateOrderRequest> validator,
//               CreateOrderHandler handler,
//               HttpContext ctx,
//               CancellationToken ct) =>
//        {
//            var validation = await validator.ValidateAsync(req, ct);
//            if (!validation.IsValid) return validation.ToBadRequest();

//            var id = await handler.Handle(new CreateOrderCommand(req.CustomerName, req.Total), ct);
//            var uri = $"{ctx.Request.Scheme}://{ctx.Request.Host}{Pattern}/{id}";
//            return Created(uri, new { id });
//        };
//}
