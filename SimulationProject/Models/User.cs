using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimulationProject.Models;

[Table("USER")]
public partial class User
{
    [Key]
    [Column("USERID")]
    public int Userid { get; set; }

    [Column("USERNAME")]
    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("PASSWORD")]
    [StringLength(100)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [Column("FIRSTNAME")]
    [StringLength(100)]
    [Unicode(false)]
    public string Firstname { get; set; } = null!;

    [Column("LASTNAME")]
    [StringLength(100)]
    [Unicode(false)]
    public string Lastname { get; set; } = null!;

    [Column("EMAIL")]
    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("ADMIN")]
    public bool Admin { get; set; }

    [Column("AGE")]
    public int? Age { get; set; }

    [Column("JOBTITLE")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Jobtitle { get; set; }

    [Column("ROLE")]
    [StringLength(255)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [Column("REFRESHTOKEN")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Refreshtoken { get; set; }

    [Column("REFRESHTOKENEXPIRY", TypeName = "datetime")]
    public DateTime? Refreshtokenexpiry { get; set; }

    [Column("JWTID")]
    [StringLength(1000)]
    [Unicode(false)]
    public string? Jwtid { get; set; }

    [Column("SECURITYQUESTION")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityquestion { get; set; }

    [Column("SECURITYANSWER")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityanswer { get; set; }

    [Column("SECURITYQUESTION1")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityquestion1 { get; set; }

    [Column("SECURITYANSWER1")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityanswer1 { get; set; }

    [Column("SECURITYQUESTION2")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityquestion2 { get; set; }

    [Column("SECURITYANSWER2")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityanswer2 { get; set; }

    [Column("ORGANIZATION")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Organization { get; set; }

    [Column("ACTIVE")]
    public bool Active { get; set; }

    [Column("TEMPCODE")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Tempcode { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Cloudcredential> Cloudcredentials { get; set; } = new List<Cloudcredential>();

    [InverseProperty("SimuserNavigation")]
    public virtual ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
}
