using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NarrativeSimulator.Core.Models.PsychProfile;

public class SymantoScoreRequest(string id, string text, string language = "en")
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = id;

    [JsonPropertyName("language")] 
    public string Language { get; set; } = language;

    [JsonPropertyName("text")]
    public string Text { get; set; } = text;

    
}

public class SymantoScoreResults
{
    public string? Id { get; set; }
    public SentinoScoreResponse? Scores { get; set; }
    public Dictionary<string, double> FacetScoreMap { get; set; }
}
