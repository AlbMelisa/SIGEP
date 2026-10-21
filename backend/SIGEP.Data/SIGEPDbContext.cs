using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using System.Reflection;

namespace SIGEP.Data;

/// <summary>
/// Contexto principal de Entity Framework Core para SIGEP.
/// Implementa IUnitOfWork para asegurar transaccionalidad atómica según el libro de Microsoft.
/// </summary>
public class SIGEPDbContext : DbContext, IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Funcion> Funciones => Set<Funcion>();
    public DbSet<Regimen> Regimenes => Set<Regimen>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<PlanificacionSabado> PlanificacionesSabado => Set<PlanificacionSabado>();
    public DbSet<DetallePlanificacionSabado> DetallesPlanificacionSabado => Set<DetallePlanificacionSabado>();

    public SIGEPDbContext(DbContextOptions<SIGEPDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones Fluent API del ensamblado Data
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}

