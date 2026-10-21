namespace SIGEP.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Una entidad se define por su identidad única (Id) y no por el valor de sus atributos.
/// </summary>
public abstract class Entity
{
    public virtual int Id { get; protected set; }

    public bool IsTransient() => this.Id == 0;

    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Entity)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        Entity item = (Entity)obj;

        if (item.IsTransient() || this.IsTransient())
            return false;

        return item.Id == this.Id;
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
            return this.Id.GetHashCode() ^ 31;

        return base.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (Equals(left, null))
            return Equals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}

