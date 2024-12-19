using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class WeekUserStatistic
{
    public long WkstUsId { get; set; }

    public long WkstId { get; set; }

    public DateOnly? WkstWeekStart { get; set; }

    public short? WkstStartUserWeight { get; set; }

    public short? WkstEndUserWeight { get; set; }

    public int? WkstTotalWeightLifted { get; set; }

    public short? WkstTotalEffort { get; set; }

    public TimeOnly? WkstTrainingDurationTotal { get; set; }

    public TimeOnly? WkstRestTimeBtwSetsTotal { get; set; }

    public short? WkstSequenceNumber { get; set; }

    public short? WkstTotalReps { get; set; }

    public sbyte? WkstTrainingNumber { get; set; }

    public TimeOnly? WkstRestTimeBtwExercTotal { get; set; }

    public sbyte? WkstExercisesNumber { get; set; }

    public sbyte? WkstTotalSets { get; set; }
    [JsonIgnore]
    public virtual ICollection<WeekExercisesStatistic> WeekExercisesStatistics { get; set; } = new List<WeekExercisesStatistic>();
    [JsonIgnore]
    public virtual User WkstUs { get; set; } = null!;
}
