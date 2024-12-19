using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class WeekExercisesStatistic
{
    public long WesWkstId { get; set; }

    public int WesExerciseId { get; set; }

    public long WesId { get; set; }

    public DateOnly? WesWeekStart { get; set; }

    public float? WesTotalWeightLifted { get; set; }

    public sbyte? WesSetsNum { get; set; }

    public sbyte? WesTotalEffort { get; set; }

    public int? WesUsersWeekStatisticsId { get; set; }

    public short? WesTotalReps { get; set; }

    public TimeOnly? WesRestTimeBtwSetsTotal { get; set; }

    public TimeOnly? WesRestTimeAfterExerc { get; set; }
    [JsonIgnore]
    public virtual Exercise WesExercise { get; set; } = null!;
    [JsonIgnore]
    public virtual WeekUserStatistic WesWkst { get; set; } = null!;
}
