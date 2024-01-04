using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BBBSBackend.DBModels;

public partial class AdditionalService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public double PricePerUnit { get; set; }

    public string ThumbnailImagePath { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<AdditionalServiceForBooking> AdditionalServiceForBookings { get; set; } = new List<AdditionalServiceForBooking>();
}
