using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using NarrativeSimulator.Core.Models;
using NarrativeSimulator.Core.Models.PsychProfile;
using NarrativeSimulator.Core.Services;

namespace NarrativeSimulator.Core;

public class AgentPersonalityAssessment
{
    public Dictionary<string, SentinoBig5> AgentPersonalities { get; set; } = [];

    public Dictionary<string, IndividualWriteup> AgentPersonalityWriteups { get; set; } = [];

    public TeamReport? TeamPersonalityReport { get; set; }
    public Dictionary<string, Dictionary<string,double>> AgentFacetScoreMap { get; set; } = [];
    public string? LlmGroupWriteUp { get; set; }
    public Dictionary<string,string> LlmIndividualWriteups { get; set; } = [];
    public string ToMarkdown()
    {
        // Generate a markdown representation of the personality assessments
        var sb = new StringBuilder();
        sb.AppendLine("# Agent Personality Assessments");
        sb.AppendLine();
        sb.AppendLine("## Agent Big 5 Scores"); sb.AppendLine();
        foreach (var (agentId, personality) in AgentPersonalities)
        {
            sb.AppendLine($"### {agentId}");
            sb.AppendLine(personality.ToMarkdown());
            sb.AppendLine();
        }
        sb.AppendLine("## Individual Writeups");
        foreach (var (agentId, writeup) in AgentPersonalityWriteups)
        {
            sb.AppendLine($"### {agentId}");
            sb.AppendLine(writeup.ToMarkdown());
            sb.AppendLine();
        }
        if (TeamPersonalityReport != null)
        {
            sb.AppendLine("## Team Personality Report");
            sb.AppendLine(TeamPersonalityReport.ToMarkdown());
            sb.AppendLine();
        }

        sb.AppendLine("## Facet Result Details");
        foreach (var (agentId, facetScores) in AgentFacetScoreMap)
        {
            sb.AppendLine($"### {agentId}");
            sb.AppendLine("| Facet | Score |");
            sb.AppendLine("|-------|-------|");
            foreach (var (facet, score) in facetScores)
            {
                sb.AppendLine($"| {facet} | {score:F2} |");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}

public class WorldState : INotifyPropertyChanged
{
    private WorldAgents? _worldAgents;
    private WorldAgent? _activeWorldAgent;
    private List<WorldAgentAction> _recentActions = [];
    private List<BeatSummary> _beats = [];
    private AgentPersonalityAssessment? _agentPersonalityAssessment;


    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? LastAgentUpdated;
    public string? Name
    {
        get => WorldAgents?.Name;
        set
        {
            if (WorldAgents == null) return;
            WorldAgents.Name = value;
            OnPropertyChanged();
        }
    }

    public string? Description => WorldAgents?.Description;
    public WorldAgents? WorldAgents
    {
        get
        {
            _worldAgents ??= WorldAgents.DefaultFromJson();
            return _worldAgents;
        }
        set => SetField(ref _worldAgents, value);
    }

    public WorldAgent? ActiveWorldAgent
    {
        get => _activeWorldAgent;
        set => SetField(ref _activeWorldAgent, value);
    }
    public string Weather { get; set; } = "Clear";
    public List<string> GlobalEvents { get; set; } = [];
    public List<string> Rumors { get; set; } = [];
    public string CurrentTime { get; set; } = DateTime.Now.ToString("t");
    public List<string> Locations => WorldAgents.Locations;

    public List<BeatSummary> Beats
    {
        get => _beats;
        set => SetField(ref _beats, value);
    }
    public void AddLastBeat(BeatSummary beat)
    {
        _beats.Insert(0, beat);
        if (_beats.Count > 40)
        {
            _beats.RemoveAt(_beats.Count - 1);
        }
        OnPropertyChanged(nameof(Beats));
    }
    public List<WorldAgentAction> RecentActions
    {
        get => _recentActions;
        set => SetField(ref _recentActions, value);
    }

    public AgentPersonalityAssessment? AgentPersonalityAssessment
    {
        get => _agentPersonalityAssessment;
        set => SetField(ref _agentPersonalityAssessment, value);
    }

    public void AddRecentAction(WorldAgentAction action)
    {
        _recentActions.Insert(0, action);
        if (_recentActions.Count > 50)
        {
            _recentActions.RemoveAt(_recentActions.Count - 1);
        }
        OnPropertyChanged(nameof(RecentActions));
    }

    public string WorldStateMarkdown()
    {
        // Markdown representation of the world state including RecentActions, GlobalEvents, and Rumors
        var sb = new StringBuilder();
        sb.AppendLine("## World State");
        sb.AppendLine($"- **Current Time:** {CurrentTime}");
        sb.AppendLine();
        sb.AppendLine("### Global Events");
        if (GlobalEvents.Count != 0)
        {
            foreach (var evt in GlobalEvents)
            {
                sb.AppendLine($"- {evt}");
            }
        }
        else
        {
            sb.AppendLine("- None");
        }
        sb.AppendLine();
        sb.AppendLine("### Rumors");
        if (Rumors.Count != 0)
        {
            foreach (var rumor in Rumors)
            {
                sb.AppendLine($"- {rumor}");
            }
        }
        else
        {
            sb.AppendLine("- None");
        }
        sb.AppendLine();
        sb.AppendLine("### Recent Actions");
        if (RecentActions.Count != 0)
        {
            foreach (var action in RecentActions.Where(w => w.Type != ActionType.Error).TakeLast(15))
            {
                var (type, details) = action.ToTypeMarkdown();
                sb.AppendLine($"- **{type}**");
                sb.AppendLine(details);
            }
        }
        else
        {
            sb.AppendLine("- None");
        }
        return sb.ToString();
    }
    public void UpdateAgent(WorldAgent agent)
    {
        var matchedAgent = WorldAgents.Agents.FirstOrDefault(a => a.AgentId == agent.AgentId);
        matchedAgent.DynamicState = agent.DynamicState;
        matchedAgent.KnowledgeMemory = agent.KnowledgeMemory;
        LastAgentUpdated?.Invoke(agent.AgentId);
        OnPropertyChanged(nameof(WorldAgents));
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}