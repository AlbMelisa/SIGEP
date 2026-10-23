using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Repositories;

/// <summary>
/// Contrato del Repositorio de Asistencia.
/// Soporta la carga diaria por jornada, validación de francos compensatorios y consultas de historial (RF04 - RF07).
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

    /// <summary>
    /// Retorna los IDs de empleados que prestaron servicio el día sábado indicado (presentes en asistencia o confirmados en nómina).
    /// </summary>
    Task<HashSet<int>> ObtenerEmpleadosQueTrabajaronSabadoAsync(DateOnly sabado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna los IDs de empleados que ya tienen asignado un Franco en el rango semanal especificado (excluyendo la fecha actual).
    /// </summary>
    Task<HashSet<int>> ObtenerEmpleadosConFrancoEnSemanaAsync(DateOnly inicioSemana, DateOnly finSemana, DateOnly fechaExcluir, CancellationToken cancellationToken = default);
}
