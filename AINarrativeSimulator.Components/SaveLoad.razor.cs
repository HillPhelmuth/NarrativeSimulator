using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Models;

namespace AINarrativeSimulator.Components;
public partial class SaveLoad
{
    private const string SnapshotStorageKey = "worldstate-snapshots";
    private List<WorldStateSnapshot> _snapshots = [];
    private bool _showSnapshots;
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject]
    private WorldState WorldState { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await LoadSnapshotsAsync();
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadSnapshotsAsync()
    {
        try
        {
            var existing = await LocalStorage.GetItemAsync<List<WorldStateSnapshot>>(SnapshotStorageKey);
            if (existing != null)
            {
                _snapshots = existing;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch { /* ignore */ }
    }

    private async Task PersistSnapshotsAsync()
    {
        try
        {
            if (_snapshots.Count == 0) return;
            await LocalStorage.SetItemAsync(SnapshotStorageKey, _snapshots);
        }
        catch { /* ignore */ }
    }

    private async Task SaveSnapshot()
    {
        if (WorldState.WorldAgents == null) return;
        var snap = new WorldStateSnapshot
        {
            Name = WorldState.Name ?? $"World {_snapshots.Count + 1}",
            Description = WorldState.Description,
            WorldAgents = WorldState.WorldAgents,
            Rumors = WorldState.Rumors.ToList(),
            GlobalEvents = WorldState.GlobalEvents.ToList(),
            RecentActions = WorldState.RecentActions.ToList(),
            Beats = WorldState.Beats.ToList()
        };
        _snapshots.Add(snap);
        await PersistSnapshotsAsync();
        _showSnapshots = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadSnapshot(Guid id)
    {
        var snap = _snapshots.FirstOrDefault(s => s.Id == id);
        if (snap?.WorldAgents == null) return;
        WorldState.WorldAgents = snap.WorldAgents;
        WorldState.Rumors = snap.Rumors ?? [];
        WorldState.GlobalEvents = snap.GlobalEvents ?? [];
        WorldState.RecentActions = snap.RecentActions ?? [];
        WorldState.Beats = snap.Beats ?? [];
        _showSnapshots = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task DeleteSnapshot(Guid id)
    {
        var idx = _snapshots.FindIndex(s => s.Id == id);
        if (idx >= 0)
        {
            _snapshots.RemoveAt(idx);
            await PersistSnapshotsAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void ToggleSnapshots() => _showSnapshots = !_showSnapshots;
    public void CloseSnapshots()
    {
        _showSnapshots = false;
        StateHasChanged();
    }
}
