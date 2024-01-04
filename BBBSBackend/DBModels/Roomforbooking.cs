using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BBBSBackend.DBModels;

public partial class RoomForBooking
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public Guid BookingId { get; set; }

    public int RoomId { get; set; }
    [JsonIgnore]
    public virtual Booking Booking { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
