using Microsoft.AspNetCore.Components;

namespace NarrativeSimulator.Components.Pages;
public partial class Home
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    private void NavigateToSimulator() => Navigation.NavigateTo("/simulator");
}
