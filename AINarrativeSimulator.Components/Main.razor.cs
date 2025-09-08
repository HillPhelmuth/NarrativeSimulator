using Blazored.LocalStorage;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Client;
using Microsoft.JSInterop;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Models;
using NarrativeSimulator.Core.Services;

namespace AINarrativeSimulator.Components;

public partial class Main
{
    private List<WorldAgentAction> _actions = [];
    private List<WorldAgent> _agents = [];
    private List<BeatSummary> _beats = [];
    [Inject]
    private WorldState WorldState { get; set; } = default!;
    [Inject]
    private INarrativeOrchestration NarrativeOrchestration { get; set; } = default!;
    [Inject]
    private IBeatEngine BeatEngine { get; set; } = default!;
    private string? selectedAgentId;
    private bool isRunning;
    private string _rumor = "";
    private CancellationTokenSource _cts = new();

    private ElementReference _grid;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private ResizableGridJsInterop ResizableGridInterop => new(JS);
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private bool _minCol1;
    private bool _minCol2;
    private bool _minCol3;

    private string Col1Size => GetColSize(_minCol1, VisibleColumnsCount);
    private string Col2Size => GetColSize(_minCol2, VisibleColumnsCount, 0.2);
    private string Col3Size => GetColSize(_minCol3, VisibleColumnsCount, -0.2);
    private int VisibleColumnsCount => 3 - new[] { _minCol1, _minCol2, _minCol3 }.Count(m => m);

    private static string GetColSize(bool minimized, int visibleCount, double modifier = 0.0)
    {
        if (minimized) return "32px"; // just enough for the vertical label
        var val = 1.0 + modifier;
        return visibleCount switch
        {
            3 => $"{val}fr",
            2 => $"{val}fr", // both remaining share
            1 => $"{val}fr", // single expands
            _ => $"{val}fr"
        };
    }

    private bool _showSummaryModal = false;
    private bool _isSummarizing = false;
    private string _summary = "";

    private bool _showComics;
    private bool _showPersonalityDash;
    private static readonly TimeSpan AgentsPersistInterval = TimeSpan.FromMinutes(15);
    private DateTime _lastAgentsPersistedUtc = DateTime.MinValue;
    private DateTime _lastWorldStatePersistedUtc = DateTime.MinValue;
    private int _seenHash;

    private async Task OpenSummaryModal()
    {
        _showSummaryModal = true;
        _isSummarizing = true;
        _summary = string.Empty;
        await InvokeAsync(StateHasChanged);
        try { _summary = await NarrativeOrchestration.SummarizeCurrentWorldState(); }
        catch (Exception ex) { _summary = $"Error generating summary: {ex.Message}"; }
        finally { _isSummarizing = false; await InvokeAsync(StateHasChanged); }
    }

    private Task CloseSummaryModal()
    {
        _showSummaryModal = false;
        return InvokeAsync(StateHasChanged);
    }

    private async Task ToggleCol(int col)
    {
        switch (col)
        {
            case 1: _minCol1 = !_minCol1; break;
            case 2: _minCol2 = !_minCol2; break;
            case 3: _minCol3 = !_minCol3; break;
        }
        await InvokeAsync(StateHasChanged);
        await Task.Delay(1);
        if(!_minCol1 && !_minCol2 && !_minCol3)
        {
            await ResizableGridInterop.ReinitGrid(_grid, false);
        }
    }

    private void HandleBeat(BeatSummary beat)
    {
        WorldState.AddLastBeat(beat);
        InvokeAsync(StateHasChanged);
    }

    private async Task GenerateSummary()
    {
        _isSummarizing = true;
        _summary = string.Empty;
        await InvokeAsync(StateHasChanged);
        try { _summary = await NarrativeOrchestration.SummarizeCurrentWorldState(); }
        catch (Exception ex) { _summary = $"Error generating summary: {ex.Message}"; }
        finally { _isSummarizing = false; await InvokeAsync(StateHasChanged); }
    }

    protected override Task OnInitializedAsync()
    {
        if (WorldState.WorldAgents is null) Navigation.NavigateTo("/");
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            WorldState.PropertyChanged += HandleWorldStatePropertyChanged;
            NarrativeOrchestration.WriteAgentChatMessage += HandleAgentChatMessageWritten;
            BeatEngine.OnBeat += HandleBeat;
            await BeatEngine.StartAsync();
            await ResizableGridInterop.InitResizableGrid(_grid);
        }
    }

    private void HandleAgentChatMessageWritten(string chatMessage, string agent)
    {
        if (chatMessage.Contains("Oh, shit!"))
        {
            WorldState.AddRecentAction(new WorldAgentAction()
            {
                Type = ActionType.Error, ActingAgent = agent,
                BriefDescription = $"Error occurred before {agent} Finished their action !", Details = chatMessage,
                Timestamp = DateTime.Now
            });
            return;
        }
        WorldState.AddRecentAction(new WorldAgentAction() { Type = ActionType.None, ActingAgent = agent, BriefDescription = $"{agent} Finished their action and has something to say!", Details = chatMessage, Timestamp = DateTime.Now });
    }

    private async void HandleWorldStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            switch (e.PropertyName)
            {
                case nameof(WorldState.WorldAgents):
                    _agents = WorldState.WorldAgents.Agents;
                    var nowUtc = DateTime.UtcNow;
                    if (nowUtc - _lastAgentsPersistedUtc >= AgentsPersistInterval)
                    {
                        await LocalStorage.SetItemAsync($"agents-{DateTime.Now:hh:mm:ss}", WorldState.WorldAgents);
                        _lastAgentsPersistedUtc = nowUtc;
                    }
                    await InvokeAsync(StateHasChanged);
                    break;
                case nameof(WorldState.RecentActions):
                    var hash = Hash(WorldState.RecentActions);
                    if (hash == _seenHash) return;
                    _actions = WorldState.RecentActions;
                    var lastAction = _actions.FirstOrDefault();
                    if (lastAction is null) return;
                    BeatEngine.Add(lastAction);

                    var nowUtc2 = DateTime.UtcNow;
                    if (nowUtc2 - _lastWorldStatePersistedUtc >= AgentsPersistInterval)
                    {
                        await LocalStorage.SetItemAsync($"worldstate-{DateTime.Now:hh:mm:ss}", WorldState);
                        _lastWorldStatePersistedUtc = nowUtc2;
                    }
                    await InvokeAsync(StateHasChanged);
                    break;
                case nameof(WorldState.Name):
                case nameof(WorldState.Beats):
                    await InvokeAsync(StateHasChanged);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling WorldState property change: {ex.Message}");
        }
    }

    private static int Hash(IEnumerable<WorldAgentAction> actions)
    {
        unchecked
        {
            int h = 17;
            foreach (var a in actions)
            {
                h = h * 31 + a.Timestamp.GetHashCode();
                h = h * 31 + (a.Details?.GetHashCode() ?? 0);
            }
            return h;
        }
    }

    private async Task HandleStart()
    {
        isRunning = true;
        var token = _cts.Token;
        await NarrativeOrchestration.RunNarrativeAsync(_rumor, token);
        StateHasChanged();
    }

    private void HandleStop()
    {
        isRunning = false;
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        StateHasChanged();
    }

    private void HandleReset()
    {
        HandleStop();
        _actions.Clear();
        selectedAgentId = null;
        StateHasChanged();
    }

    private void HandleInjectRumor(string rumor)
    {
        _rumor += "\nRumor: " + rumor;
        WorldState.Rumors.Add(rumor);
        _actions.Add(new WorldAgentAction()
        {
            BriefDescription = "A rumor has been injected into the world",
            Type = ActionType.None,
            Target = "public",
            Details = rumor,
            Timestamp = DateTime.Now
        });
        StateHasChanged();
    }

    private void HandleInjectEvent(string evt)
    {
        _rumor += "\nEvent: " + evt;
        WorldState.GlobalEvents.Add(evt);
        _actions.Add(new WorldAgentAction
        {
            BriefDescription = "A world event has been injected into the world",
            Type = ActionType.Discover,
            Target = "public",
            Details = evt,
            Timestamp = DateTime.Now
        });
        StateHasChanged();
    }

    private static string MarkdownAsHtml(string markdownString)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        return Markdown.ToHtml(markdownString, pipeline);
    }

    private Task OnSelectedAgentChanged(string id)
    {
        selectedAgentId = id;
        return Task.CompletedTask;
    }
}