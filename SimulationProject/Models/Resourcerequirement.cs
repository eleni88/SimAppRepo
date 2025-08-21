using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("RESOURCEREQUIREMENT")]
[Index("Simid", Name = "UC_RESOURCE_SIMID", IsUnique = true)]
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
    [InverseProperty("Resourcerequirement")]
    public virtual Simulation? Sim { get; set; }
}
