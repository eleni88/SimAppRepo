using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("CLOUDCREDENTIALS")]
public partial class Cloudcredential
{
    [Key]
    [Column("CREDID")]
    public int Credid { get; set; }

    [Column("USERNAME")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Username { get; set; }

    [Column("USERPASSSWORD")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Userpasssword { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("CLOUDID")]
    public int Cloudid { get; set; }

    [ForeignKey("Cloudid")]
    [InverseProperty("Cloudcredentials")]
    public virtual Cloudprovider Cloud { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("Cloudcredentials")]
    public virtual User User { get; set; } = null!;
}
