using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;

namespace SIGEP.Domain.Repositories;

/// <summary>
/// Contrato de persistencia para el módulo de Planificación de Sábados (RF08 - RF13).
/// </summary>
public interface IPlanificacionSabadoRepository : IRepository<PlanificacionSabado>
{
    Task<PlanificacionSabado?> GetByFechaAsync(DateOnly fechaSabado, CancellationToken cancellationToken = default);
    Task<PlanificacionSabado?> GetConDetallesByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PlanificacionSabado?> GetUltimaConfirmadaAntesDeAsync(DateOnly fecha, CancellationToken cancellationToken = default);
    Task<List<PlanificacionSabado>> GetHistorialAsync(int limite = 15, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve un mapa con el ID de empleado y la fecha del último sábado en el que prestó servicios.
    /// </summary>
    Task<Dictionary<int, DateOnly>> GetUltimoSabadoTrabajadoPorEmpleadoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve un mapa con el ID de empleado y el ID del turno en el que prestó servicios su último sábado.
    /// </summary>
    Task<Dictionary<int, int>> GetUltimoTurnoSabadoPorEmpleadoAsync(CancellationToken cancellationToken = default);
}

