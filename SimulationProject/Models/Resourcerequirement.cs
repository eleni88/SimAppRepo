using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("RESOURCEREQUIREMENT")]
public partial class Resourcerequirement
{
    [Key]
    [Column("RESOURCEID")]
    public int Resourceid { get; set; }

    [Column("INSTANCETYPE")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Instancetype { get; set; }

    [Column("MININSTANCES")]
    public int? Mininstances { get; set; }

    [Column("MAXINSTANCES")]
    public int? Maxinstances { get; set; }

    [Column("SIMID")]
    public int? Simid { get; set; }

    [ForeignKey("Simid")]
    [InverseProperty("Resourcerequirements")]
    public virtual Simulation? Sim { get; set; }
}
