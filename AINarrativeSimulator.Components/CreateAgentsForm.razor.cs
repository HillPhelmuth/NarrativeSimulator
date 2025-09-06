using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core.Models;

namespace AINarrativeSimulator.Components;
public partial class CreateAgentsForm
{
    private CreateAgentWorldForm _form = new();
    [Parameter]
    public EventCallback<CreateAgentWorldForm> OnCreateAgents { get; set; }

    private async Task SubmitAsync()
    {
        if (OnCreateAgents.HasDelegate)
        {
            // Create a shallow copy to avoid mutation after submit
            var copy = new CreateAgentWorldForm
            {
                Prompt = _form.Prompt,
                WorldType = _form.WorldType,
                FictionalWorldDescription = _form.FictionalWorldDescription,
                NumberOfAgents = _form.NumberOfAgents
            };
            await OnCreateAgents.InvokeAsync(copy);
        }
    }
}
