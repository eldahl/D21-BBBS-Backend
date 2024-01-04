using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BBBSBackend.DBModels;

public partial class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Capacity { get; set; }

    public double PricePerNight { get; set; }

    public string ThumbnailImagePath { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<RoomForBooking> RoomForBookings { get; set; } = new List<RoomForBooking>();
}
