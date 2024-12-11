using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class Value
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public string? UnitText { get; set; }

    public int? MinValue { get; set; }

    public int? MaxValue { get; set; }

    public virtual ICollection<BaseSalary> BaseSalaries { get; set; } = new List<BaseSalary>();
}
