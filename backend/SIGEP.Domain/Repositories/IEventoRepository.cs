using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Services;

namespace SIGEP.Domain.Repositories;

public interface IEventoRepository : IRepository<Evento>
{
    Task<Evento?> GetConDetallesByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Evento>> GetEventosActivosAsync(CancellationToken cancellationToken = default);
    Task<List<Evento>> GetHistorialEventosAsync(int limite = 20, CancellationToken cancellationToken = default);
    Task<Dictionary<int, InfoHistoricoEmpleado>> ObtenerHistoricoParaScoringAsync(DateOnly fechaEvento, CancellationToken cancellationToken = default);
}
