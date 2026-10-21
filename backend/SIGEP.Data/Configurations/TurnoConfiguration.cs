using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("turnos");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id_turno")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.HoraInicio)
            .HasColumnName("hora_inicio")
            .IsRequired();

        builder.Property(t => t.HoraFin)
            .HasColumnName("hora_fin")
            .IsRequired();

        builder.Property(t => t.RegimenId)
            .HasColumnName("id_regimen")
            .IsRequired();

        builder.Property(t => t.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(255);

        builder.HasOne(t => t.Regimen)
            .WithMany(r => r.Turnos)
            .HasForeignKey(t => t.RegimenId)
            .OnDelete(DeleteBehavior.Restrict);

        // Turnos oficiales relevados en el ERS Simoca (Sección 3.2.8)
        builder.HasData(
            // Régimen Incorporado
            new { Id = 1, Nombre = "Mañana", HoraInicio = new TimeOnly(6, 0), HoraFin = new TimeOnly(12, 0), RegimenId = 1, Descripcion = "Turno mañana Incorporado (06:00 - 12:00)" },
            new { Id = 2, Nombre = "Tarde", HoraInicio = new TimeOnly(12, 0), HoraFin = new TimeOnly(18, 0), RegimenId = 1, Descripcion = "Turno tarde Incorporado (12:00 - 18:00)" },
            new { Id = 3, Nombre = "Noche", HoraInicio = new TimeOnly(18, 0), HoraFin = new TimeOnly(23, 59, 59), RegimenId = 1, Descripcion = "Turno noche Incorporado (18:00 - 24:00)" },

            // Régimen Histórico
            new { Id = 4, Nombre = "Mañana", HoraInicio = new TimeOnly(7, 0), HoraFin = new TimeOnly(13, 0), RegimenId = 2, Descripcion = "Turno mañana Histórico (07:00 - 13:00)" },
            new { Id = 5, Nombre = "Siesta", HoraInicio = new TimeOnly(13, 0), HoraFin = new TimeOnly(19, 0), RegimenId = 2, Descripcion = "Turno siesta Histórico (13:00 - 19:00)" },
            new { Id = 6, Nombre = "Noche", HoraInicio = new TimeOnly(19, 0), HoraFin = new TimeOnly(7, 0), RegimenId = 2, Descripcion = "Turno noche Histórico (19:00 - 07:00) con 48 hs de descanso posterior" }
        );
    }
}

