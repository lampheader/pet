using System;
using System.Collections.Generic;

namespace hh_db.DB_Models;

public partial class Vacancy
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string? Title { get; set; }

    public string? Experience { get; set; }

    public int? HiringOrganizationId { get; set; }

    public int? DateStatusId { get; set; }

    public int? JobLocationId { get; set; }

    public int? ApplicantLocationRequirementsId { get; set; }

    public string? EmploymentType { get; set; }

    public int? BaseSalaryId { get; set; }

    public virtual ApplicantsLocationRequirement? ApplicantLocationRequirements { get; set; }

    public virtual BaseSalary? BaseSalary { get; set; }

    public virtual DateStatus? DateStatus { get; set; }

    public virtual HiringOrganization? HiringOrganization { get; set; }

    public virtual Identifier IdNavigation { get; set; } = null!;

    public virtual JobLocation? JobLocation { get; set; }

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
