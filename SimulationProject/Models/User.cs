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
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Column("ADMIN")]
    public bool Admin { get; set; }

    [Column("AGE")]
    public int? Age { get; set; }

    [Column("JOBTITLE")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Jobtitle { get; set; }

    [Column("SECURITYQUESTION")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityquestion { get; set; }

    [Column("SECURITYANSWER")]
    [StringLength(200)]
    [Unicode(false)]
    public string? Securityanswer { get; set; }

    [Column("ROLE")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Role { get; set; }
}
