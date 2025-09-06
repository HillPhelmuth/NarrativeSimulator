using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Models;

namespace AINarrativeSimulator.Components;
public partial class Intro
{
    [Inject] 
    private WorldState WorldState { get; set; } = default!;
    [Inject]
    private INarrativeOrchestration NarrativeOrchestration { get; set; } = default!;
    [Parameter]
    public EventCallback OnAgentsCreated { get; set; }

    private async Task CreateAgents(CreateAgentWorldForm createAgentForm)
    {
        var worldDescription = createAgentForm.WorldType == WorldType.RealWorld ? "in the real world" 
            : $"in a fictional world described as: {createAgentForm.FictionalWorldDescription}";
        var prompt = $"""
                      Create agents {worldDescription} that are conform to the user instructions:

                      **User Instructions**

                      {createAgentForm.Prompt}

                      """;
        var agentWorld = await NarrativeOrchestration.GenerateAgents(prompt, createAgentForm.NumberOfAgents);
        WorldState.WorldAgents = agentWorld;
        StateHasChanged();
        await OnAgentsCreated.InvokeAsync();
    }
}
