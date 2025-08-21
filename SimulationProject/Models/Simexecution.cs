using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("SIMEXECUTION")]
public partial class Simexecution
{
    [Key]
    [Column("EXECID")]
    public int Execid { get; set; }

    [Column("STATE")]
    [StringLength(200)]
    [Unicode(false)]
    public string? State { get; set; }

    [Column("COST")]
    public double? Cost { get; set; }

    [Column("STARTDATE", TypeName = "datetime")]
    public DateTime? Startdate { get; set; }

    [Column("ENDDATE", TypeName = "datetime")]
    public DateTime? Enddate { get; set; }

    [Column("EXECREPORT")]
    [StringLength(8000)]
    [Unicode(false)]
    public string? Execreport { get; set; }

    [Column("SIMID")]
    public int Simid { get; set; }

    [Column("duration")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Duration { get; set; }

    [Column("CURRENTINSTANCES")]
    public int? Currentinstances { get; set; }

    [ForeignKey("Simid")]
    [InverseProperty("Simexecutions")]
    public virtual Simulation Sim { get; set; } = null!;
}
