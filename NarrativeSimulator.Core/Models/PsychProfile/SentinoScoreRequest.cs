using System.Text.Json.Serialization;

namespace NarrativeSimulator.Core.Models.PsychProfile;

public sealed class SentinoScoreRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("inventories")]
    public List<string> Inventories { get; set; } = ["big5"];

    [JsonPropertyName("lang")]
    public string? Lang { get; set; } = "en";
}