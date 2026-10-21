using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEP.Application.DTOs;
using SIGEP.Application.Interfaces;
using System.Security.Claims;

namespace SIGEP.Api.Controllers;

[ApiController]
[Route("api/planificacion-sabados")]
[Authorize]
public class PlanificacionSabadosController : ControllerBase
{
    private readonly IPlanificacionSabadoAppService _planificacionService;

    public PlanificacionSabadosController(IPlanificacionSabadoAppService planificacionService)
    {
        _planificacionService = planificacionService;
    }

    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out int id) ? id : 1;
    }

    /// <summary>
    /// RF08: Genera automáticamente la propuesta preliminar de sábados con el 50% de dotación y alternancia.
    /// </summary>
    [HttpPost("propuesta")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> GenerarPropuesta([FromBody] GenerarPropuestaSabadoRequestDto request)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var propuesta = await _planificacionService.GenerarPropuestaAsync(request, idUsuario);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = propuesta.Id }, propuesta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una planificación por su fecha (formato YYYY-MM-DD).
    /// </summary>
    [HttpGet("por-fecha/{fecha}")]
    public async Task<IActionResult> ObtenerPorFecha(string fecha)
    {
        if (!DateOnly.TryParse(fecha, out var dateOnly))
            return BadRequest(new { mensaje = "Formato de fecha inválido. Utilice YYYY-MM-DD." });

        var plan = await _planificacionService.ObtenerPorFechaAsync(dateOnly);
        if (plan == null)
            return NotFound(new { mensaje = $"No existe planificación registrada para el sábado {fecha}." });

        return Ok(plan);
    }

    /// <summary>
    /// Obtiene una planificación por su ID interno.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var plan = await _planificacionService.ObtenerPorIdAsync(id);
            return Ok(plan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de planificaciones recientes.
    /// </summary>
    [HttpGet("historial")]
    public async Task<IActionResult> ObtenerHistorial([FromQuery] int limite = 15)
    {
        var historial = await _planificacionService.ObtenerHistorialAsync(limite);
        return Ok(historial);
    }

    /// <summary>
    /// RF12: Modifica manualmente el turno asignado a un agente, justificando la causa.
    /// </summary>
    [HttpPut("modificar-turno")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> ModificarTurno([FromBody] ModificarTurnoSabadoRequestDto request)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var planActualizado = await _planificacionService.ModificarTurnoAsync(request, idUsuario);
            return Ok(planActualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// RF12: Reemplaza un agente asignado por otro agente de dotación activa, justificando la causa.
    /// </summary>
    [HttpPut("reemplazar")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> ReemplazarEmpleado([FromBody] ReemplazarEmpleadoSabadoRequestDto request)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var planActualizado = await _planificacionService.ReemplazarEmpleadoAsync(request, idUsuario);
            return Ok(planActualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// RF13: Confirma formalmente la nómina del sábado, bloqueando modificaciones.
    /// </summary>
    [HttpPost("{id:int}/confirmar")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> Confirmar(int id)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var planConfirmado = await _planificacionService.ConfirmarAsync(id, idUsuario);
            return Ok(planConfirmado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}

