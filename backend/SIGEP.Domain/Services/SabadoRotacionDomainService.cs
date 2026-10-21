using SIGEP.Domain.Entities;

namespace SIGEP.Domain.Services;

/// <summary>
/// Implementación del Servicio de Dominio para el Algoritmo de Rotación de Sábados (RF08 - RF11).
/// Diseñado con reglas puras de dominio libres de dependencias de persistencia.
/// </summary>
public class SabadoRotacionDomainService : ISabadoRotacionDomainService
{
    public PlanificacionSabado GenerarPropuestaPreliminar(
        DateOnly fechaSabado,
        int idUsuarioCreacion,
        IReadOnlyList<Empleado> empleadosIncorporadosActivos,
        IReadOnlyList<Turno> turnosDisponibles,
        IReadOnlyDictionary<int, DateOnly> ultimoSabadoTrabajadoPorEmpleado,
        IReadOnlyDictionary<int, int> ultimoTurnoSabadoPorEmpleado,
        string? observaciones = null)
    {
        if (fechaSabado.DayOfWeek != DayOfWeek.Saturday)
            throw new InvalidOperationException($"La fecha {fechaSabado:dd/MM/yyyy} debe ser sábado.");

        if (empleadosIncorporadosActivos == null || !empleadosIncorporadosActivos.Any())
            throw new InvalidOperationException("No existen empleados activos en el Régimen Incorporado para planificar.");

        if (turnosDisponibles == null || !turnosDisponibles.Any())
            throw new InvalidOperationException("No existen turnos configurados para la asignación de sábados.");

        var planificacion = new PlanificacionSabado(fechaSabado, idUsuarioCreacion, observaciones);

        // RF09: Exactamente el 50% de la dotación activa (entero superior si es impar)
        int totalEmpleados = empleadosIncorporadosActivos.Count;
        int cupoRequerido = (int)Math.Ceiling(totalEmpleados / 2.0);

        var sabadoAnterior = fechaSabado.AddDays(-7);

        // RF10: Priorización por alternancia quincenal estricta
        // Prioridad 1: Quienes NO trabajaron el sábado anterior (o nunca trabajaron)
        // Criterio de orden:
        // 1. TrabajoElSabadoAnterior (false primero)
        // 2. UltimaFechaTrabajada (las más antiguas primero, DateOnly.MinValue si nunca trabajó)
        // 3. IdEmpleado para desempate determinista
        var empleadosOrdenados = empleadosIncorporadosActivos
            .Select(emp =>
            {
                var tieneRegistro = ultimoSabadoTrabajadoPorEmpleado.TryGetValue(emp.Id, out var ultimaFecha);
                bool trabajoSabadoAnterior = tieneRegistro && ultimaFecha == sabadoAnterior;
                DateOnly fechaReferencia = tieneRegistro ? ultimaFecha : DateOnly.MinValue;

                return new
                {
                    Empleado = emp,
                    TrabajoSabadoAnterior = trabajoSabadoAnterior,
                    UltimaFechaTrabajada = fechaReferencia
                };
            })
            .OrderBy(x => x.TrabajoSabadoAnterior) // false (0) viene antes que true (1)
            .ThenBy(x => x.UltimaFechaTrabajada)
            .ThenBy(x => x.Empleado.Id)
            .Take(cupoRequerido)
            .Select(x => x.Empleado)
            .ToList();

        // RF11: Distribución equitativa entre turnos operativos y alternancia de horario
        var turnosOrdenados = turnosDisponibles.OrderBy(t => t.HoraInicio).ToList();
        var conteoPorTurno = turnosOrdenados.ToDictionary(t => t.Id, _ => 0);

        foreach (var empleado in empleadosOrdenados)
        {
            ultimoTurnoSabadoPorEmpleado.TryGetValue(empleado.Id, out int ultimoTurnoId);

            // Seleccionar el turno con menor cantidad de personas asignadas.
            // Si hay empate en conteo, preferir un turno diferente al realizado en su último sábado.
            var turnoElegido = turnosOrdenados
                .OrderBy(t => conteoPorTurno[t.Id])
                .ThenBy(t => t.Id == ultimoTurnoId ? 1 : 0) // desempate: prioriza no repetir turno
                .First();

            planificacion.AgregarAsignacion(empleado.Id, turnoElegido.Id, esManual: false, motivo: null);
            conteoPorTurno[turnoElegido.Id]++;
        }

        return planificacion;
    }
}

