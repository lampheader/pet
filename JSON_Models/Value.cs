using System.Text.Json.Serialization;

namespace Json_Serializer.JSON_Models;
public partial class Value
{
    [JsonPropertyName("@type")]
    public string? type { get; set; }
    public string? unitText { get; set; }
    public int? minValue { get; set; }
    public int? maxValue { get; set; }
}
