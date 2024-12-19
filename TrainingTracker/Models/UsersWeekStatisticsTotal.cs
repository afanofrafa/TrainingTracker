using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class UsersWeekStatisticsTotal
{
    public int UwsExerciseId { get; set; }

    public int UwsId { get; set; }

    public DateOnly? UwsWeekStart { get; set; }

    public long? UwsWexerciseNum { get; set; }

    public long? UwsSetsNum { get; set; }

    public long? UwsUsersHaveDoneNum { get; set; }

    public long? UwsTotalEffort { get; set; }

    public long? UwsRepsNum { get; set; }

    public long? UwsRestTimeBtwSetsSec { get; set; }

    public long? UwsRestTimeAfterExercSec { get; set; }

    public double? UwsWeightLifted { get; set; }
    [JsonIgnore]
    public virtual Exercise UwsExercise { get; set; } = null!;
}
