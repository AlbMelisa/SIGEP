using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Services;

/// <summary>
/// Implementación del Servicio de Dominio para el Algoritmo de Scoring Ponderado (RF16)
/// alineado con el ERS IEEE 830 (historial, rotación, sábados y asistencia semanal)
/// y la verificación inquebrantable del descanso laboral de 48 hs (RF17).
/// </summary>
public class EventScoringDomainService : IEventScoringDomainService
{
    // Ponderadores del modelo matemático calibrados (suman 1.0)
    private const decimal W_HISTORIAL = 0.40m;           // Equidad distributiva en eventos anuales
    private const decimal W_RECENCIA = 0.30m;            // Factor de rotación quincenal/mensual
    private const decimal W_SABADOS = 0.15m;             // Compensación por carga de fines de semana
    private const decimal W_ASISTENCIA_SEMANAL = 0.15m;  // Presentismo y cumplimiento semanal regular (lunes a viernes)

    public IReadOnlyList<EvaluacionCandidatoEvento> EvaluarCandidatosParaTurno(
        TurnoEvento turnoEvento,
        IReadOnlyList<Empleado> empleadosActivos,
        IReadOnlyDictionary<int, InfoHistoricoEmpleado> historicoPorEmpleado,
        DateTime fechaReferencia)
    {
        var resultados = new List<EvaluacionCandidatoEvento>();

        // Fecha y hora de inicio del turno del evento
        var inicioTurnoEvento = turnoEvento.Fecha.ToDateTime(turnoEvento.HoraInicio);

        // Precalcular valores máximos para normalización
        int maxEventos = historicoPorEmpleado.Values.Any() 
            ? Math.Max(1, historicoPorEmpleado.Values.Max(v => v.CantidadEventosAnioActual)) 
            : 1;

        int maxSabados = historicoPorEmpleado.Values.Any() 
            ? Math.Max(1, historicoPorEmpleado.Values.Max(v => v.CantidadSabadosUltimos90Dias)) 
            : 1;

        foreach (var empleado in empleadosActivos)
        {
            if (empleado.Estado != EstadoEmpleado.Activo)
                continue;

            historicoPorEmpleado.TryGetValue(empleado.Id, out var historico);
            historico ??= new InfoHistoricoEmpleado();

            var evaluacion = new EvaluacionCandidatoEvento
            {
                Empleado = empleado
            };

            // =========================================================================
            // RF17: INVARIANTE INQUEBRANTABLE DE SEGURIDAD LABORAL
            // 48 horas de descanso obligatorio post-guardia nocturna (Régimen Histórico)
            // =========================================================================
            if (empleado.RegimenId == 2 && historico.FechaFinUltimaGuardiaNocturna.HasValue)
            {
                var finGuardia = historico.FechaFinUltimaGuardiaNocturna.Value;
                var horasDescanso = (inicioTurnoEvento - finGuardia).TotalHours;

                if (horasDescanso < 48.0)
                {
                    evaluacion.EstaExcluido = true;
                    evaluacion.MotivoExclusion = $"Excluido por descanso obligatorio de 48 hs post-guardia nocturna (finalizada {finGuardia:dd/MM/yyyy HH:mm}, descanso actual: {horasDescanso:F1} hs / 48 hs requeridas).";
                    evaluacion.PuntajeTotal = 0;
                    resultados.Add(evaluacion);
                    continue;
                }
            }

            // =========================================================================
            // RF16: CÁLCULO DEL SCORE PONDERADO (ERS IEEE 830)
            // =========================================================================

            // 1. Puntaje Historial: menor cantidad de eventos en el año = mayor prioridad (1.0 = 0 eventos)
            decimal puntajeHist = 1.0m - ((decimal)historico.CantidadEventosAnioActual / maxEventos);
            evaluacion.PuntajeHistorial = Math.Clamp(puntajeHist, 0.0m, 1.0m);

            // 2. Puntaje Recencia (Factor de rotación): más días sin eventos = mayor prioridad
            decimal puntajeRec;
            if (!historico.FechaUltimoEvento.HasValue)
            {
                puntajeRec = 1.0m; // Nunca participó, prioridad máxima
            }
            else
            {
                var diasDesdeUltimo = (turnoEvento.Fecha.DayNumber - historico.FechaUltimoEvento.Value.DayNumber);
                puntajeRec = Math.Clamp((decimal)diasDesdeUltimo / 180.0m, 0.0m, 1.0m); // Escala normalizada en 180 días
            }
            evaluacion.PuntajeRecencia = puntajeRec;

            // 3. Puntaje Sábados: menor carga de fines de semana en los últimos 90 días = mayor disponibilidad
            decimal puntajeSab = 1.0m - ((decimal)historico.CantidadSabadosUltimos90Dias / maxSabados);
            evaluacion.PuntajeSabados = Math.Clamp(puntajeSab, 0.0m, 1.0m);

            // 4. Puntaje Asistencia Semanal (Invertido para protección del descanso):
            // Si el evento es una carga laboral extraordinaria, quien cumplió los 5 días de lunes a viernes
            // queda protegido con Psem = 0.0 para descansar el fin de semana.
            // Quien tuvo inasistencias en la semana recibe mayor puntaje para ser convocado a compensar carga laboral.
            decimal ratioPresente = Math.Clamp((decimal)historico.DiasPresentesSemanaPrevia / 5.0m, 0.0m, 1.0m);
            decimal puntajeSem = 1.0m - ratioPresente;
            evaluacion.PuntajeAsistenciaSemanal = puntajeSem;

            // Ptotal = w1*Phist + w2*Prec + w3*Psab + w4*Psem
            evaluacion.PuntajeTotal = Math.Round(
                (W_HISTORIAL * evaluacion.PuntajeHistorial) +
                (W_RECENCIA * evaluacion.PuntajeRecencia) +
                (W_SABADOS * evaluacion.PuntajeSabados) +
                (W_ASISTENCIA_SEMANAL * evaluacion.PuntajeAsistenciaSemanal),
                4);

            resultados.Add(evaluacion);
        }

        // Ordenar primero los NO excluidos por mayor puntaje total descendente, luego por ID
        return resultados
            .OrderBy(e => e.EstaExcluido ? 1 : 0)
            .ThenByDescending(e => e.PuntajeTotal)
            .ThenBy(e => e.Empleado.Id)
            .ToList();
    }
}
