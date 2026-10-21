using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Repositories;

/// <summary>
/// Contrato del Repositorio de Asistencia.
/// Soporta la carga diaria por jornada y las consultas complejas de historial multivariable (RF06).
/// </summary>
public interface IAsistenciaRepository : IRepository<Asistencia>
{
    Task<Asistencia?> GetByEmpleadoYFechaAsync(int empleadoId, DateOnly fecha, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asistencia>> GetByFechaAsync(DateOnly fecha, int? turnoId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asistencia>> GetHistorialAsync(
        DateOnly? desde,
        DateOnly? hasta,
        int? empleadoId,
        int? turnoId,
        EstadoAsistencia? estado,
        TipoJornada? tipoJornada,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Asistencia>> GetAsistenciasRecientesAsync(int empleadoId, DateOnly fechaReferencia, int diasAtras = 30, CancellationToken cancellationToken = default);
}

