using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class AsistenciaConfiguration : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> builder)
    {
        builder.ToTable("asistencias");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id_asistencia")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.EmpleadoId)
            .HasColumnName("id_empleado")
            .IsRequired();

        builder.Property(a => a.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(a => a.HoraIngreso)
            .HasColumnName("hora_ingreso");

        builder.Property(a => a.HoraSalida)
            .HasColumnName("hora_salida");

        builder.Property(a => a.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.TipoJornada)
            .HasColumnName("tipo_jornada")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.TurnoId)
            .HasColumnName("id_turno")
            .IsRequired();

        builder.Property(a => a.EventoId)
            .HasColumnName("id_evento");

        builder.Property(a => a.Observacion)
            .HasColumnName("observacion")
            .HasMaxLength(500);

        // Claves foráneas
        builder.HasOne(a => a.Empleado)
            .WithMany()
            .HasForeignKey(a => a.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Turno)
            .WithMany()
            .HasForeignKey(a => a.TurnoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único: Un empleado solo puede tener un registro de asistencia por fecha y turno
        builder.HasIndex(a => new { a.EmpleadoId, a.Fecha, a.TurnoId })
            .IsUnique();

        // Índices para optimizar consultas de historial y planilla diaria
        builder.HasIndex(a => a.Fecha);
        builder.HasIndex(a => new { a.Fecha, a.TurnoId });
        builder.HasIndex(a => a.TipoJornada);
    }
}

