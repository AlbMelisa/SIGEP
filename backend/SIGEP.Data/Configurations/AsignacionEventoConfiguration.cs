using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class AsignacionEventoConfiguration : IEntityTypeConfiguration<AsignacionEvento>
{
    public void Configure(EntityTypeBuilder<AsignacionEvento> builder)
    {
        builder.ToTable("asignaciones_evento");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id_asignacion_evento")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.IdTurnoEvento)
            .HasColumnName("id_turno_evento")
            .IsRequired();

        builder.Property(a => a.IdEmpleado)
            .HasColumnName("id_empleado")
            .IsRequired();

        builder.Property(a => a.PuntajeCalculado)
            .HasColumnName("puntaje_calculado")
            .HasPrecision(8, 4)
            .IsRequired();

        builder.Property(a => a.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.EsAsignacionManual)
            .HasColumnName("es_asignacion_manual")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.MotivoModificacion)
            .HasColumnName("motivo_modificacion")
            .HasMaxLength(500);

        builder.HasOne(a => a.TurnoEvento)
            .WithMany(t => t.Asignaciones)
            .HasForeignKey(a => a.IdTurnoEvento)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Empleado)
            .WithMany()
            .HasForeignKey(a => a.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.IdTurnoEvento, a.IdEmpleado })
            .IsUnique();

        builder.HasIndex(a => a.IdEmpleado);
        builder.HasIndex(a => a.Estado);
    }
}

