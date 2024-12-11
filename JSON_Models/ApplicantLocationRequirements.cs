using System.Text.Json.Serialization;

namespace Json_Serializer.JSON_Models;

public partial class ApplicantLocationRequirements
{
    [JsonPropertyName("@type")]
    public string? type { get; set; }
    public string? name { get; set; }
}