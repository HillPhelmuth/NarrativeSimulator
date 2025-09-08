using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NarrativeSimulator.Core.Helpers;
using NarrativeSimulator.Core.Models.PsychProfile;


namespace NarrativeSimulator.Core.Services;

public interface ISentinoClient
{
    Task<SentinoScoreResponse> ScoreTextAsync(SentinoScoreRequest request, CancellationToken ct = default);
}

public sealed class SentinoClient : ISentinoClient
{
    private readonly HttpClient _http;
    private readonly SentinoOptions _opts;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SentinoClient(HttpClient http, IOptions<SentinoOptions> opts)
    {
        _http = http;
        _opts = opts.Value;
        if (_http.BaseAddress is null && Uri.TryCreate(_opts.BaseUrl, UriKind.Absolute, out var uri))
            _http.BaseAddress = uri;
    }

    public async Task<SentinoScoreResponse> ScoreTextAsync(SentinoScoreRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opts.ApiKey))
            throw new InvalidOperationException("Sentino ApiKey is not configured.");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "score/text");
        msg.Headers.TryAddWithoutValidation("x-rapidapi-key", _opts.ApiKey);
        msg.Headers.TryAddWithoutValidation("Accept", "application/json");

        var json = JsonSerializer.Serialize(request, JsonOpts);
        msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(msg, ct).ConfigureAwait(false);
        var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Sentino error {((int)res.StatusCode)}: {body}");

        var parsed = JsonSerializer.Deserialize<SentinoScoreResponse>(body, JsonOpts);
        if (parsed is null)
            throw new InvalidOperationException("Failed to parse Sentino response.");
        return parsed;
    }
}

