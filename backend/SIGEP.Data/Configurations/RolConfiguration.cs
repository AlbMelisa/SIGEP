using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

/// <summary>
/// Mapeo de la entidad Rol a la tabla de base de datos 'roles'.
/// Utiliza Fluent API para mantener el Dominio limpio de anotaciones de persistencia.
/// </summary>
public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id_rol")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(r => r.Nombre)
            .IsUnique();

        builder.Property(r => r.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(255);

        // Seed inicial de datos para roles requeridos
        builder.HasData(
            new { Id = 1, Nombre = Rol.RolJefe, Descripcion = "Administrador y control total de personal y eventos" },
            new { Id = 2, Nombre = Rol.RolEncargado, Descripcion = "Operador de asistencia diaria y planificaciones" }
        );
    }
}

