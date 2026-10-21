using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEP.Application.DTOs.Asistencia;
using SIGEP.Application.Interfaces;

namespace SIGEP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciaController : ControllerBase
{
    private readonly IAsistenciaAppService _asistenciaAppService;

    public AsistenciaController(IAsistenciaAppService asistenciaAppService)
    {
        _asistenciaAppService = asistenciaAppService ?? throw new ArgumentNullException(nameof(asistenciaAppService));
    }

    /// <summary>
    /// Obtiene la planilla de asistencia del día para el registro de marcaciones (RF04).
    /// Si no hay registros guardados aún, genera la grilla inicial con los empleados activos.
    /// </summary>
    [HttpGet("planilla")]
    public async Task<IActionResult> ObtenerPlanilla(
        [FromQuery] string? fecha,
        [FromQuery] int? turnoId,
        CancellationToken cancellationToken)
    {
        DateOnly fechaConsulta = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(fecha) && DateOnly.TryParse(fecha, out var parsedFecha))
        {
            fechaConsulta = parsedFecha;
        }

        var planilla = await _asistenciaAppService.ObtenerPlanillaDelDiaAsync(fechaConsulta, turnoId, cancellationToken);
        return Ok(planilla);
    }

    /// <summary>
    /// Guarda en lote la planilla de asistencia diaria (RF04, RF05).
    /// </summary>
    [HttpPost("guardar")]
    public async Task<IActionResult> GuardarPlanilla(
        [FromBody] GuardarPlanillaLoteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            int guardados = await _asistenciaAppService.GuardarPlanillaLoteAsync(request, cancellationToken);
            return Ok(new { message = $"Se guardaron {guardados} registros de asistencia exitosamente.", guardados });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta el historial de asistencia con filtros multivariables (RF06, RF07).
    /// </summary>
    [HttpGet("historial")]
    public async Task<IActionResult> ConsultarHistorial(
        [FromQuery] string? desde,
        [FromQuery] string? hasta,
        [FromQuery] int? empleadoId,
        [FromQuery] int? turnoId,
        [FromQuery] string? estado,
        [FromQuery] string? tipoJornada,
        CancellationToken cancellationToken)
    {
        DateOnly? fechaDesde = null;
        if (!string.IsNullOrWhiteSpace(desde) && DateOnly.TryParse(desde, out var parsedDesde))
            fechaDesde = parsedDesde;

        DateOnly? fechaHasta = null;
        if (!string.IsNullOrWhiteSpace(hasta) && DateOnly.TryParse(hasta, out var parsedHasta))
            fechaHasta = parsedHasta;

        var filtro = new FiltroHistorialRequestDto(
            Desde: fechaDesde,
            Hasta: fechaHasta,
            EmpleadoId: empleadoId,
            TurnoId: turnoId,
            Estado: estado,
            TipoJornada: tipoJornada
        );

        var historial = await _asistenciaAppService.ConsultarHistorialAsync(filtro, cancellationToken);
        return Ok(historial);
    }
}

