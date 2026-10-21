using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Services;

/// <summary>
/// Implementación del Servicio de Dominio para el Algoritmo de Scoring Ponderado (RF16)
/// y la verificación inquebrantable del descanso laboral de 48 hs (RF17).
/// </summary>
public class EventScoringDomainService : IEventScoringDomainService
{
    // Ponderadores del modelo matemático (suman 1.0)
    private const decimal W_HISTORIAL = 0.40m;   // Equidad en participación
    private const decimal W_RECENCIA = 0.30m;    // Tiempo transcurrido desde el último evento
    private const decimal W_SABADOS = 0.15m;     // Compensación de carga de fines de semana
    private const decimal W_ANTIGUEDAD = 0.15m;  // Experiencia y antigüedad laboral

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
            // RF16: CÁLCULO DEL SCORE PONDERADO
            // =========================================================================

            // 1. Puntaje Historial: menor cantidad de eventos = mayor prioridad (1.0 = 0 eventos)
            decimal puntajeHist = 1.0m - ((decimal)historico.CantidadEventosAnioActual / maxEventos);
            evaluacion.PuntajeHistorial = Math.Clamp(puntajeHist, 0.0m, 1.0m);

            // 2. Puntaje Recencia: más días sin eventos = mayor prioridad
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

            // 3. Puntaje Sábados: menor carga de sábados en los últimos 90 días = mayor disponibilidad
            decimal puntajeSab = 1.0m - ((decimal)historico.CantidadSabadosUltimos90Dias / maxSabados);
            evaluacion.PuntajeSabados = Math.Clamp(puntajeSab, 0.0m, 1.0m);

            // 4. Puntaje Antigüedad: escala sobre 25 años de servicio
            var aniosAntiguedad = (decimal)(fechaReferencia - empleado.FechaIngreso).TotalDays / 365.25m;
            decimal puntajeAnt = Math.Clamp(aniosAntiguedad / 25.0m, 0.0m, 1.0m);
            evaluacion.PuntajeAntiguedad = puntajeAnt;

            // Ptotal = w1*Phist + w2*Prec + w3*Psab + w4*Pant
            evaluacion.PuntajeTotal = Math.Round(
                (W_HISTORIAL * evaluacion.PuntajeHistorial) +
                (W_RECENCIA * evaluacion.PuntajeRecencia) +
                (W_SABADOS * evaluacion.PuntajeSabados) +
                (W_ANTIGUEDAD * evaluacion.PuntajeAntiguedad),
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
