using NarrativeSimulator.Core.Helpers;
using NarrativeSimulator.Core.Models.PsychProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace NarrativeSimulator.Core.Services;

public interface ISymantoClient
{
    Task<List<SymantoScoreResults>> ScoreTextsAsync(IEnumerable<SymantoScoreRequest> request, CancellationToken ct = default);
}

public class SymantoClient : ISymantoClient
{
    private readonly HttpClient _http;
    private readonly SymantoOptions _opts;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    public SymantoClient(HttpClient http, IOptions<SymantoOptions> opts)
    {
        _http = http;
        _opts = opts.Value;
        if (_http.BaseAddress is null && Uri.TryCreate(_opts.BaseUrl, UriKind.Absolute, out var uri))
            _http.BaseAddress = uri;
    }

    public async Task<List<SymantoScoreResults>> ScoreTextsAsync(IEnumerable<SymantoScoreRequest> request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opts.ApiKey))
            throw new InvalidOperationException("Symanto ApiKey is not configured.");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/big5");
        msg.Headers.TryAddWithoutValidation("x-rapidapi-key", _opts.ApiKey);
        msg.Headers.TryAddWithoutValidation("Accept", "application/json");

        var json = JsonSerializer.Serialize(request, JsonOpts);
        msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(msg, ct).ConfigureAwait(false);
        var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Symanto error {((int)res.StatusCode)}: {body}");
        var facets = JsonSerializer.Deserialize<List<FacetRecord>>(body, JsonOpts);
        var results = new List<SymantoScoreResults>();
        foreach (var facet in facets ?? [])
        {
            var sourceText = request.FirstOrDefault(r => r.Id == facet.Id)?.Text;
            results.Add(new SymantoScoreResults
            {
                Id = facet.Id,
                Scores = FacetToBig5Adapter.Translate(facet, sourceText: sourceText, lang: "en"),
                FacetScoreMap = facet.AsDoubleMap()
            });
        }
        Console.WriteLine($"Symanto Facets Score: \n=============================\n{JsonSerializer.Serialize(results)}\n=============================\n");
        return results;
    }
}
public sealed class FacetRecord
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    // Capture ALL other name:value pairs as facets on [0,1]
    [JsonExtensionData] public Dictionary<string, JsonElement>? Facets { get; set; }

    public Dictionary<string, double> AsDoubleMap()
    {
        var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        if (Facets is null) return dict;
        foreach (var kv in Facets)
        {
            if (kv.Value.ValueKind is JsonValueKind.Number && kv.Value.TryGetDouble(out var v))
                dict[kv.Key.Replace("-", "_").ToLowerInvariant()] = Math.Clamp(v, 0d, 1d);
        }
        return dict;
    }
}

// --- Adapter service: Facets → Sentino-shaped Big Five ---

public interface IFacetToBig5Adapter
{
    SentinoScoreResponse Translate(FacetRecord rec, string? sourceText = null, string lang = "en");
}

public sealed class FacetToBig5Adapter 
{
    // Facet sets per trait (lower_snake_case). Reverse-key where “high” means LOW trait.
    private static readonly string[] ODirect = ["openness", "adventurous", "artistic", "emotionally_aware", "imaginative", "intellectual"
    ];
    private static readonly string[] CDirect = ["conscientiousness", "cautious", "disciplined", "dutiful", "orderliness", "self_efficacy", "achievement_striving"
    ];
    private static readonly string[] EDirect = ["extraversion", "active", "assertive", "cheerful", "excitement_seeking", "outgoing", "gregariousness"
    ];
    private static readonly string[] ADirect = ["agreeableness", "cooperative", "trusting", "altruism", "modesty", "sympathy"
    ];
    private static readonly string[] AReverse = ["uncompromising", "authority_challenging"]; // higher = LOWER agreeableness
    private static readonly string[] NDirect = ["neuroticism", "melancholy", "self_conscious", "prone_to_worry", "stress_prone", "fiery", "immoderation"
    ];

    public static SentinoScoreResponse Translate(FacetRecord rec, string? sourceText = null, string lang = "en")
    {
        var f = rec.AsDoubleMap();

        var openness = Compose(f, ODirect);
        var conscientiousness = Compose(f, CDirect);
        var extraversion = Compose(f, EDirect);
        var agreeableness = Compose(f, ADirect, AReverse);
        var neuroticism = Compose(f, NDirect);

        return new SentinoScoreResponse
        {
            Text = sourceText,
            Inventories = ["big5"],
            Scoring = new SentinoScoring
            {
                Big5 = new SentinoBig5
                {
                    Openness = ToSentino(openness),
                    Conscientiousness = ToSentino(conscientiousness),
                    Extraversion = ToSentino(extraversion),
                    Agreeableness = ToSentino(agreeableness),
                    Neuroticism = ToSentino(neuroticism),
                }
            },
            Lang = lang
        };
    }

    // Average available indicators. If ≥3 facets exist, ignore the top-level key to avoid double counting.
    private static (double q, double conf) Compose(
        Dictionary<string, double> f, string[] direct, string[]? reverse = null)
    {
        reverse ??= [];
        var vals = new List<double>(8);

        // Count non-top-level facets to decide whether to include the top-level summary if present
        bool hasManyFacets = direct.Count(k => k is not ("openness" or "conscientiousness" or "extraversion" or "agreeableness" or "neuroticism") && f.ContainsKey(k)) >= 3;

        foreach (var k in direct)
        {
            if (hasManyFacets && IsTopLevel(k)) continue;
            if (f.TryGetValue(k, out var v)) vals.Add(v);
        }
        foreach (var k in reverse)
        {
            if (f.TryGetValue(k, out var v)) vals.Add(1 - v); // reverse-key
        }

        if (vals.Count == 0) return (double.NaN, 0d);

        var mean = vals.Average();
        var sd = Std(vals);
        // Normalize sd against Uniform(0,1) SD (≈0.2887) so “lower spread” → higher consistency.
        var consistency = 1 - Math.Min(sd / 0.28867513459481287, 1); // 0..1
        var coverage = Math.Clamp(vals.Count / 6.0, 0, 1);           // heuristic target ≈6 indicators
        var distance = Math.Abs(mean - 0.5) * 2;                     // 0 (ambiguous) .. 1 (extreme)

        // Blend into a confidence score. Weights are pragmatic and easy to tune.
        var confidence = Math.Clamp(0.4 * distance + 0.4 * consistency + 0.2 * coverage, 0, 1);

        // Treat mean on [0,1] as a proto-quantile until you have real norms.
        var quantile = Math.Clamp(mean, 0, 1);

        return (quantile, confidence);
    }

    private static SentinoTraitScore ToSentino((double q, double conf) x)
    {
        var (q, conf) = x;
        if (double.IsNaN(q)) return new SentinoTraitScore { Quantile = null, Score = null, Confidence = 0, ConfidenceText = "very low" };

        var score = q >= 0.5 ? 1.0 : -1.0; // mimic Sentino’s sign convention
        return new SentinoTraitScore
        {
            Quantile = Math.Round(q, 3),
            Score = score,
            Confidence = Math.Round(conf, 3),
            ConfidenceText = ConfidenceText(conf)
        };
    }

    private static string ConfidenceText(double c)
    {
        return c switch
        {
            >= 0.85 => "very high",
            >= 0.65 => "high",
            >= 0.45 => "normal",
            >= 0.25 => "low",
            _ => "very low"
        };
    }

    private static bool IsTopLevel(string k) =>
        k is "openness" or "conscientiousness" or "extraversion" or "agreeableness" or "neuroticism";

    private static double Std(IReadOnlyList<double> xs)
    {
        var m = xs.Average();
        var v = xs.Sum(vv => (vv - m) * (vv - m)) / xs.Count;
        return Math.Sqrt(v);
    }
}