using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("SIMULATION")]
public partial class Simulation
{
    [Key]
    [Column("SIMID")]
    public int Simid { get; set; }

    [Column("NAME")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Name { get; set; }

    [Column("DESCRIPTION")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Description { get; set; }

    [Column("CODEURL")]
    [StringLength(500)]
    [Unicode(false)]
    public string? Codeurl { get; set; }

    [Column("SIMPARAMS")]
    public string? Simparams { get; set; }

    [Column("SIMUSER")]
    public int? Simuser { get; set; }

    [Column("SIMCLOUD")]
    public int? Simcloud { get; set; }

    [Column("CREATEDATE", TypeName = "datetime")]
    public DateTime Createdate { get; set; }

    [Column("UPDATEDATE", TypeName = "datetime")]
    public DateTime? Updatedate { get; set; }

    [Column("REGIONID")]
    public int? Regionid { get; set; }

    [ForeignKey("Regionid")]
    [InverseProperty("Simulations")]
    public virtual Region? Region { get; set; }

    [InverseProperty("Sim")]
    public virtual ICollection<Resourcerequirement> Resourcerequirements { get; set; } = new List<Resourcerequirement>();

    [ForeignKey("Simcloud")]
    [InverseProperty("Simulations")]
    public virtual Cloudprovider? SimcloudNavigation { get; set; }

    [InverseProperty("Sim")]
    public virtual ICollection<Simexecution> Simexecutions { get; set; } = new List<Simexecution>();

    [ForeignKey("Simuser")]
    [InverseProperty("Simulations")]
    public virtual User? SimuserNavigation { get; set; }
}
