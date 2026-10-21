using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class RegimenConfiguration : IEntityTypeConfiguration<Regimen>
{
    public void Configure(EntityTypeBuilder<Regimen> builder)
    {
        builder.ToTable("regimenes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id_regimen")
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

        builder.HasData(
            new { Id = 1, Nombre = Regimen.Incorporado, Descripcion = "Jornada ordinaria lunes a viernes y sábados rotativos por mitades" },
            new { Id = 2, Nombre = Regimen.Historico, Descripcion = "Turnos rotativos con descanso obligatorio de 48 hs tras turno noche" }
        );
    }
}

