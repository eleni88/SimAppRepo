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

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Greek_100_CI_AI");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK__USER__7B9E7F35CC9ADE84");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
