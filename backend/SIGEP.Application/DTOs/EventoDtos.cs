namespace SIGEP.Application.DTOs;

public class CrearTurnoEventoRequestDto
{
    public string Nombre { get; set; } = null!;
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int CupoRequerido { get; set; }
}

public class CrearEventoRequestDto
{
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string? Lugar { get; set; }
    public List<CrearTurnoEventoRequestDto> Turnos { get; set; } = new();
}

public class AsignacionEventoDto
{
    public int Id { get; set; }
    public int IdEmpleado { get; set; }
    public string Documento { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Funcion { get; set; } = null!;
    public string Regimen { get; set; } = null!;
    public decimal PuntajeCalculado { get; set; }
    public string Estado { get; set; } = null!;
    public bool EsAsignacionManual { get; set; }
    public string? MotivoModificacion { get; set; }
}

public class TurnoEventoDto
{
    public int Id { get; set; }
    public int IdEvento { get; set; }
    public string Nombre { get; set; } = null!;
    public DateOnly Fecha { get; set; }
    public string Horario { get; set; } = null!;
    public int CupoRequerido { get; set; }
    public int TotalAsignados { get; set; }
    public List<AsignacionEventoDto> Asignaciones { get; set; } = new();
}

public class EventoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string? Lugar { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public string? UsuarioConfirmacion { get; set; }
    public int TotalTurnos { get; set; }
    public List<TurnoEventoDto> Turnos { get; set; } = new();
}

public class EventoResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string? Lugar { get; set; }
    public string Estado { get; set; } = null!;
    public int TotalTurnos { get; set; }
    public int TotalPersonalAsignado { get; set; }
}

public class CandidatoEvaluadoDto
{
    public int IdEmpleado { get; set; }
    public string Documento { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Funcion { get; set; } = null!;
    public string Regimen { get; set; } = null!;
    public decimal PuntajeTotal { get; set; }
    public decimal PuntajeHistorial { get; set; }
    public decimal PuntajeRecencia { get; set; }
    public decimal PuntajeSabados { get; set; }
    public decimal PuntajeAntiguedad { get; set; }
}

public class CandidatoExcluidoDto
{
    public int IdEmpleado { get; set; }
    public string Documento { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Regimen { get; set; } = null!;
    public string MotivoExclusion { get; set; } = null!;
}

public class PropuestaScoringTurnoResponseDto
{
    public int IdTurnoEvento { get; set; }
    public string NombreTurno { get; set; } = null!;
    public int CupoRequerido { get; set; }
    public List<CandidatoEvaluadoDto> AsignacionesPropuestas { get; set; } = new();
    public List<CandidatoExcluidoDto> AgentesExcluidosPorDescanso { get; set; } = new();
}

public class AjustarAsignacionEventoRequestDto
{
    public int IdTurnoEvento { get; set; }
    public int IdEmpleadoSaliente { get; set; }
    public int IdEmpleadoEntrante { get; set; }
    public string Motivo { get; set; } = null!;
}
