using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class PruebaaspContext : DbContext
{
    public PruebaaspContext()
    {
    }

    public PruebaaspContext(DbContextOptions<PruebaaspContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Token> Token { get; set; }
    public virtual DbSet<Dim_Usuarios> Dim_Usuario { get; set; }
    public virtual DbSet<Dim_Gestos> Dim_Gesto { get; set; }
    public virtual DbSet<Dim_Aparatos> Dim_Aparato { get; set; }
    public virtual DbSet<Dim_Tiempo> Dim_Tiempo { get; set; }
    public virtual DbSet<Fact_Historico_Actividad> Historico_Actividad { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        // 🔥 OBLIGAMOS A ENTIY FRAMEWORK A MAPEAR CORRECTAMENTE TU MODELO ANALÍTICO
        modelBuilder.Entity<Dim_Usuarios>(entity =>
        {
            // Definimos la llave primaria explícita
            entity.HasKey(e => e.sk_usuario_id);

            // Le indicamos el nombre exacto de la tabla física en MySQL
            entity.ToTable("dim_usuario");

            // Forzamos las propiedades y longitudes basadas en tus DataAnnotations
            entity.Property(e => e.sk_usuario_id).HasColumnName("sk_usuario_id");
            entity.Property(e => e.nombre_usuario).HasMaxLength(100).HasColumnName("nombre_usuario");
            entity.Property(e => e.email_usuario).HasMaxLength(150).HasColumnName("email_usuario");
            entity.Property(e => e.nombre_arduino).HasMaxLength(100).HasColumnName("nombre_arduino");
            entity.Property(e => e.mac_address_usuario).HasMaxLength(17).HasColumnName("mac_address_usuario");

            // 🚨 AQUÍ ESTÁ EL TRUCO: Forzamos la existencia física de la columna en la BD
            entity.Property(e => e.contrasenia).HasMaxLength(500).HasColumnName("contrasenia");
        });

        // Configuración para la tabla de Tokens de sesión
        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("token");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}