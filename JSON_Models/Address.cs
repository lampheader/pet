using System.Text.Json.Serialization;

namespace Json_Serializer.JSON_Models;

public partial class Address
{
	[JsonPropertyName("@type")]
	public string? type { get; set; }
	public string? addressLocality { get; set; }
	public string? addressRegion { get; set; }
	public string? addressCountry { get; set; }
	public string? streetAddress { get; set; }
};