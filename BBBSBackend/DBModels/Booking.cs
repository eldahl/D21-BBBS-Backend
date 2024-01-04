using System;
using System.Collections.Generic;

namespace BBBSBackend.DBModels;

public partial class Booking
{
    public Booking() { 
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime ArrivalDate { get; set; }

    public DateTime DepartureDate { get; set; }

    public int NumberOfPeople { get; set; }

    public bool Paid { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool ReservationTimeOut { get; set; }

    public bool Canceled { get; set; }

    public virtual ICollection<AdditionalServiceForBooking> AdditionalServiceForBookings { get; set; } = new List<AdditionalServiceForBooking>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<RoomForBooking> RoomForBookings { get; set; } = new List<RoomForBooking>();
}
