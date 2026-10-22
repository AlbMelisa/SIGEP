using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("eventos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id_evento")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500);

        builder.Property(e => e.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(e => e.FechaFin)
            .HasColumnName("fecha_fin")
            .IsRequired();

        builder.Property(e => e.Lugar)
            .HasColumnName("lugar")
            .HasMaxLength(200);

        builder.Property(e => e.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(e => e.FechaConfirmacion)
            .HasColumnName("fecha_confirmacion");

        builder.Property(e => e.IdUsuarioCreacion)
            .HasColumnName("id_usuario_creacion")
            .IsRequired();

        builder.Property(e => e.IdUsuarioConfirmacion)
            .HasColumnName("id_usuario_confirmacion");

        // Relaciones
        builder.HasOne(e => e.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(e => e.IdUsuarioCreacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UsuarioConfirmacion)
            .WithMany()
            .HasForeignKey(e => e.IdUsuarioConfirmacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Turnos)
            .WithOne(t => t.Evento)
            .HasForeignKey(t => t.IdEvento)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Estado);
        builder.HasIndex(e => new { e.FechaInicio, e.FechaFin });
    }
}

