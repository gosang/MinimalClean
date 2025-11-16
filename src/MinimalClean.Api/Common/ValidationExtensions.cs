using FluentValidation.Results;

namespace MinimalClean.Api.Common;

public static class ValidationExtensions
{
    public static IResult ToBadRequest(this ValidationResult result) =>
        Results.BadRequest(new
        {
            errors = result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        });
}