using System.Text.Json.Serialization;

namespace Json_Serializer.JSON_Models;
public partial class BaseSalary
{
    [JsonPropertyName("@type")]
    public string? type { get; set; }
    public string? currency { get; set; }
    public Value value { get; set; }
}