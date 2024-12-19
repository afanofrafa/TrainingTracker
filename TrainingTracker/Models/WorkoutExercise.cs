using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class WorkoutExercise
{
    public long WeWorkoutId { get; set; }

    public int WeWexerciseId { get; set; }

    public long WeId { get; set; }

    public TimeOnly? WeRestTimeAfterExercise { get; set; }

    public sbyte? WeSequenceNumber { get; set; }
    [JsonIgnore]
    public virtual ICollection<Set>? Sets { get; set; } = new List<Set>();
    [JsonIgnore]
    public virtual Exercise? WeWexercise { get; set; } = null!;
    [JsonIgnore]
    public virtual Workout? WeWorkout { get; set; } = null!;
}
