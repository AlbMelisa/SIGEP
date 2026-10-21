using SIGEP.Application.DTOs.Personal;

namespace SIGEP.Application.Interfaces;

/// <summary>
/// Contrato del Servicio de Aplicación para el Módulo de Gestión de Personal.
/// Orquesta los casos de uso: alta, consulta con filtros, modificación y cambio de estado.
/// </summary>
public interface IPersonalAppService
{
    Task<IEnumerable<EmpleadoDto>> ListarPersonalAsync(string? query, int? regimenId, string? estado, CancellationToken cancellationToken = default);
    Task<EmpleadoDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmpleadoDto> RegistrarEmpleadoAsync(CrearEmpleadoRequestDto request, CancellationToken cancellationToken = default);
    Task<EmpleadoDto> ModificarEmpleadoAsync(int id, ActualizarEmpleadoRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> CambiarEstadoAsync(int id, CancellationToken cancellationToken = default);
    Task<ListasReferenciaDto> ObtenerListasReferenciaAsync(CancellationToken cancellationToken = default);
}

