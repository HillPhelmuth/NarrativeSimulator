using System.Text;
using NarrativeSimulator.Core.Models.PsychProfile;
using NarrativeSimulator.Core.Services;

namespace NarrativeSimulator.Core.Models;

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
        //sb.AppendLine();
        //sb.AppendLine("## Agent Big 5 Scores"); sb.AppendLine();
        //foreach (var (agentId, personality) in AgentPersonalities)
        //{
        //    sb.AppendLine($"### {agentId}");
        //    sb.AppendLine(personality.ToMarkdown());
        //    sb.AppendLine();
        //}
        sb.AppendLine("## Agent Big 5 Scores & Individual Writeups");
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

        sb.AppendLine("## NEO-PI-R Facet Result Details");
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