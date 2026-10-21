namespace SIGEP.Application.DTOs;

public class GenerarPropuestaSabadoRequestDto
{
    public DateOnly FechaSabado { get; set; }
    public string? Observaciones { get; set; }
}

public class ModificarTurnoSabadoRequestDto
{
    public int IdPlanificacion { get; set; }
    public int IdEmpleado { get; set; }
    public int NuevoIdTurno { get; set; }
    public string Motivo { get; set; } = null!;
}

public class ReemplazarEmpleadoSabadoRequestDto
{
    public int IdPlanificacion { get; set; }
    public int IdEmpleadoSaliente { get; set; }
    public int IdEmpleadoEntrante { get; set; }
    public int IdTurno { get; set; }
    public string Motivo { get; set; } = null!;
}

public class DetallePlanificacionSabadoDto
{
    public int Id { get; set; }
    public int IdEmpleado { get; set; }
    public string Documento { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Funcion { get; set; } = null!;
    public int IdTurno { get; set; }
    public string NombreTurno { get; set; } = null!;
    public string HorarioTurno { get; set; } = null!;
    public bool EsModificacionManual { get; set; }
    public string? MotivoModificacion { get; set; }
}

public class PlanificacionSabadoDto
{
    public int Id { get; set; }
    public DateOnly FechaSabado { get; set; }
    public string Estado { get; set; } = null!;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public string? UsuarioConfirmacion { get; set; }
    public int TotalAsignados { get; set; }
    public List<DetallePlanificacionSabadoDto> Detalles { get; set; } = new();
}

public class PlanificacionSabadoResumenDto
{
    public int Id { get; set; }
    public DateOnly FechaSabado { get; set; }
    public string Estado { get; set; } = null!;
    public int TotalAsignados { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
}

