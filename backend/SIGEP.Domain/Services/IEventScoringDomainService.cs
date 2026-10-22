using SIGEP.Domain.Entities;

namespace SIGEP.Domain.Services;

public class EvaluacionCandidatoEvento
{
    public Empleado Empleado { get; set; } = null!;
    public decimal PuntajeTotal { get; set; }
    public decimal PuntajeHistorial { get; set; }
    public decimal PuntajeRecencia { get; set; }
    public decimal PuntajeSabados { get; set; }
    public decimal PuntajeAsistenciaSemanal { get; set; }
    public bool EstaExcluido { get; set; }
    public string? MotivoExclusion { get; set; }
}

public class InfoHistoricoEmpleado
{
    public int CantidadEventosAnioActual { get; set; }
    public DateOnly? FechaUltimoEvento { get; set; }
    public int CantidadSabadosUltimos90Dias { get; set; }
    public int DiasPresentesSemanaPrevia { get; set; }
    public DateTime? FechaFinUltimaGuardiaNocturna { get; set; }
}

public interface IEventScoringDomainService
{
    /// <summary>
    /// RF16 & RF17: Evalúa todos los agentes candidatos para un turno de evento,
    /// aplicando la exclusión estricta de 48 hs de descanso post-noche y calculando el puntaje ponderado
    /// basado en historial de eventos, factor de rotación (recencia), balance de sábados y asistencia semanal.
    /// </summary>
    IReadOnlyList<EvaluacionCandidatoEvento> EvaluarCandidatosParaTurno(
        TurnoEvento turnoEvento,
        IReadOnlyList<Empleado> empleadosActivos,
        IReadOnlyDictionary<int, InfoHistoricoEmpleado> historicoPorEmpleado,
        DateTime fechaReferencia);
}
