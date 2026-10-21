using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id_empleado")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Apellido)
            .HasColumnName("apellido")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Documento)
            .HasColumnName("documento")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(e => e.Documento)
            .IsUnique();

        builder.Property(e => e.Direccion)
            .HasColumnName("direccion")
            .HasMaxLength(255);

        builder.Property(e => e.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(150);

        builder.Property(e => e.FuncionId)
            .HasColumnName("id_funcion")
            .IsRequired();

        builder.Property(e => e.RegimenId)
            .HasColumnName("id_regimen")
            .IsRequired();

        builder.Property(e => e.TurnoHabitualId)
            .HasColumnName("id_turno_habitual")
            .IsRequired();

        builder.Property(e => e.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.FechaIngreso)
            .HasColumnName("fecha_ingreso")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relaciones con claves foráneas protegidas
        builder.HasOne(e => e.Funcion)
            .WithMany()
            .HasForeignKey(e => e.FuncionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Regimen)
            .WithMany()
            .HasForeignKey(e => e.RegimenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TurnoHabitual)
            .WithMany()
            .HasForeignKey(e => e.TurnoHabitualId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para optimizar consultas de filtrado (RNF05)
        builder.HasIndex(e => e.RegimenId);
        builder.HasIndex(e => e.Estado);
    }
}

