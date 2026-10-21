using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;

namespace SIGEP.Data.Repositories;

/// <summary>
/// Implementación con Entity Framework Core del Repositorio de Empleados.
/// Incluye las relaciones de navegación (Funcion, Regimen, TurnoHabitual) para evitar consultas N+1.
/// </summary>
public class EmpleadoRepository : Repository<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(SIGEPDbContext context) : base(context)
    {
    }

    public override async Task<Empleado?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Funcion)
            .Include(e => e.Regimen)
            .Include(e => e.TurnoHabitual)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Empleado?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var docNormalizado = documento.Trim().Replace(".", "");
        return await DbSet
            .Include(e => e.Funcion)
            .Include(e => e.Regimen)
            .Include(e => e.TurnoHabitual)
            .FirstOrDefaultAsync(e => e.Documento == docNormalizado, cancellationToken);
    }

    public async Task<bool> ExistsDocumentoAsync(string documento, int? excludeEmpleadoId = null, CancellationToken cancellationToken = default)
    {
        var docNormalizado = documento.Trim().Replace(".", "");
        var query = DbSet.AsQueryable();

        if (excludeEmpleadoId.HasValue)
            query = query.Where(e => e.Id != excludeEmpleadoId.Value);

        return await query.AnyAsync(e => e.Documento == docNormalizado, cancellationToken);
    }

    public async Task<IEnumerable<Empleado>> GetActivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Funcion)
            .Include(e => e.Regimen)
            .Include(e => e.TurnoHabitual)
            .Where(e => e.Estado == EstadoEmpleado.Activo)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Empleado>> GetFilteredAsync(string? query, int? regimenId, EstadoEmpleado? estado, CancellationToken cancellationToken = default)
    {
        var q = DbSet
            .Include(e => e.Funcion)
            .Include(e => e.Regimen)
            .Include(e => e.TurnoHabitual)
            .AsQueryable();

        if (regimenId.HasValue && regimenId.Value > 0)
        {
            q = q.Where(e => e.RegimenId == regimenId.Value);
        }

        if (estado.HasValue)
        {
            q = q.Where(e => e.Estado == estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLower();
            var termDoc = term.Replace(".", "");
            q = q.Where(e =>
                e.Nombre.ToLower().Contains(term) ||
                e.Apellido.ToLower().Contains(term) ||
                e.Documento.Contains(termDoc) ||
                (e.Nombre.ToLower() + " " + e.Apellido.ToLower()).Contains(term));
        }

        return await q
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }
}

