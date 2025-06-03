using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("CLOUDPROVIDER")]
public partial class Cloudprovider
{
    [Key]
    [Column("CLOUDID")]
    public int Cloudid { get; set; }

    [Column("NAME")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Name { get; set; }

    [InverseProperty("Cloud")]
    public virtual ICollection<Cloudcredential> Cloudcredentials { get; set; } = new List<Cloudcredential>();

    [InverseProperty("Cloud")]
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();

    [InverseProperty("SimcloudNavigation")]
    public virtual ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
}
