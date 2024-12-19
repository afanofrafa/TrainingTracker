using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrainingTracker.Models;

public partial class Equipment
{
    public long EqSetId { get; set; }

    public long EqId { get; set; }

    public string? EqName { get; set; }

    public string? EqDescription { get; set; }

    public float? EqWeight { get; set; }
    [JsonIgnore]
    public virtual Set? EqSet { get; set; } = null!;
}
