using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class PlanificacionSabadoConfiguration : IEntityTypeConfiguration<PlanificacionSabado>
{
    public void Configure(EntityTypeBuilder<PlanificacionSabado> builder)
    {
        builder.ToTable("planificaciones_sabado");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id_planificacion_sabado")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.FechaSabado)
            .HasColumnName("fecha_sabado")
            .IsRequired();

        builder.Property(p => p.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(500);

        builder.Property(p => p.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(p => p.FechaConfirmacion)
            .HasColumnName("fecha_confirmacion");

        builder.Property(p => p.IdUsuarioCreacion)
            .HasColumnName("id_usuario_creacion")
            .IsRequired();

        builder.Property(p => p.IdUsuarioConfirmacion)
            .HasColumnName("id_usuario_confirmacion");

        // Relaciones con Usuario
        builder.HasOne(p => p.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(p => p.IdUsuarioCreacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UsuarioConfirmacion)
            .WithMany()
            .HasForeignKey(p => p.IdUsuarioConfirmacion)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con DetallePlanificacionSabado
        builder.HasMany(p => p.Detalles)
            .WithOne(d => d.PlanificacionSabado)
            .HasForeignKey(d => d.IdPlanificacionSabado)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(p => p.FechaSabado)
            .IsUnique();

        builder.HasIndex(p => p.Estado);
    }
}

