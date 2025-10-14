using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("INSTANCETYPE")]
public partial class Instancetype
{
    [Key]
    [Column("INSTANCEID")]
    public int Instanceid { get; set; }

    [Column("INSTANCETYPE")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Instancetype1 { get; set; }

    [Column("CLOUDID")]
    public int? Cloudid { get; set; }

    [ForeignKey("Cloudid")]
    [InverseProperty("Instancetypes")]
    public virtual Cloudprovider? Cloud { get; set; }

    [InverseProperty("Instance")]
    public virtual ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
}
