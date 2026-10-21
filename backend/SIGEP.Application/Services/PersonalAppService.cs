using SIGEP.Application.DTOs.Personal;
using SIGEP.Application.Interfaces;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;

namespace SIGEP.Application.Services;

/// <summary>
/// Servicio de Aplicación para Gestión de Personal.
/// Implementa los casos de uso garantizando las reglas de negocio del ERS de Simoca.
/// </summary>
public class PersonalAppService : IPersonalAppService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IFuncionRepository _funcionRepository;
    private readonly IRegimenRepository _regimenRepository;
    private readonly ITurnoRepository _turnoRepository;

    public PersonalAppService(
        IEmpleadoRepository empleadoRepository,
        IFuncionRepository funcionRepository,
        IRegimenRepository regimenRepository,
        ITurnoRepository turnoRepository)
    {
        _empleadoRepository = empleadoRepository ?? throw new ArgumentNullException(nameof(empleadoRepository));
        _funcionRepository = funcionRepository ?? throw new ArgumentNullException(nameof(funcionRepository));
        _regimenRepository = regimenRepository ?? throw new ArgumentNullException(nameof(regimenRepository));
        _turnoRepository = turnoRepository ?? throw new ArgumentNullException(nameof(turnoRepository));
    }

    public async Task<IEnumerable<EmpleadoDto>> ListarPersonalAsync(string? query, int? regimenId, string? estado, CancellationToken cancellationToken = default)
    {
        EstadoEmpleado? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoEmpleado>(estado, true, out var parsedEstado))
        {
            estadoEnum = parsedEstado;
        }

        var empleados = await _empleadoRepository.GetFilteredAsync(query, regimenId, estadoEnum, cancellationToken);
        return empleados.Select(MapToDto);
    }

    public async Task<EmpleadoDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id, cancellationToken);
        return empleado == null ? null : MapToDto(empleado);
    }

    public async Task<EmpleadoDto> RegistrarEmpleadoAsync(CrearEmpleadoRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1. Validar que el DNI no esté duplicado
        var docNormalizado = request.Documento.Trim().Replace(".", "");
        if (await _empleadoRepository.ExistsDocumentoAsync(docNormalizado, null, cancellationToken))
            throw new InvalidOperationException($"Ya existe un empleado registrado con el DNI {request.Documento}.");

        // 2. Validar que la Función exista
        var funcion = await _funcionRepository.GetByIdAsync(request.FuncionId, cancellationToken);
        if (funcion == null)
            throw new ArgumentException("La función laboral especificada no existe.");

        // 3. Validar que el Régimen exista
        var regimen = await _regimenRepository.GetByIdAsync(request.RegimenId, cancellationToken);
        if (regimen == null)
            throw new ArgumentException("El régimen especificado no existe.");

        // 4. Regla de Negocio RF03: Validar que el Turno Habitual pertenezca al Régimen
        var turno = await _turnoRepository.GetByIdAsync(request.TurnoHabitualId, cancellationToken);
        if (turno == null)
            throw new ArgumentException("El turno habitual especificado no existe.");

        if (turno.RegimenId != request.RegimenId)
            throw new InvalidOperationException($"Regla de negocio: El turno '{turno.Nombre}' no pertenece al régimen '{regimen.Nombre}'.");

        // 5. Instanciar la Entidad de Dominio
        var nuevoEmpleado = new Empleado(
            nombre: request.Nombre,
            apellido: request.Apellido,
            documento: docNormalizado,
            funcionId: request.FuncionId,
            regimenId: request.RegimenId,
            turnoHabitualId: request.TurnoHabitualId,
            direccion: request.Direccion,
            telefono: request.Telefono,
            email: request.Email
        );

        // 6. Agregar y guardar cambios
        await _empleadoRepository.AddAsync(nuevoEmpleado, cancellationToken);
        await _empleadoRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Retornar DTO con relaciones cargadas
        var creado = await _empleadoRepository.GetByIdAsync(nuevoEmpleado.Id, cancellationToken);
        return MapToDto(creado!);
    }

    public async Task<EmpleadoDto> ModificarEmpleadoAsync(int id, ActualizarEmpleadoRequestDto request, CancellationToken cancellationToken = default)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id, cancellationToken);
        if (empleado == null)
            throw new KeyNotFoundException($"No se encontró el empleado con Id {id}.");

        var docNormalizado = request.Documento.Trim().Replace(".", "");
        if (await _empleadoRepository.ExistsDocumentoAsync(docNormalizado, id, cancellationToken))
            throw new InvalidOperationException($"El DNI {request.Documento} ya está asignado a otro empleado.");

        var turno = await _turnoRepository.GetByIdAsync(request.TurnoHabitualId, cancellationToken);
        if (turno == null || turno.RegimenId != request.RegimenId)
            throw new InvalidOperationException("El turno habitual seleccionado no corresponde al régimen laboral asignado.");

        empleado.ModificarDatosPersonales(
            nombre: request.Nombre,
            apellido: request.Apellido,
            documento: docNormalizado,
            funcionId: request.FuncionId,
            direccion: request.Direccion,
            telefono: request.Telefono,
            email: request.Email
        );

        empleado.AsignarRegimenYTurno(request.RegimenId, request.TurnoHabitualId);

        _empleadoRepository.Update(empleado);
        await _empleadoRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var actualizado = await _empleadoRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(actualizado!);
    }

    public async Task<bool> CambiarEstadoAsync(int id, CancellationToken cancellationToken = default)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(id, cancellationToken);
        if (empleado == null)
            return false;

        if (empleado.EstaActivo())
            empleado.DarDeBaja();
        else
            empleado.Reactivar();

        _empleadoRepository.Update(empleado);
        await _empleadoRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ListasReferenciaDto> ObtenerListasReferenciaAsync(CancellationToken cancellationToken = default)
    {
        var funciones = await _funcionRepository.GetAllAsync(cancellationToken);
        var regimenes = await _regimenRepository.GetAllAsync(cancellationToken);
        var turnos = await _turnoRepository.GetAllAsync(cancellationToken);

        return new ListasReferenciaDto(
            Funciones: funciones.Select(f => new OpcionReferenciaDto(f.Id, f.Nombre, f.Descripcion)),
            Regimenes: regimenes.Select(r => new OpcionReferenciaDto(r.Id, r.Nombre, r.Descripcion)),
            Turnos: turnos.Select(t => new TurnoReferenciaDto(
                t.Id,
                t.Nombre,
                $"{t.HoraInicio:HH:mm} - {t.HoraFin:HH:mm}",
                t.RegimenId
            ))
        );
    }

    private static EmpleadoDto MapToDto(Empleado e)
    {
        return new EmpleadoDto(
            Id: e.Id,
            Nombre: e.Nombre,
            Apellido: e.Apellido,
            NombreCompleto: e.NombreCompleto,
            Documento: e.Documento,
            Direccion: e.Direccion,
            Telefono: e.Telefono,
            Email: e.Email,
            FuncionId: e.FuncionId,
            FuncionNombre: e.Funcion?.Nombre ?? "No asignada",
            RegimenId: e.RegimenId,
            RegimenNombre: e.Regimen?.Nombre ?? "No asignado",
            TurnoHabitualId: e.TurnoHabitualId,
            TurnoHabitualNombre: e.TurnoHabitual?.Nombre ?? "No asignado",
            TurnoHorario: e.TurnoHabitual != null ? $"{e.TurnoHabitual.HoraInicio:HH:mm} - {e.TurnoHabitual.HoraFin:HH:mm}" : "",
            Estado: e.Estado.ToString(),
            FechaIngreso: e.FechaIngreso
        );
    }
}

