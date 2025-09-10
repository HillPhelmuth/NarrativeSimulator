using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Helpers;
using NarrativeSimulator.Core.Models;
using Blazored.LocalStorage;

namespace AINarrativeSimulator.Components;
public partial class Intro
{
    [Inject]
    private WorldState WorldState { get; set; } = default!;
    [Inject]
    private INarrativeOrchestration NarrativeOrchestration { get; set; } = default!;
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;
    [Parameter]
    public EventCallback OnAgentsCreated { get; set; }

    private bool _usePresetGroup;
    private bool _isGenerating; // busy indicator flag

    private List<PresetAgentGroup> _presetGroups =
        FileHelper.ExtractFromAssembly<List<PresetAgentGroup>>("PreGeneratedOptions.json");
    private class SelectPresetForm
    {
        public string SelectedPresetFileName { get; set; } = "";
    }
    private SelectPresetForm _selectPresetForm = new();

    // Snapshots
    private const string SnapshotStorageKey = "worldstate-snapshots";
    private List<WorldStateSnapshot> _snapshots = new();
    private bool _showSnapshots;

    protected override async Task OnInitializedAsync()
    {
        await LoadSnapshotsAsync();
    }

    private async Task LoadSnapshotsAsync()
    {
        try
        {
            var existing = await LocalStorage.GetItemAsync<List<WorldStateSnapshot>>(SnapshotStorageKey);
            if (existing != null) _snapshots = existing;
        }
        catch { }
    }

    private void ToggleSnapshots() => _showSnapshots = !_showSnapshots;

    private async Task LoadSnapshot(Guid id)
    {
        var snap = _snapshots.FirstOrDefault(s => s.Id == id);
        if (snap == null || snap.WorldAgents == null) return;
        WorldState.WorldAgents = snap.WorldAgents;
        WorldState.Rumors = snap.Rumors ?? [];
        WorldState.GlobalEvents = snap.GlobalEvents ?? [];
        WorldState.RecentActions = snap.RecentActions ?? [];
        WorldState.Beats = snap.Beats ?? [];
        _showSnapshots = false;
        await OnAgentsCreated.InvokeAsync();
        StateHasChanged();
    }

    private async Task CreateAgents(CreateAgentWorldForm createAgentForm)
    {
        _isGenerating = true;
        StateHasChanged();
        try
        {
            var worldDescription = createAgentForm.WorldType == WorldType.RealWorld ? "in the real world"
                : $"in a fictional world described as: {createAgentForm.FictionalWorldDescription}";
            var prompt = $$"""
                          Create agents {worldDescription} that are conform to the user instructions:

                          **User Instructions**

                          {createAgentForm.Prompt}

                          """;
            var agentWorld = await NarrativeOrchestration.GenerateAgents(prompt, createAgentForm.NumberOfAgents);
            WorldState.WorldAgents = agentWorld;
            StateHasChanged();
            await OnAgentsCreated.InvokeAsync();
        }
        finally
        {
            _isGenerating = false;
            StateHasChanged();
        }
    }

    private void SelectPreset(PresetAgentGroup group)
    {
        if (group is null || string.IsNullOrWhiteSpace(group.Filename)) return;
        try
        {
            var loaded = FileHelper.ExtractFromAssembly<WorldAgents>(group.Filename);
            if (loaded != null)
            {
                WorldState.WorldAgents = loaded;
            }
        }
        catch
        {
            // TODO: optionally log
        }
        StateHasChanged();
        OnAgentsCreated.InvokeAsync();
    }
}

public class PresetAgentGroup
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int AgentCount { get; set; }
    public string Filename { get; set; }
}
