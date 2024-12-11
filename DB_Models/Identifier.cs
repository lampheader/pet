using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class Identifier
{
    public int Value { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public virtual Vacancy? Vacancy { get; set; }
}
