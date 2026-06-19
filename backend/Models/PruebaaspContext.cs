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
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Gesto> Gestos { get; set; }
    public virtual DbSet<Aparato> Aparatos { get; set; }
    public virtual DbSet<AparatoTipo> AparatoTipos { get; set; }
    public virtual DbSet<AparatoAccion> AparatoAcciones { get; set; }
    public virtual DbSet<AparatoBluetooth> AparatoBluetooth { get; set; }
    public virtual DbSet<AparatoConfiguracionRed> AparatoConfiguracionesRed { get; set; }
    public virtual DbSet<AparatoControl> AparatoControles { get; set; }
    public virtual DbSet<Tiempo> Tiempos { get; set; }
    public virtual DbSet<HistorialActividad> HistorialActividades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4")
            .HasAnnotation("MySql:Engine", "InnoDB");

        modelBuilder.Entity<Aparato>(entity =>
        {
            entity.HasKey(e => e.sk_aparato_id);
            entity.ToTable("aparato");

            entity.Property(e => e.sk_aparato_id).HasColumnName("sk_aparato_id");
            entity.Property(e => e.nombre_aparato).HasMaxLength(100).HasColumnName("nombre_aparato");
            entity.Property(e => e.icono).HasMaxLength(50).HasColumnName("icono");
            entity.Property(e => e.fecha_sincronizacion).HasColumnName("fecha_sincronizacion");
            entity.Property(e => e.sk_aparato_tipo_id).HasColumnName("sk_aparato_tipo_id");
            entity.Property(e => e.sk_aparato_accion_id).HasColumnName("sk_aparato_accion_id");
            entity.Property(e => e.sk_usuario_id).HasColumnName("sk_usuario_id");

            entity.HasOne(e => e.Tipo)
                .WithMany(e => e.Aparatos)
                .HasForeignKey(e => e.sk_aparato_tipo_id)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Accion)
                .WithMany(e => e.Aparatos)
                .HasForeignKey(e => e.sk_aparato_accion_id)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AparatoTipo>(entity =>
        {
            entity.HasKey(e => e.sk_aparato_tipo_id);
            entity.ToTable("aparato_tipo");

            entity.Property(e => e.sk_aparato_tipo_id).HasColumnName("sk_aparato_tipo_id");
            entity.Property(e => e.nombre_tipo).HasMaxLength(50).HasColumnName("nombre_tipo");
            entity.Property(e => e.icono).HasMaxLength(50).HasColumnName("icono");
            entity.Property(e => e.es_asistente).HasColumnName("es_asistente").HasDefaultValue(false);

            entity.HasIndex(e => e.nombre_tipo).IsUnique();
        });

        modelBuilder.Entity<AparatoAccion>(entity =>
        {
            entity.HasKey(e => e.sk_aparato_accion_id);
            entity.ToTable("aparato_accion");

            entity.Property(e => e.sk_aparato_accion_id).HasColumnName("sk_aparato_accion_id");
            entity.Property(e => e.accion_nombre).HasMaxLength(100).HasColumnName("accion_nombre");
            entity.Property(e => e.comando_bluetooth).HasMaxLength(50).HasColumnName("comando_bluetooth");

            entity.HasIndex(e => new { e.accion_nombre, e.comando_bluetooth }).IsUnique();
        });

        modelBuilder.Entity<AparatoBluetooth>(entity =>
        {
            entity.HasKey(e => e.sk_aparato_bluetooth_id);
            entity.ToTable("aparato_bluetooth");

            entity.Property(e => e.sk_aparato_bluetooth_id).HasColumnName("sk_aparato_bluetooth_id");
            entity.Property(e => e.sk_aparato_id).HasColumnName("sk_aparato_id");
            entity.Property(e => e.mac_bluetooth).HasMaxLength(17).HasColumnName("mac_bluetooth");
            entity.Property(e => e.nombre_bluetooth).HasMaxLength(100).HasColumnName("nombre_bluetooth");

            entity.HasIndex(e => e.sk_aparato_id).IsUnique();
            entity.HasOne(e => e.Aparato)
                .WithOne(e => e.Bluetooth)
                .HasForeignKey<AparatoBluetooth>(e => e.sk_aparato_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AparatoConfiguracionRed>(entity =>
        {
            entity.HasKey(e => e.sk_aparato_configuracion_red_id);
            entity.ToTable("aparato_configuracion_red");

            entity.Property(e => e.sk_aparato_configuracion_red_id).HasColumnName("sk_aparato_configuracion_red_id");
            entity.Property(e => e.sk_aparato_id).HasColumnName("sk_aparato_id");
            entity.Property(e => e.device_key).HasMaxLength(100).HasColumnName("device_key");
            entity.Property(e => e.ip_address).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.mac_address).HasMaxLength(17).HasColumnName("mac_address");
            entity.Property(e => e.host_name).HasMaxLength(100).HasColumnName("host_name");
            entity.Property(e => e.puerto_socket).HasColumnName("puerto_socket");
            entity.Property(e => e.protocolo_socket).HasMaxLength(20).HasColumnName("protocolo_socket");
            entity.Property(e => e.ruta_socket).HasMaxLength(200).HasColumnName("ruta_socket");
            entity.Property(e => e.activo).HasColumnName("activo");
            entity.Property(e => e.fecha_creacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.fecha_ultima_conexion).HasColumnName("fecha_ultima_conexion");

            entity.HasIndex(e => e.sk_aparato_id).IsUnique();
            entity.HasIndex(e => e.device_key).IsUnique();
            entity.HasOne(e => e.Aparato)
                .WithOne(e => e.ConfiguracionRed)
                .HasForeignKey<AparatoConfiguracionRed>(e => e.sk_aparato_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AparatoControl>(entity =>
        {
            entity.HasKey(e => e.sk_aparato_control_id);
            entity.ToTable("aparato_control");

            entity.Property(e => e.sk_aparato_control_id).HasColumnName("sk_aparato_control_id");
            entity.Property(e => e.sk_aparato_controlador_id).HasColumnName("sk_aparato_controlador_id");
            entity.Property(e => e.sk_aparato_controlado_id).HasColumnName("sk_aparato_controlado_id");
            entity.Property(e => e.comando_socket).HasMaxLength(100).HasColumnName("comando_socket");
            entity.Property(e => e.activo).HasColumnName("activo");
            entity.Property(e => e.fecha_creacion).HasColumnName("fecha_creacion");

            entity.HasIndex(e => new { e.sk_aparato_controlador_id, e.sk_aparato_controlado_id }).IsUnique();
            entity.HasOne(e => e.Controlador)
                .WithMany(e => e.AparatosControlados)
                .HasForeignKey(e => e.sk_aparato_controlador_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Controlado)
                .WithMany(e => e.Controladores)
                .HasForeignKey(e => e.sk_aparato_controlado_id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Gesto>(entity =>
        {
            entity.HasKey(e => e.sk_gesto_id);
            entity.ToTable("gesto");

            entity.Property(e => e.sk_gesto_id).HasColumnName("sk_gesto_id");
            entity.Property(e => e.bk_gesto_id).HasColumnName("bk_gesto_id");
            entity.Property(e => e.nombre_gesto).HasMaxLength(100).HasColumnName("nombre_gesto");
            entity.Property(e => e.identificador_ia).HasColumnName("identificador_ia");
            entity.Property(e => e.nivel_confianza_minimo).HasColumnName("nivel_confianza_minimo");
            entity.Property(e => e.tipo_disparador_nombre).HasMaxLength(100).HasColumnName("tipo_disparador_nombre");
            entity.Property(e => e.sk_aparato_id).HasColumnName("sk_aparato_id");
            entity.Property(e => e.sk_usuario_id).HasColumnName("sk_usuario_id");
        });

        modelBuilder.Entity<Tiempo>(entity =>
        {
            entity.HasKey(e => e.sk_tiempo_id);
            entity.ToTable("tiempo");

            entity.Property(e => e.sk_tiempo_id).HasColumnName("sk_tiempo_id");
            entity.Property(e => e.fecha_completa).HasColumnName("fecha_completa");
            entity.Property(e => e.anio).HasColumnName("anio");
            entity.Property(e => e.mes_numero).HasColumnName("mes_numero");
            entity.Property(e => e.mes_nombre).HasColumnName("mes_nombre");
            entity.Property(e => e.dia_semana_nombre).HasColumnName("dia_semana_nombre");
            entity.Property(e => e.hora_periodo).HasColumnName("hora_periodo");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.sk_usuario_id);
            entity.ToTable("usuario");

            entity.Property(e => e.sk_usuario_id).HasColumnName("sk_usuario_id");
            entity.Property(e => e.nombre_usuario).HasMaxLength(100).HasColumnName("nombre_usuario");
            entity.Property(e => e.email_usuario).HasMaxLength(150).HasColumnName("email_usuario");
            entity.Property(e => e.nombre_arduino).HasMaxLength(100).HasColumnName("nombre_arduino");
            entity.Property(e => e.mac_address_usuario).HasMaxLength(17).HasColumnName("mac_address_usuario");
            entity.Property(e => e.contrasenia).HasMaxLength(500).HasColumnName("contrasenia");
        });

        modelBuilder.Entity<HistorialActividad>(entity =>
        {
            entity.HasKey(e => e.sk_actividad_id);
            entity.ToTable("historial_actividad");

            entity.Property(e => e.sk_actividad_id).HasColumnName("sk_actividad_id");
            entity.Property(e => e.sk_usuario_id).HasColumnName("sk_usuario_id");
            entity.Property(e => e.sk_gesto_id).HasColumnName("sk_gesto_id");
            entity.Property(e => e.sk_aparato_id).HasColumnName("sk_aparato_id");
            entity.Property(e => e.sk_tiempo_id).HasColumnName("sk_tiempo_id");
            entity.Property(e => e.confianza_ia).HasColumnName("confianza_ia");
            entity.Property(e => e.tiempo_respuesta).HasColumnName("tiempo_respuesta");
            entity.Property(e => e.ejecucion_exitosa).HasColumnName("ejecucion_exitosa");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("token");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cadena).HasColumnName("cadena");
            entity.Property(e => e.FechaExpiracion).HasColumnName("fecha_expiracion");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.sk_usuario_id).HasColumnName("sk_usuario_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
