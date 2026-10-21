using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class TurnoEventoConfiguration : IEntityTypeConfiguration<TurnoEvento>
{
    public void Configure(EntityTypeBuilder<TurnoEvento> builder)
    {
        builder.ToTable("turnos_evento");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id_turno_evento")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.IdEvento)
            .HasColumnName("id_evento")
            .IsRequired();

        builder.Property(t => t.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(t => t.HoraInicio)
            .HasColumnName("hora_inicio")
            .IsRequired();

        builder.Property(t => t.HoraFin)
            .HasColumnName("hora_fin")
            .IsRequired();

        builder.Property(t => t.CupoRequerido)
            .HasColumnName("cupo_requerido")
            .IsRequired();

        builder.HasOne(t => t.Evento)
            .WithMany(e => e.Turnos)
            .HasForeignKey(t => t.IdEvento)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Asignaciones)
            .WithOne(a => a.TurnoEvento)
            .HasForeignKey(a => a.IdTurnoEvento)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.IdEvento);
        builder.HasIndex(t => t.Fecha);
    }
}
