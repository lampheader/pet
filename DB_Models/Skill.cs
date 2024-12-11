using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class Skill
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
