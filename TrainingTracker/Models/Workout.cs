using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class Workout
{
    public long WUserId { get; set; }

    public long WId { get; set; }

    public TimeOnly? WStartTime { get; set; }

    public TimeOnly? WEndTime { get; set; }

    public DateOnly? WDate { get; set; }

    public TimeOnly? WTotalDuration { get; set; }

    public sbyte? WSequenceNumber { get; set; }
    [JsonIgnore]
    public virtual User? WUser { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<WorkoutExercise>? WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
