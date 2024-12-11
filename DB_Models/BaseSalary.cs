using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class BaseSalary
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public string? Currency { get; set; }

    public int ValueId { get; set; }

    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();

    public virtual Value Value { get; set; } = null!;
}
