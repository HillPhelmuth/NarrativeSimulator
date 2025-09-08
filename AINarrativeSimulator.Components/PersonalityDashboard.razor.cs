using Markdig;
using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;

namespace AINarrativeSimulator.Components;
public partial class PersonalityDashboard
{
    [Inject]
    private WorldState WorldState { get; set; } = default!;

    private AgentPersonalityAssessment Assessment => WorldState.AgentPersonalityAssessment;
    [Inject]
    private INarrativeOrchestration NarrativeOrchestration { get; set; } = default!;
    //private string _agentId => WorldState.ActiveWorldAgent?.AgentId ?? "";
    private string? _agentId;
    protected override async Task OnInitializedAsync()
    {
        WorldState.AgentPersonalityAssessment ??= await NarrativeOrchestration.GenerateAllPersonalityScores();
        _agentId = WorldState.ActiveWorldAgent?.AgentId ?? AgentIds.FirstOrDefault() ?? "";
        WorldState.PropertyChanged += HandleWorldStatePropertyChanged;
        await base.OnInitializedAsync();
    }

    private void HandleWorldStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorldState.ActiveWorldAgent))
        {
            _agentId = WorldState.ActiveWorldAgent?.AgentId ?? AgentIds.FirstOrDefault() ?? "";
            
            InvokeAsync(StateHasChanged);
        }
    }

    private static string? MarkdownAsHtml(string? markdownString)
    {
        if (string.IsNullOrWhiteSpace(markdownString))
            return markdownString;
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(markdownString, pipeline);
        return result;
    }
    private async Task OnAfterAgentSelected()
    {
        WorldState.ActiveWorldAgent = WorldState.WorldAgents?.Agents.FirstOrDefault(x => x.AgentId == _agentId);
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private IEnumerable<string> AgentIds => WorldState.WorldAgents?.Agents.Select(x => x.AgentId) ?? [];
}
