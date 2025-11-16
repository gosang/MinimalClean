using FluentValidation;
using FluentValidation.Results;

namespace MinimalClean.Api.Common;

public class ValidationFilter<TRequest> : IEndpointFilter
{
    private readonly IValidator<TRequest> _validator;

    public ValidationFilter(IValidator<TRequest> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return Results.BadRequest(new { error = "Invalid request payload" });
        }

        ValidationResult validation = await _validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!validation.IsValid)
        {
            return Results.BadRequest(new
            {
                errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }

        // Continue pipeline if valid
        return await next(context);
    }
}