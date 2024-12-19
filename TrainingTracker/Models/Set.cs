using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class Set
{
    public long SId { get; set; }

    public sbyte? SSequenceNumber { get; set; }

    public sbyte? SEffort { get; set; }

    public TimeOnly? SRestTimeAfterSet { get; set; }

    public string? SComments { get; set; }

    public short? SRepsDone { get; set; }

    public int SWexerciseId { get; set; }

    public long SWeId { get; set; }
    [JsonIgnore]
    public virtual ICollection<Equipment>? Equipment { get; set; } = new List<Equipment>();
    [JsonIgnore]
    public virtual WorkoutExercise? WorkoutExercise { get; set; } = null!;
}
