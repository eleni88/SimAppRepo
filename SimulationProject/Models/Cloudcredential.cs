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

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("CLOUDID")]
    public int Cloudid { get; set; }

    [Column("ACCESSKEYID")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Accesskeyid { get; set; }

    [Column("SECRETACCESSKEY")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Secretaccesskey { get; set; }

    [Column("CLIENTID")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Clientid { get; set; }

    [Column("CLIENTSECRET")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Clientsecret { get; set; }

    [Column("TENANTID")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Tenantid { get; set; }

    [Column("SUBSCRIPTIONID")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Subscriptionid { get; set; }

    [Column("GCPSERVICEKEYJSON", TypeName = "text")]
    public string? Gcpservicekeyjson { get; set; }

    [Column("GCPPROJECTID")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Gcpprojectid { get; set; }

    [Column("GCPREGION")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Gcpregion { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Cloudid")]
    [InverseProperty("Cloudcredentials")]
    public virtual Cloudprovider Cloud { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("Cloudcredentials")]
    public virtual User User { get; set; } = null!;
}
