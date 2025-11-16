namespace MinimalClean.Domain.Common;

public interface IDomainEvent { DateTime OccurredUtc { get; } }

public abstract class HasDomainEvents
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    protected void Raise(IDomainEvent @event) => _events.Add(@event);
    public void ClearEvents() => _events.Clear();
}