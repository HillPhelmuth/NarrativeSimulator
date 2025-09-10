using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NarrativeSimulator.Core.Models;

namespace NarrativeSimulator.Core.Services;

public interface IBeatEngine
{
    // Push raw actions into the engine as they arrive
    void Add(WorldAgentAction action);

    // Start/stop the windowing loop
    Task StartAsync(string name, string description, CancellationToken ct = default);
    Task StopAsync();

    // Fired whenever a beat is produced
    event Action<BeatSummary>? OnBeat;
}

public sealed class BeatEngine(INarrativeOrchestration orchestration) : IBeatEngine, IDisposable
{
    private readonly TimeSpan _window = TimeSpan.FromSeconds(5); // tune: 3–8s works well
    private int _minActions = 6;
    private string _model = "openai/gpt-oss-20b";
    private readonly ConcurrentQueue<WorldAgentAction> _buffer = new();
    private Timer? _timer;
    private volatile bool _running;
    private readonly TimeSpan _idleFlush = TimeSpan.FromMinutes(2);
    public event Action<BeatSummary>? OnBeat;
    private DateTime _lastBeatUtc = DateTime.MinValue;
    private List<BeatSummary> _beatHistory = [];
    private string? _worldName;
    private string? _worldDescription;
    public void Add(WorldAgentAction action)
    {
        if (action.Type is ActionType.None or ActionType.Error) return;
        Console.WriteLine($"Add action to buffer from: {action.ActingAgent}");
        _buffer.Enqueue(action);
    }

    public Task StartAsync(string name, string description, CancellationToken ct = default)
    {
        _worldName = name;
        _worldDescription = description;
        if (_running) return Task.CompletedTask;
        _running = true;
        _timer = new Timer(async _ => await TryMakeBeatAsync(ct), null,
            dueTime: _window, period: _window);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _running = false;
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }

    private async Task TryMakeBeatAsync(CancellationToken ct)
    {
        if (!_running) return;

        var now = DateTime.UtcNow;

        // Fast path: nothing to do
        if (_buffer.IsEmpty) return;

      
        var count = _buffer.Count; // fine here; perf is OK for typical sizes
        var dueToIdle = false;

        if (count < 6 && !dueToIdle)
            return; // don't drain yet — keep accumulating

        // Build a batch to summarize.
        var take = Math.Min(count, 6);
        var batch = DequeueUpTo(take);

        if (batch.Length == 0) return;

        var start = batch.Min(a => a.Timestamp).ToUniversalTime();
        var end = batch.Max(a => a.Timestamp).ToUniversalTime();

        var payload = string.Join("\n\n", batch.Select(a => a.ToTypeMarkdown()).Select(x => $"{x.Item1} -> {x.Item2}"));

        var prompt = BuildBeatPrompt(_worldName, _worldDescription, payload, start, end, _beatHistory);
        var beatResponseJson = "";
        try
        {
            var settings = new OpenAIPromptExecutionSettings()
            { ResponseFormat = typeof(BeatSummary), ReasoningEffort = "high" };
            beatResponseJson = await orchestration.ExecuteLlmPrompt(prompt, _model, settings, ct);
            var beat = JsonSerializer.Deserialize<BeatSummary>(beatResponseJson);
            if (beat is null) return;

            beat.WindowStartUtc = start;
            beat.WindowEndUtc = end;
            beat.SourceActionCount = batch.Length;
            beat.BeatId = Hash($"{beat.ContinuityKey}|{start:O}|{end:O}");

            OnBeat?.Invoke(beat);
            _lastBeatUtc = now;
            _beatHistory.Add(beat); // store full beat for history context
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TryAddBeat failed.Output: {beatResponseJson}\n\nError:\n{ex}");
        }
    }

    private WorldAgentAction[] DequeueUpTo(int n)
    {
        var list = new List<WorldAgentAction>(Math.Max(1, n));
        while (n > 0 && _buffer.TryDequeue(out var a))
        {
            list.Add(a);
            n--;
        }
        return list.ToArray();
    }

    private static string BuildBeatPrompt(string worldName, string description, string actionsJson, DateTime startUtc,
        DateTime endUtc,
        List<BeatSummary>? beatHistory = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("System: You are the Narrative Beat Dramatizer for a fast-ticking narrative.");
        sb.AppendLine($"The narrative name is '{worldName}', and has the following details:");
        sb.AppendLine(description);
        sb.AppendLine("Rules:");
        sb.AppendLine("- Dramatize ONLY the events you are given. Do not invent new events.");
        sb.AppendLine("- Keep 'Title' ≤ 8 words. 'Dramatization' should be interesting and story-worthy 1-3 paragraphs that is consistent with previous narrative beats.");
        sb.AppendLine($"- 'Mood' ∈ {string.Join(", ", Enum.GetNames<Mood>())}.");
        sb.AppendLine("- 'Tension' is 0–100 (0=calm, 100=crisis).");
        sb.AppendLine();
        sb.AppendLine("Actions:");
        sb.AppendLine(actionsJson);
        sb.AppendLine();
        sb.AppendLine($"Time window: {startUtc:O} to {endUtc:O} (UTC)");
        sb.AppendLine();
        if (beatHistory is { Count: > 0 })
        {
            sb.AppendLine("**Preview beat dramatizations**");
            foreach (var d in beatHistory.TakeLast(10))
            {
                sb.AppendLine($"- {d.WindowEndUtc}: {d.Dramatization}");
            }
        }
        return sb.ToString();
    }

    private static string Hash(string s)
    {
        using var sha = System.Security.Cryptography.SHA1.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        return string.Concat(bytes.Take(10).Select(b => b.ToString("x2")));
    }

    public void Dispose() => _timer?.Dispose();
}