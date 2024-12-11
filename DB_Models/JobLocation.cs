using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class JobLocation
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public int? AddressId { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
