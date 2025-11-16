namespace MinimalClean.Api.Common;

public abstract class EndpointBase
{
    protected static IResult Ok<T>(T value) => Results.Ok(value);
    protected static IResult Created<T>(string uri, T value) => Results.Created(uri, value);
    protected static IResult NotFound(string message) => Results.NotFound(new { error = message });
    protected static IResult BadRequest(object error) => Results.BadRequest(error);
}
