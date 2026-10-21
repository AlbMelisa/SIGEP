using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class FuncionConfiguration : IEntityTypeConfiguration<Funcion>
{
    public void Configure(EntityTypeBuilder<Funcion> builder)
    {
        builder.ToTable("funciones");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id_funcion")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(f => f.Nombre)
            .IsUnique();

        builder.Property(f => f.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(255);

        // Seed inicial de funciones municipales
        builder.HasData(
            new { Id = 1, Nombre = "Servicios", Descripcion = "Mantenimiento, limpieza y servicios urbanos" },
            new { Id = 2, Nombre = "Tránsito", Descripcion = "Control vial y seguridad vehicular" },
            new { Id = 3, Nombre = "Administración", Descripcion = "Gestión documental y atención administrativa" },
            new { Id = 4, Nombre = "Cultura", Descripcion = "Eventos culturales y espacios públicos" },
            new { Id = 5, Nombre = "Desarrollo Social", Descripcion = "Asistencia comunitaria y programas sociales" }
        );
    }
}

