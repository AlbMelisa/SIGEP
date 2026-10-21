using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEP.Application.DTOs.Personal;
using SIGEP.Application.Interfaces;

namespace SIGEP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere token JWT para cualquier operación de personal
public class PersonalController : ControllerBase
{
    private readonly IPersonalAppService _personalAppService;

    public PersonalController(IPersonalAppService personalAppService)
    {
        _personalAppService = personalAppService ?? throw new ArgumentNullException(nameof(personalAppService));
    }

    /// <summary>
    /// Consulta el listado de empleados con filtros por búsqueda de texto, régimen y estado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? query,
        [FromQuery] int? regimenId,
        [FromQuery] string? estado,
        CancellationToken cancellationToken)
    {
        var empleados = await _personalAppService.ListarPersonalAsync(query, regimenId, estado, cancellationToken);
        return Ok(empleados);
    }

    /// <summary>
    /// Obtiene un empleado por su Id con todos sus datos laborales.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var empleado = await _personalAppService.ObtenerPorIdAsync(id, cancellationToken);
        if (empleado == null)
            return NotFound(new { error = $"Empleado con Id {id} no encontrado." });

        return Ok(empleado);
    }

    /// <summary>
    /// Registra un nuevo empleado en la dotación municipal.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] CrearEmpleadoRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var nuevo = await _personalAppService.RegistrarEmpleadoAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Modifica los datos personales o laborales de un empleado existente.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] ActualizarEmpleadoRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var actualizado = await _personalAppService.ModificarEmpleadoAsync(id, request, cancellationToken);
            return Ok(actualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Alterna el estado del empleado (Activo / Inactivo - Baja lógica).
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CancellationToken cancellationToken)
    {
        var resultado = await _personalAppService.CambiarEstadoAsync(id, cancellationToken);
        if (!resultado)
            return NotFound(new { error = $"Empleado con Id {id} no encontrado." });

        return Ok(new { message = "Estado del empleado actualizado correctamente." });
    }

    /// <summary>
    /// Retorna las listas maestras de Funciones, Regímenes y Turnos para los formularios de selección.
    /// </summary>
    [HttpGet("referencias")]
    [AllowAnonymous] // Permite al frontend cargar catálogos
    public async Task<IActionResult> ObtenerReferencias(CancellationToken cancellationToken)
    {
        var referencias = await _personalAppService.ObtenerListasReferenciaAsync(cancellationToken);
        return Ok(referencias);
    }
}

