using ApexCharts;
using Microsoft.AspNetCore.Components;
using NarrativeSimulator.Core;
using NarrativeSimulator.Core.Models;
using NarrativeSimulator.Core.Models.PsychProfile;

namespace AINarrativeSimulator.Components.PersonalityProfile;

public partial class TeamPersonalityPanel
{
    [Parameter, EditorRequired] public AgentPersonalityAssessment Data { get; set; } = default!;

    private static readonly string[] Traits =
        ["Openness", "Conscientiousness", "Extraversion", "Agreeableness", "Neuroticism"];

    public class HeatCell
    {
        public string Trait { get; set; } = "";
        public string Agent { get; set; } = "";
        public int Percent { get; set; } = 50;
        public string Band { get; set; } = "Medium"; // updated default
        public string ConfidenceText { get; set; } = "";
    }

    private static int Pct(SentinoTraitScore? t) =>
        t?.Quantile is { } q ? (int)Math.Clamp(Math.Round(q * 100), 0, 100) : 50;

    // Updated to 5 categories: Very Low (0-10), Low (11-30), Medium (31-70), High (71-90), Very High (91-100)
    private static string BandOf(int x) =>
        x <= 20 ? "Very Low" :
        x <= 40 ? "Low" :
        x <= 60 ? "Medium" :
        x <= 80 ? "High" :
        "Very High";
    private static string Conf(SentinoTraitScore? t) => t?.ConfidenceText ?? "";

    private IEnumerable<HeatCell> Row(string agentId, SentinoBig5 b)
    {
        yield return new HeatCell
        {
            Agent = agentId, Trait = "Openness", Percent = Pct(b.Openness), Band = BandOf(Pct(b.Openness)),
            ConfidenceText = Conf(b.Openness)
        };
        yield return new HeatCell
        {
            Agent = agentId, Trait = "Conscientiousness", Percent = Pct(b.Conscientiousness),
            Band = BandOf(Pct(b.Conscientiousness)), ConfidenceText = Conf(b.Conscientiousness)
        };
        yield return new HeatCell
        {
            Agent = agentId, Trait = "Extraversion", Percent = Pct(b.Extraversion), Band = BandOf(Pct(b.Extraversion)),
            ConfidenceText = Conf(b.Extraversion)
        };
        yield return new HeatCell
        {
            Agent = agentId, Trait = "Agreeableness", Percent = Pct(b.Agreeableness),
            Band = BandOf(Pct(b.Agreeableness)), ConfidenceText = Conf(b.Agreeableness)
        };
        yield return new HeatCell
        {
            Agent = agentId, Trait = "Neuroticism", Percent = Pct(b.Neuroticism), Band = BandOf(Pct(b.Neuroticism)),
            ConfidenceText = Conf(b.Neuroticism)
        };
    }

    private IEnumerable<string> AgentIds => Data.AgentPersonalities.Keys;

    private ApexChartOptions<HeatCell> HeatOpts => new()
    {
        Chart = new Chart { Toolbar = new Toolbar { Show = true } },
        Xaxis = new XAxis { Title = new AxisTitle { Text = "Traits" }, Categories = Traits.ToList() },
        DataLabels = new DataLabels { Enabled = true, Formatter = "function(val){ return Math.round(val); }" },
        PlotOptions = new PlotOptions
        {
            Heatmap = new PlotOptionsHeatmap
            {
                ShadeIntensity = 0.7,
                ColorScale = new PlotOptionsHeatmapColorScale()
                {
                    Ranges =
                    [
                        new PlotOptionsHeatmapColorScaleRange { From = 0, To = 20, Name = "Very Low", Color = "#991b1b" },
                        new PlotOptionsHeatmapColorScaleRange { From = 21, To = 40, Name = "Low", Color = "#ef4444" },
                        new PlotOptionsHeatmapColorScaleRange { From = 41, To = 60, Name = "Medium", Color = "#9ca3af" },
                        new PlotOptionsHeatmapColorScaleRange { From = 61, To = 80, Name = "High", Color = "#22c55e" },
                        new PlotOptionsHeatmapColorScaleRange { From = 81, To = 100, Name = "Very High", Color = "#15803d" }
                    ]
                }
            }
        },
        Tooltip = new Tooltip
        {
            Shared = false,
            Y = new TooltipY
            {
                
                //Formatter =
                //    "function(val, opts) { var d=opts.w.config.series[opts.seriesIndex].data[opts.dataPointIndex]; return d.Band + ' ('+Math.round(val)+'%)' + (d.ConfidenceText? ' — '+d.ConfidenceText : ''); }"
            }
        }
    };

    private IEnumerable<string> Highlights => Data.TeamPersonalityReport?.Highlights ?? [];
    private IEnumerable<string> Risks => Data.TeamPersonalityReport?.Risks ?? [];
}