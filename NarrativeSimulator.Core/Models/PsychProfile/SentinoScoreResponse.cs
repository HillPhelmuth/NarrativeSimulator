using System.Text.Json.Serialization;

namespace NarrativeSimulator.Core.Models.PsychProfile;

public sealed class SentinoScoreResponse
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("inventories")]
    public List<string>? Inventories { get; set; }

    [JsonPropertyName("scoring")]
    public SentinoScoring? Scoring { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }
}
public sealed class SentinoScoring
{
    [JsonPropertyName("big5")]
    public SentinoBig5? Big5 { get; set; }
}

public sealed class SentinoBig5
{
    [JsonPropertyName("agreeableness")]
    public SentinoTraitScore? Agreeableness { get; set; }

    [JsonPropertyName("conscientiousness")]
    public SentinoTraitScore? Conscientiousness { get; set; }

    [JsonPropertyName("extraversion")]
    public SentinoTraitScore? Extraversion { get; set; }

    [JsonPropertyName("neuroticism")]
    public SentinoTraitScore? Neuroticism { get; set; }

    [JsonPropertyName("openness")]
    public SentinoTraitScore? Openness { get; set; }

    public Dictionary<string, SentinoTraitScore> GetTraitScores()
    {
        // Using reflection to get all properties of this class
        var traitScores = new Dictionary<string, SentinoTraitScore>();
        var properties = typeof(SentinoBig5).GetProperties();
        foreach (var prop in properties)
        {
            if (prop.PropertyType == typeof(SentinoTraitScore))
            {
                var value = (SentinoTraitScore?)prop.GetValue(this);
                if (value != null)
                {
                    traitScores[prop.Name.ToLower()] = value; // Use lowercase keys
                }
            }
        }
        return traitScores;
    }
    public string ToMarkdown()
    {
        var sb = new System.Text.StringBuilder();
        var traitScores = GetTraitScores();
        foreach (var (trait, score) in traitScores)
        {
            sb.AppendLine($"- **{trait.ToUpper()}**: Quantile = {score.Quantile}, Confidence = {score.Confidence} ({score.ConfidenceText})");
        }
        return sb.ToString();
    }
}

public sealed class SentinoTraitScore
{
    [JsonPropertyName("quantile")]
    public double? Quantile { get; set; }

    // Note: API returns -1 / 1; keep double for future-proofing.
    [JsonPropertyName("score")]
    public double? Score { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("confidence_text")]
    public string? ConfidenceText { get; set; }
}