using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Repositories;

/// <summary>
/// Contrato de persistencia y consultas especializadas de Empleados.
/// </summary>
public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<Empleado?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<bool> ExistsDocumentoAsync(string documento, int? excludeEmpleadoId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Empleado>> GetActivosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Empleado>> GetFilteredAsync(string? query, int? regimenId, EstadoEmpleado? estado, CancellationToken cancellationToken = default);
}

