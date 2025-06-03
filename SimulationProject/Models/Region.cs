using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("REGION")]
public partial class Region
{
    [Key]
    [Column("REGIONID")]
    public int Regionid { get; set; }

    [Column("REGIONNAME")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Regionname { get; set; }

    [Column("REGIONCODE")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Regioncode { get; set; }

    [Column("CLOUDID")]
    public int? Cloudid { get; set; }

    [ForeignKey("Cloudid")]
    [InverseProperty("Regions")]
    public virtual Cloudprovider? Cloud { get; set; }
}
