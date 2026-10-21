using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEP.Domain.Entities;

namespace SIGEP.Data.Configurations;

/// <summary>
/// Mapeo de la entidad Usuario a la tabla 'usuarios'.
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id_usuario")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.RolId)
            .HasColumnName("id_rol")
            .IsRequired();

        builder.Property(u => u.EmpleadoId)
            .HasColumnName("id_empleado");

        builder.Property(u => u.TelegramChatId)
            .HasColumnName("telegram_chat_id")
            .HasMaxLength(100);

        builder.HasIndex(u => u.TelegramChatId)
            .IsUnique()
            .HasFilter("telegram_chat_id IS NOT NULL");

        builder.Property(u => u.Estado)
            .HasColumnName("estado")
            .HasConversion<string>() // Guarda el enum como texto ('Activo', 'Inactivo')
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UltimoAcceso)
            .HasColumnName("ultimo_acceso");

        // Relación con Rol
        builder.HasOne(u => u.Rol)
            .WithMany()
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

