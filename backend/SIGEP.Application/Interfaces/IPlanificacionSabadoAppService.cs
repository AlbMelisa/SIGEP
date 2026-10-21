using SIGEP.Application.DTOs;

namespace SIGEP.Application.Interfaces;

public interface IPlanificacionSabadoAppService
{
    Task<PlanificacionSabadoDto> GenerarPropuestaAsync(GenerarPropuestaSabadoRequestDto request, int idUsuarioActual);
    Task<PlanificacionSabadoDto?> ObtenerPorFechaAsync(DateOnly fechaSabado);
    Task<PlanificacionSabadoDto> ObtenerPorIdAsync(int id);
    Task<List<PlanificacionSabadoResumenDto>> ObtenerHistorialAsync(int limite = 15);
    Task<PlanificacionSabadoDto> ModificarTurnoAsync(ModificarTurnoSabadoRequestDto request, int idUsuarioActual);
    Task<PlanificacionSabadoDto> ReemplazarEmpleadoAsync(ReemplazarEmpleadoSabadoRequestDto request, int idUsuarioActual);
    Task<PlanificacionSabadoDto> ConfirmarAsync(int idPlanificacion, int idUsuarioActual);
}

