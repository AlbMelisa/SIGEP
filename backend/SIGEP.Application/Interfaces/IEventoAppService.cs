using SIGEP.Application.DTOs;

namespace SIGEP.Application.Interfaces;

public interface IEventoAppService
{
    Task<EventoDto> CrearEventoAsync(CrearEventoRequestDto request, int idUsuarioActual);
    Task<EventoDto> ObtenerPorIdAsync(int id);
    Task<List<EventoResumenDto>> ObtenerEventosActivosAsync();
    Task<List<EventoResumenDto>> ObtenerHistorialAsync(int limite = 20);
    Task<PropuestaScoringTurnoResponseDto> GenerarPropuestaScoringAsync(int idTurnoEvento);
    Task<EventoDto> AplicarPropuestaScoringAsync(int idTurnoEvento, int idUsuarioActual);
    Task<EventoDto> AjustarAsignacionAsync(AjustarAsignacionEventoRequestDto request, int idUsuarioActual);
    Task<EventoDto> ConfirmarEventoAsync(int idEvento, int idUsuarioActual);
}
