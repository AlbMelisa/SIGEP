using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class DetallePlanificacionSabadoConfiguration : IEntityTypeConfiguration<DetallePlanificacionSabado>
{
    public void Configure(EntityTypeBuilder<DetallePlanificacionSabado> builder)
    {
        builder.ToTable("detalles_planificacion_sabado");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id_detalle_planificacion")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.IdPlanificacionSabado)
            .HasColumnName("id_planificacion_sabado")
            .IsRequired();

        builder.Property(d => d.IdEmpleado)
            .HasColumnName("id_empleado")
            .IsRequired();

        builder.Property(d => d.IdTurno)
            .HasColumnName("id_turno")
            .IsRequired();

        builder.Property(d => d.EsModificacionManual)
            .HasColumnName("es_modificacion_manual")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.MotivoModificacion)
            .HasColumnName("motivo_modificacion")
            .HasMaxLength(500);

        // Relaciones
        builder.HasOne(d => d.PlanificacionSabado)
            .WithMany(p => p.Detalles)
            .HasForeignKey(d => d.IdPlanificacionSabado)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Empleado)
            .WithMany()
            .HasForeignKey(d => d.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Turno)
            .WithMany()
            .HasForeignKey(d => d.IdTurno)
            .OnDelete(DeleteBehavior.Restrict);

        // Un empleado solo puede estar asignado una vez en la misma planificación de sábado
        builder.HasIndex(d => new { d.IdPlanificacionSabado, d.IdEmpleado })
            .IsUnique();

        builder.HasIndex(d => d.IdEmpleado);
        builder.HasIndex(d => d.IdTurno);
    }
}

