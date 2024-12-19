using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingTracker.Models;

public partial class User
{
    public long UsId { get; set; }

    public string? UsName { get; set; }

    public string? UsEmail { get; set; }

    public string? UsLogin { get; set; }

    public short? UsStartWeight { get; set; }

    public short? UsHeight { get; set; }

    public sbyte? UsAge { get; set; }
    [MaxLength(255)]
    public string? UsPassword { get; set; }

    public short? UsCurrWeight { get; set; }

    public DateOnly? UsStartDate { get; set; }
    [JsonIgnore]
    public virtual ICollection<WeekUserStatistic>? WeekUserStatistics { get; set; } = new List<WeekUserStatistic>();
    [JsonIgnore]
    public virtual ICollection<Workout>? Workouts { get; set; } = new List<Workout>();
}
