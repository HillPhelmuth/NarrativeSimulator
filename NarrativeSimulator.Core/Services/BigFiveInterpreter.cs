using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NarrativeSimulator.Core.Models.PsychProfile;
using System.Text.Json.Serialization;

namespace NarrativeSimulator.Core.Services;



public record TraitBand(string Band, int Score); // Score is 0-100

public record IndividualWriteup(
    TraitBand Openness,
    TraitBand Conscientiousness,
    TraitBand Extraversion,
    TraitBand Agreeableness,
    TraitBand Neuroticism,
    Dictionary<string, string> Paragraphs)
{
    public string ToMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine("| Trait | Band | Score | Description |");
        sb.AppendLine("|-------|------|-------|-------------|");
        sb.AppendLine($"| Openness | {Openness.Band} | {Openness.Score} | {Paragraphs["Openness"]} |");
        sb.AppendLine($"| Conscientiousness | {Conscientiousness.Band} | {Conscientiousness.Score} | {Paragraphs["Conscientiousness"]} |");
        sb.AppendLine($"| Extraversion | {Extraversion.Band} | {Extraversion.Score} | {Paragraphs["Extraversion"]} |");
        sb.AppendLine($"| Agreeableness | {Agreeableness.Band} | {Agreeableness.Score} | {Paragraphs["Agreeableness"]} |");
        sb.AppendLine($"| Neuroticism | {Neuroticism.Band} | {Neuroticism.Score} | {Paragraphs["Neuroticism"]} |");
        return sb.ToString();
    }
}

public record TeamReport(
    Dictionary<string, TeamMetric> Metrics,
    List<string> Highlights,
    List<string> Risks)
{
    public string ToMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Team Personality Report");
        sb.AppendLine();
        sb.AppendLine("| Trait | Mean Score | Std Dev |");
        sb.AppendLine("|-------|------------|---------|");
        foreach (var (trait, metric) in Metrics)
        {
            sb.AppendLine($"| {trait} | {metric.Mean:F1} | {metric.Sd:F1} |");
        }
        sb.AppendLine();
        if (Highlights.Count > 0)
        {
            sb.AppendLine("### Highlights");
            foreach (var h in Highlights) sb.AppendLine($"- {h}");
            sb.AppendLine();
        }
        if (Risks.Count > 0)
        {
            sb.AppendLine("### Risks");
            foreach (var r in Risks) sb.AppendLine($"- {r}");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
public record TeamMetric(double Mean, double Sd);
// === New interpreter interface bound to Sentino types ===
public interface IBigFiveInterpreter
{
    IndividualWriteup Interpret(SentinoBig5 big5);
    IndividualWriteup Interpret(SentinoScoreResponse response) =>
        response.Scoring?.Big5 is { } b ? Interpret(b) :
        throw new InvalidOperationException("Sentino response has no Big5 scoring.");

    TeamReport InterpretTeam(Dictionary<string, SentinoBig5> team);
    TeamReport InterpretTeam(Dictionary<string,SentinoScoreResponse> team) =>
        InterpretTeam(team.ToDictionary(t => t.Key, t=> t.Value.Scoring?.Big5 ?? new SentinoBig5()));
}

public class BigFiveInterpreter : IBigFiveInterpreter
{
    private readonly int lowCut = 35, highCut = 65;

    // Short, neutral blurbs (edit freely).
    private readonly Dictionary<string, (string low, string mid, string high)> _text = new()
    {
        ["Openness"] = ("prefers the familiar and proven; values practicality",
                        "balances novelty with pragmatism",
                        "curious and idea-driven; seeks novelty and abstraction"),
        ["Conscientiousness"] = ("flexible and spontaneous; may defer structure",
                                 "keeps a workable level of organization",
                                 "planned, disciplined, and goal-focused"),
        ["Extraversion"] = ("reserved, energy from solo focus; communicates selectively",
                            "situationally social; adapts to the room",
                            "outgoing, talkative, and initiative-forward"),
        ["Agreeableness"] = ("direct and competitive; challenges ideas openly",
                             "cooperative in most contexts",
                             "warm, tactful, and collaboration-oriented"),
        ["Neuroticism"] = ("even-keeled under pressure",
                           "generally steady with situational stress",
                           "stress-sensitive; vigilant to threats")
    };

    public IndividualWriteup Interpret(SentinoBig5 s)
    {
        // Convert Sentino quantiles (0..1) to percent (0..100). Missing → 50 (neutral).
        var o = Pct(s.Openness);
        var c = Pct(s.Conscientiousness);
        var e = Pct(s.Extraversion);
        var a = Pct(s.Agreeableness);
        var n = Pct(s.Neuroticism);

        var bands = new Dictionary<string, TraitBand>
        {
            ["Openness"] = Band(o),
            ["Conscientiousness"] = Band(c),
            ["Extraversion"] = Band(e),
            ["Agreeableness"] = Band(a),
            ["Neuroticism"] = Band(n)
        };

        var paras = bands.ToDictionary(kv => kv.Key, kv => PickText(kv.Key, kv.Value.Score));
        return new IndividualWriteup(bands["Openness"], bands["Conscientiousness"],
                                     bands["Extraversion"], bands["Agreeableness"], bands["Neuroticism"], paras);
    }

    public TeamReport InterpretTeam(Dictionary<string, SentinoBig5> team)
    {
        // Build vectors of percents for each trait
        var opennessValues = new List<double>();
        var conscientiousnessValues = new List<double>(); 
        var extraversionValues = new List<double>();
        var agreeablenessValues = new List<double>(); 
        var neuroticismValues = new List<double>();

        foreach (var (_, s) in team)
        {
            opennessValues.Add(Pct(s.Openness));
            conscientiousnessValues.Add(Pct(s.Conscientiousness));
            extraversionValues.Add(Pct(s.Extraversion));
            agreeablenessValues.Add(Pct(s.Agreeableness));
            neuroticismValues.Add(Pct(s.Neuroticism));
        }

        var metrics = new Dictionary<string, TeamMetric>
        {
            ["Openness"] = Stat(opennessValues),
            ["Conscientiousness"] = Stat(conscientiousnessValues),
            ["Extraversion"] = Stat(extraversionValues),
            ["Agreeableness"] = Stat(agreeablenessValues),
            ["Neuroticism"] = Stat(neuroticismValues)
        };

        var highlights = new List<string>();
        var risks = new List<string>();
        Console.WriteLine($"Interpreted Metrics:\n{JsonSerializer.Serialize(metrics)}");
        if (metrics["Agreeableness"].Mean >= 60) highlights.Add("High team agreeableness → smoother coordination.");
        if (metrics["Conscientiousness"].Mean >= 60) highlights.Add("High conscientiousness → planning and follow-through.");
        if (metrics["Agreeableness"].Sd > 13) risks.Add("Big spread on agreeableness → style friction (blunt vs. diplomatic).");
        if (metrics["Conscientiousness"].Sd > 13) risks.Add("Uneven conscientiousness → bottlenecks on deadlines.");
        if (metrics["Extraversion"].Sd > 13) risks.Add("Loud voices may drown out quiet signal; consider facilitation.");

        return new TeamReport(metrics, highlights, risks);
    }

    // --- Helpers ---
    private static int Pct(SentinoTraitScore? t)
    {
        // Quantile is 0..1. If missing, return neutral 50 to avoid skewing the UI.
        var q = t?.Quantile;
        if (q is null || double.IsNaN(q.Value)) return 50;
        var pct = (int)Math.Round(Math.Clamp(q.Value, 0, 1) * 100);
        return Math.Clamp(pct, 0, 100);
    }

    private TraitBand Band(int pct)
    {
        var band = pct <= lowCut ? "Low" : pct >= highCut ? "High" : "Typical";
        return new TraitBand(band, pct);
    }

    private string PickText(string trait, int pct)
    {
        var (low, mid, high) = _text[trait];
        return pct <= lowCut ? low : pct >= highCut ? high : mid;
    }

    private static TeamMetric Stat(IReadOnlyList<double> xs)
    {
        if (xs.Count == 0) return new TeamMetric(0, 0);
        var mean = xs.Average();
        var sd = Math.Sqrt(xs.Sum(v => (v - mean) * (v - mean)) / xs.Count);
        return new TeamMetric(mean, sd);
    }
}
