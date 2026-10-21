using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Repositories;

namespace SIGEP.Data.Repositories;

public class FuncionRepository : Repository<Funcion>, IFuncionRepository
{
    public FuncionRepository(SIGEPDbContext context) : base(context) { }
}

public class RegimenRepository : Repository<Regimen>, IRegimenRepository
{
    public RegimenRepository(SIGEPDbContext context) : base(context) { }
}

public class TurnoRepository : Repository<Turno>, ITurnoRepository
{
    public TurnoRepository(SIGEPDbContext context) : base(context) { }

    public async Task<IEnumerable<Turno>> GetByRegimenIdAsync(int regimenId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.RegimenId == regimenId)
            .OrderBy(t => t.HoraInicio)
            .ToListAsync(cancellationToken);
    }
}

