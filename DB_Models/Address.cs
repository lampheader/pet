using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class Address
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public string? AddressLocality { get; set; }

    public string? AddressRegion { get; set; }

    public string? AddressCountry { get; set; }

    public string? StreetAddress { get; set; }

    public virtual ICollection<JobLocation> JobLocations { get; set; } = new List<JobLocation>();
}
