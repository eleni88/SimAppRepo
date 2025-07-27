using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SimulationProject.Models;

namespace SimulationProject.Data;

public partial class SimSaasContext : DbContext
{
    public SimSaasContext()
    {
    }

    public SimSaasContext(DbContextOptions<SimSaasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cloudcredential> Cloudcredentials { get; set; }

    public virtual DbSet<Cloudprovider> Cloudproviders { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Resourcerequirement> Resourcerequirements { get; set; }

    public virtual DbSet<Simexecution> Simexecutions { get; set; }

    public virtual DbSet<Simulation> Simulations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Greek_100_CI_AI");

        modelBuilder.Entity<Cloudcredential>(entity =>
        {
            entity.HasKey(e => e.Credid).HasName("PK_CREDID");

            entity.HasOne(d => d.Cloud).WithMany(p => p.Cloudcredentials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UCLOUDID");

            entity.HasOne(d => d.User).WithMany(p => p.Cloudcredentials).HasConstraintName("FK_USERID");
        });

        modelBuilder.Entity<Cloudprovider>(entity =>
        {
            entity.HasKey(e => e.Cloudid).HasName("PK_CLOUDID");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasOne(d => d.Cloud).WithMany(p => p.Regions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CLOUDID");
        });

        modelBuilder.Entity<Resourcerequirement>(entity =>
        {
            entity.HasKey(e => e.Resourceid).HasName("PK_RESOURCEID");
        });

        modelBuilder.Entity<Simexecution>(entity =>
        {
            entity.HasKey(e => e.Execid).HasName("PK_EXECID");

            entity.HasOne(d => d.Sim).WithMany(p => p.Simexecutions).HasConstraintName("FK_SIMID");
        });

        modelBuilder.Entity<Simulation>(entity =>
        {
            entity.HasKey(e => e.Simid).HasName("PK_SIMULID");

            entity.HasOne(d => d.Region).WithMany(p => p.Simulations).HasConstraintName("FK_REGIONID");

            entity.HasOne(d => d.SimcloudNavigation).WithMany(p => p.Simulations).HasConstraintName("FK_SIMCLOUD");

            entity.HasOne(d => d.SimuserNavigation).WithMany(p => p.Simulations)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SIMUSER");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK__USER__7B9E7F35CC9ADE84");

            entity.Property(e => e.Active).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
