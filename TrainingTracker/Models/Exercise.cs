using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class Exercise
{
    public int EId { get; set; }

    public string? EName { get; set; }

    public string? EDescription { get; set; }

    public sbyte? EEquipmentRequired { get; set; }

    public string? ETechniqueDifficultyLevel { get; set; }
    [JsonIgnore]
    public virtual ICollection<UsersWeekStatisticsTotal>? UsersWeekStatisticsTotals { get; set; } = new List<UsersWeekStatisticsTotal>();
    [JsonIgnore]
    public virtual ICollection<WeekExercisesStatistic>? WeekExercisesStatistics { get; set; } = new List<WeekExercisesStatistic>();
    [JsonIgnore]
    public virtual ICollection<WorkoutExercise>? WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
