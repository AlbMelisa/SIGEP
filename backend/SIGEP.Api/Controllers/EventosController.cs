using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEP.Application.DTOs;
using SIGEP.Application.Interfaces;
using System.Security.Claims;

namespace SIGEP.Api.Controllers;

[ApiController]
[Route("api/eventos")]
[Authorize]
public class EventosController : ControllerBase
{
    private readonly IEventoAppService _eventoService;

    public EventosController(IEventoAppService eventoService)
    {
        _eventoService = eventoService;
    }

    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out int id) ? id : 1;
    }

    /// <summary>
    /// RF14: Registra un nuevo evento especial con sus turnos operativos.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> Crear([FromBody] CrearEventoRequestDto request)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var evento = await _eventoService.CrearEventoAsync(request, idUsuario);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = evento.Id }, evento);
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
    /// Obtiene el detalle completo de un evento con sus turnos y nóminas asignadas.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var evento = await _eventoService.ObtenerPorIdAsync(id);
            return Ok(evento);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Lista eventos activos (Planificados o En Curso).
    /// </summary>
    [HttpGet("activos")]
    public async Task<IActionResult> ObtenerActivos()
    {
        var eventos = await _eventoService.ObtenerEventosActivosAsync();
        return Ok(eventos);
    }

    /// <summary>
    /// Obtiene el historial de eventos.
    /// </summary>
    [HttpGet("historial")]
    public async Task<IActionResult> ObtenerHistorial([FromQuery] int limite = 20)
    {
        var eventos = await _eventoService.ObtenerHistorialAsync(limite);
        return Ok(eventos);
    }

    /// <summary>
    /// RF16 & RF17: Previsualiza el resultado del algoritmo de scoring ponderado y exclusiones por descanso.
    /// </summary>
    [HttpGet("turnos/{idTurno:int}/scoring")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> PrevisualizarScoring(int idTurno)
    {
        try
        {
            var propuesta = await _eventoService.GenerarPropuestaScoringAsync(idTurno);
            return Ok(propuesta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// RF16: Asigna la nómina calculada por el algoritmo al turno del evento.
    /// </summary>
    [HttpPost("turnos/{idTurno:int}/aplicar-scoring")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> AplicarScoring(int idTurno)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var eventoActualizado = await _eventoService.AplicarPropuestaScoringAsync(idTurno, idUsuario);
            return Ok(eventoActualizado);
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
    /// RF18: Permite el ajuste manual o reemplazo de agentes en la nómina del evento.
    /// </summary>
    [HttpPut("turnos/ajustar")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> AjustarAsignacion([FromBody] AjustarAsignacionEventoRequestDto request)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var eventoActualizado = await _eventoService.AjustarAsignacionAsync(request, idUsuario);
            return Ok(eventoActualizado);
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
    /// RF18: Confirma formalmente el evento y sus nóminas, dejándolo En Curso.
    /// </summary>
    [HttpPost("{id:int}/confirmar")]
    [Authorize(Roles = "Jefe,Encargado")]
    public async Task<IActionResult> Confirmar(int id)
    {
        try
        {
            var idUsuario = ObtenerUsuarioId();
            var eventoConfirmado = await _eventoService.ConfirmarEventoAsync(id, idUsuario);
            return Ok(eventoConfirmado);
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

