using System.Text.Json.Serialization;

namespace Json_Serializer.JSON_Models;
public partial class Vacancy
{
    [JsonPropertyName("@context")]
    public string? context { get; set; }

    [JsonPropertyName("@type")]
    public string? type { get; set; }
    public string? description { get; set; }
    public DateTime? datePosted { get; set; }
    public string? title { get; set; }
    public HiringOrganization? hiringOrganization { get; set; }
    public DateTime? validThrough { get; set; }
    public JobLocation? jobLocation { get; set; }
    public ApplicantLocationRequirements? applicantLocationRequirements { get; set; }
    public string? employmentType { get; set; }
    public BaseSalary? baseSalary { get; set; }
    public Identifier? identifier { get; set; }
}