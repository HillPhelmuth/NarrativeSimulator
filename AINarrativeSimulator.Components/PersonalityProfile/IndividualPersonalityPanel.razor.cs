using ApexCharts;
using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Models.PsychProfile;
using NarrativeSimulator.Core.Services;

namespace AINarrativeSimulator.Components.PersonalityProfile;

public partial class IndividualPersonalityPanel
{
    [Parameter, EditorRequired] public AgentPersonalityAssessment Data { get; set; } = default!;
    [Parameter, EditorRequired] public string AgentId { get; set; } = default!;
    private string? _cachedId;
    private bool _showChart = true;
    private record BigFivePoint(string Trait, double Percentile);

    private ApexChart<BigFivePoint>? _chart;
    private static readonly string[] Traits = ["Openness", "Conscientiousness", "Extraversion", "Agreeableness", "Neuroticism"
    ];
    [Inject] 
    private WorldState WorldState { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        WorldState.PropertyChanged += HandleWorldStatePropertyChanged;
        return base.OnInitializedAsync();
    }

    private async void HandleWorldStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorldState.ActiveWorldAgent))
        {
            var newId = WorldState.ActiveWorldAgent?.AgentId;
            if (newId != null && newId != AgentId)
            {
                AgentId = newId;
                _showChart = false;
                StateHasChanged();
                Console.WriteLine($"\n===============================\n`_showChart` is {_showChart}\n===============================\n");
                Task.Delay(100);
                _showChart = true;
                await _chart.RenderAsync();
                _cachedId = AgentId;
                InvokeAsync(StateHasChanged);
            }
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        var firstPass = _cachedId == null;
        if (_cachedId != AgentId && !firstPass)
        {
            _showChart = false;
            StateHasChanged();
            await Task.Delay(50);
            _showChart = true;
            _cachedId = AgentId;
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    private static int Pct(SentinoTraitScore? t) =>
        t?.Quantile is double q ? (int)System.Math.Clamp(System.Math.Round(q * 100), 0, 100) : 50;

    private IEnumerable<BigFivePoint> RadarPoints
    {
        get
        {
            if (!Data.AgentPersonalities.TryGetValue(AgentId, out var b5))
                yield break;
            yield return new BigFivePoint("Openness", Pct(b5.Openness));
            yield return new BigFivePoint("Conscientiousness", Pct(b5.Conscientiousness));
            yield return new BigFivePoint("Extraversion", Pct(b5.Extraversion));
            yield return new BigFivePoint("Agreeableness", Pct(b5.Agreeableness));
            yield return new BigFivePoint("Neuroticism", Pct(b5.Neuroticism));
        }
    }

    private IndividualWriteup? Writeup => Data.AgentPersonalityWriteups.GetValueOrDefault(AgentId);

    private ApexChartOptions<BigFivePoint> RadarOpts => new()
    {
        Yaxis = [new() { Min = 0, Max = 100, TickAmount = 5 }],
        DataLabels = new DataLabels { Enabled = true },
        Stroke = new Stroke { Curve = Curve.Straight, Width = 2 },
        Annotations = new Annotations
        {
            Yaxis = [new() { Y = 50, BorderColor = "#94a3b8", Label = new Label { Text = "Median (50)" } }]
        }
    };
}