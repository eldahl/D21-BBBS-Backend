using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BBBSBackend.DBModels;

public partial class AdditionalServiceForBooking
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public Guid BookingId { get; set; }

    public int AdditionalServiceId { get; set; }

    public virtual AdditionalService AdditionalService { get; set; } = null!;
    [JsonIgnore]
    public virtual Booking Booking { get; set; } = null!;
}
