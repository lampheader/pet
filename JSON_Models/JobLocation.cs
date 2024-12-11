using System.Text.Json.Serialization;

namespace Json_Serializer.JSON_Models;
public partial class JobLocation
{
    [JsonPropertyName("@type")]
    public string type { get; set; }
    public Address? address { get; set; }
}