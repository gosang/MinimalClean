namespace MinimalClean.Application.Abstractions;

public interface IHandler<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}