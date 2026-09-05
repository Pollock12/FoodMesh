namespace FoodMesh.Shared.Common;

/// <summary>
/// Base class for all Domain Entities.
/// An Entity is an object defined by its identity (Id) rather than its attributes.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier (e.g., Guid or string).</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
