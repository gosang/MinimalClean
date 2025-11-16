using FluentValidation;

namespace MinimalClean.Api.Endpoints.Orders.Create;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().MaximumLength(100);
        RuleFor(x => x.Total)
            .GreaterThan(0);
    }
}
