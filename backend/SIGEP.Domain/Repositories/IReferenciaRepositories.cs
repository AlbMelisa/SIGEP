using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;

namespace SIGEP.Domain.Repositories;

public interface IFuncionRepository : IRepository<Funcion>
{
}

public interface IRegimenRepository : IRepository<Regimen>
{
}

public interface ITurnoRepository : IRepository<Turno>
{
    Task<IEnumerable<Turno>> GetByRegimenIdAsync(int regimenId, CancellationToken cancellationToken = default);
}

