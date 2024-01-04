using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BBBSBackend.DBModels;

public partial class Customer
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool ReceiveNewsletter { get; set; }

    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
