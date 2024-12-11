using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class DateStatus
{
    public int Id { get; set; }

    public DateTime? DatePosted { get; set; }

    public DateTime? ValIdThrough { get; set; }

    public DateTime? LastUpdate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
