using System.Text.Json;
using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Models;
using NarrativeSimulator.Core.Models.PsychProfile;
using ApexCharts;

namespace AINarrativeSimulator.Components.PersonalityProfile;
public partial class BigFiveVisual
{
    [Parameter]
    public WorldAgent? Agent { get; set; }

    private string? _cachedAgentId;

    [Inject]
    private INarrativeOrchestration NarrativeOrchestration { get; set; } = default!;
    [Inject]
    private WorldState WorldState { get; set; } = default!;

    private SentinoBig5? _big5;
    private bool _isReady;
    private string _waitMessage = "Analyzing personality...";

    private List<TraitDatum> _traitData = [];
    private ApexChartOptions<TraitDatum> _options = new();
    private bool _hasRendered;
    protected override async Task OnParametersSetAsync()
    {
        if (Agent.AgentId == _cachedAgentId) return;
        _cachedAgentId = Agent?.AgentId;
        if (!_hasRendered) return;
        _isReady = false;
        StateHasChanged();
        await Task.Delay(1);
        var agentPersonalities = await NarrativeOrchestration.GenerateAllPersonalityScores();
        _big5 = agentPersonalities.AgentPersonalities[Agent.AgentId];
        _traitData = _big5.GetTraitScores()
            .Select(kvp => new TraitDatum
            {
                Name = ToDisplay(kvp.Key),
                Quantile = kvp.Value.Quantile ?? 0,
                Score = kvp.Value.Score,
                Confidence = kvp.Value.Confidence
            })
            .OrderBy(d => d.Name)
            .ToList();
        ConfigureChart();
        _isReady = true;
        StateHasChanged();
        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Agent is not null)
        {
            //var response = await NarrativeOrchestration.GeneratePersonalityScores(Agent);
            var agentPersonalities = await NarrativeOrchestration.GenerateAllPersonalityScores();
            _big5 = agentPersonalities.AgentPersonalities[Agent.AgentId]/*response.Item2*/;
            Console.WriteLine($"Big5 for {Agent.AgentId}: {JsonSerializer.Serialize(_big5)}");
            if (_big5 is not null)
            {
                _traitData = _big5.GetTraitScores()
                    .Select(kvp => new TraitDatum
                    {
                        Name = ToDisplay(kvp.Key),
                        Quantile = kvp.Value.Quantile ?? 0,
                        Score = kvp.Value.Score,
                        Confidence = kvp.Value.Confidence
                    })
                    .OrderBy(d => d.Name)
                    .ToList();
            }
            ConfigureChart();
            _isReady = true;
            StateHasChanged();
            _hasRendered = true;
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void ConfigureChart()
    {
        _options = new ApexChartOptions<TraitDatum>
        {
            Chart = new Chart { Type = ChartType.Bar, Toolbar = new Toolbar { Show = false } },
            PlotOptions = new PlotOptions
            {
                Bar = new PlotOptionsBar
                {
                    Horizontal = false,
                    BorderRadius = 4,
                    DataLabels = new PlotOptionsBarDataLabels { Position = BarDataLabelPosition.Top }
                }
            },
            DataLabels = new DataLabels
            {
                Enabled = true,
                Formatter = @"function(val){ return (val*100).toFixed(0) + '%'; }"
            },
            Yaxis =
            [
                new YAxis
                {
                    Min = 0,
                    Max = 1,
                    Labels = new YAxisLabels { Formatter = @"function(val){ return (val*100)+'%'; }" },
                    Title = new AxisTitle { Text = "Quantile" }
                }
            ],
            Xaxis = new XAxis { Title = new AxisTitle { Text = "Trait" } },
            Tooltip = new Tooltip { Y = new TooltipY { Formatter = @"function(val){ return (val*100).toFixed(1)+'%'; }" } },
            Legend = new Legend { Show = true },
            Theme = new Theme { Mode = Mode.Dark, Palette = PaletteType.Palette5}
        };
    }

    private static string ToDisplay(string key) => key switch
    {
        "agreeableness" => "Agreeableness",
        "conscientiousness" => "Conscientiousness",
        "extraversion" => "Extraversion",
        "neuroticism" => "Neuroticism",
        "openness" => "Openness",
        _ => key
    };
}
public sealed class TraitDatum
{
    public string Name { get; set; } = string.Empty;
    public double Quantile { get; set; }
    public double? Score { get; set; }
    public double? Confidence { get; set; }
}