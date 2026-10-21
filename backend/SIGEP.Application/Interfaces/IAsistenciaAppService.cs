using SIGEP.Application.DTOs.Asistencia;

namespace SIGEP.Application.Interfaces;

/// <summary>
/// Contrato del Servicio de Aplicación para el Módulo de Asistencia.
/// Expone los casos de uso para la planilla diaria y la consulta de historial (RF04 - RF07).
/// </summary>
public interface IAsistenciaAppService
{
    Task<IEnumerable<AsistenciaItemDto>> ObtenerPlanillaDelDiaAsync(DateOnly fecha, int? turnoId, CancellationToken cancellationToken = default);
    Task<int> GuardarPlanillaLoteAsync(GuardarPlanillaLoteRequestDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistorialAsistenciaDto>> ConsultarHistorialAsync(FiltroHistorialRequestDto filtro, CancellationToken cancellationToken = default);
}

