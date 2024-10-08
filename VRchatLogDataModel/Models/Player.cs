﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VRchatLogDataModel.Models;

[Table("Player")]
public partial class Player
{
    [Key]
    [Column("PlayerID")]
    public int PlayerId { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Column("isCurrentStaff")]
    public bool IsCurrentStaff { get; set; }

    [Column("isFormerStaff")]
    public bool IsFormerStaff { get; set; }

    [InverseProperty("Player")]
    public virtual ICollection<PlayerActivity> PlayerActivities { get; set; } = new List<PlayerActivity>();
}