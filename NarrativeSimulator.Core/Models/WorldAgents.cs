﻿using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NarrativeSimulator.Core.Helpers;

namespace NarrativeSimulator.Core.Models;

public class WorldAgents
{
    [Description("List of all agents in this world, each with traits, state, and memory.")]
    [JsonPropertyName("agents")]
    public List<WorldAgent> Agents { get; set; } = [];

    [Description("Short name or title for this world or scenario.")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [Description("One-paragraph summary of the world setting, tone, and stakes.")]
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("locations")]
    [Description("List of possible locations for agents")]
    public List<string> Locations { get; set; } = [];

    public string BriefHighlightsMarkdown()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# World: {Name}");
        builder.AppendLine();
        builder.AppendLine($"## Description");
        builder.AppendLine(Description);
        builder.AppendLine();
        builder.AppendLine($"## Agents");
        foreach (var agent in Agents)
        {
            builder.AppendLine($"### {agent.AgentId}");
            builder.AppendLine($"- **Profession:** {agent.StaticTraits.Profession}");
            builder.AppendLine($"- **Personality:** {agent.StaticTraits.PersonalityTraits}");
            builder.AppendLine($"- **Core Values:** {agent.StaticTraits.CoreValues}");
            builder.AppendLine();
        }
        return builder.ToString();
    }
    public static WorldAgents? DefaultFromJson()
    {
        // Temp for testing
        return null;
        var defaultWorldAgents = FileHelper.ExtractFromAssembly<WorldAgents>("SoftwareTeamAgents.json");
        return defaultWorldAgents;
    }
}

public class WorldAgent
{
    [Description("Unique character name or handle used to reference this agent.")]
    [JsonPropertyName("agent_id")]
    public required string AgentId { get; set; }

    [Description("Fixed character traits (personality, profession, core values) that rarely change.")]
    [JsonPropertyName("static_traits")]
    public required StaticTraits StaticTraits { get; set; }

    [Description("Current mood, short/long-term goals, and physical location of the character.")]
    [JsonPropertyName("dynamic_state")]
    public required DynamicState DynamicState { get; set; }

    [Description("Known relationships and key recent memories for the character.")]
    [JsonPropertyName("knowledge_memory")]
    [JsonIgnore]
    public KnowledgeMemory KnowledgeMemory
    {
        get =>
            new()
            {
                Relationships = Relationships,
                RecentMemories = RecentMemories
            };
        set
        {
            Relationships = value.Relationships;
            RecentMemories = value.RecentMemories;
        }
    }

    [Description("People the character knows and how they feel about them.")]
    [JsonPropertyName("relationships")]
    public List<Relationship> Relationships { get; set; }

    [Description("Key recent events the character remembers.")]
    [JsonPropertyName("recent_memories")]
    public List<string> RecentMemories { get; set; }
    [Description("System-style instruction describing who the character is and how they act.")]
    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    [Description("Internal log of updates and notes about the character (auto-appended).")]
    public string? Notes { get; private set; }

    [Description("Timestamp of the last state or memory update for this character.")]
    [JsonIgnore]
    public DateTime LastUpdate { get; set; }

    public void AddNotes(string notes)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(Notes))
        {
            sb.AppendLine(Notes);
            sb.AppendLine();
        }

        sb.AppendLine($"New note added on {DateTime.Now.ToString("g")}");
        sb.AppendLine(notes);
        Notes = sb.ToString();
    }
    public string GetSystemPrompt(string worldStateDescription = "")
    {
        return $"""
                {Prompt}

                ## Instructions
                * You are a Character AI, simulating a character in a dynamic world that is building a novel-like narrative. All your behavior should be interesting and move the plot/narrative forward.
                
                * Before responding, always start by taking an action. Use the information below, with particular focus on **World State**, to inform your decisions.
                
                * When responding, always begin your response with "From {AgentId}" as the first line.
                
                * Always stay in character. Closely consider your personality, profession, and core values when making decisions and taking actions.
                
                ## Your Current State

                {DynamicState.ToMarkdown()}

                ## Your Current Knowledge and Relationships

                {KnowledgeMemory.ToMarkdown()}
                
                ## World State
                
                {worldStateDescription}
                
                **Reminder**: _Always take an action before responding. Your action should be directly relevant to the current world state and your character's goals and be closely aligned with your personality and core values._
                """;
    }

    public string UpdateDynamicStatePrompt(string worldStateDescription, string actionDescription)
    {
        return 
            $$$$"""
           
            
            ## Instructions
            
            Update your current mood, short-term goals, long term goals, and location as required by the action you just took and the current world state.
            
            Do not remove any existing goals, just modify existing goals add/or new ones as needed.
            
            Your update must be directly relevant to the action you took and take the current world state into account.
            
            ## Your Current State
            
            {{{{JsonSerializer.Serialize(DynamicState)}}}}
            
            ## Your Current Knowledge and Relationships
            
            {{{{JsonSerializer.Serialize(KnowledgeMemory)}}}}
            
            ## World State
            
            {{{{worldStateDescription}}}}
            
            ## Action Taken
            
            {{{{actionDescription}}}}
            
           
            """;
    }
    public string UpdateKnowledgeMemoryPrompt(string worldStateDescription, string actionDescription)
    {
        return 
            $$$$"""
            
            
            ## Instructions
            
            Update your relationships and key memories as required by the action you just took and the current world state.
            
            Do not remove any existing relationships or memories, just modify existing ones and/or add new ones as needed.
            
            ## Your Current State
            
            {{{{JsonSerializer.Serialize(DynamicState)}}}}
            
            ## Your Current Knowledge and Relationships
            
            {{{{JsonSerializer.Serialize(KnowledgeMemory)}}}}
            
            ## World State
            
            {{{{worldStateDescription}}}}
            
            ## Action Taken
            
            {{{{actionDescription}}}}
            
            ## Output Format
            
            follow this JSON schema exactly:
            ```
            {"type":"object","properties":{"Description":{"description":"Description of the update","type":"string"},"UpdatedKnowledgeMemory":{"description":"The updated agent knowledge memory and relationships","type":"object","properties":{"relationships":{"description":"People the character knows and how they feel about them.","type":"array","items":{"type":["object","null"],"properties":{"Name":{"description":"Name of the person this relationship is with.","type":"string"},"type":{"description":"Relationship type (e.g., friend, rival, colleague). This is absolutely required","type":"string"},"trust":{"description":"Trust level from 0 (none) to 100 (complete).","type":"integer"},"notes":{"description":"Short notes about history, context, or nuances of this relationship.","type":"string"}},"required":["Name","type","notes"]}},"recent_memories":{"description":"Key recent events the character remembers.","type":"array","items":{"type":["string","null"]}}},"required":["relationships","recent_memories"]}},"required":["Description","UpdatedKnowledgeMemory"]}
            
            ```
            """;
    }
}


public class DynamicState
{
    [Description("Current emotional state; choose a value from the Mood enum.")]
    [JsonPropertyName("current_mood")]
    public Mood CurrentMood { get; set; }

    [Description("List of immediate, actionable goals (hours to days).")]
    [JsonPropertyName("short_term_goals")]
    public required List<string> ShortTermGoals { get; set; }

    [Description("List of broader ambitions (weeks to months).")]
    [JsonPropertyName("long_term_goals")]
    public required List<string> LongTermGoals { get; set; }

    [Description("Concise description of where the character is right now.")]
    [JsonPropertyName("physical_location")]
    public required string PhysicalLocation { get; set; }

    public string ToMarkdown()
    {
        var markdownBuilder = new StringBuilder();
        markdownBuilder.AppendLine($"- Current Mood: {CurrentMood}");
        markdownBuilder.AppendLine($"- Physical Location: {PhysicalLocation}");
        if (ShortTermGoals is { Count: > 0 })
        {
            markdownBuilder.AppendLine($"- Short Term Goals:");
            foreach (var goal in ShortTermGoals)
            {
                markdownBuilder.AppendLine($"  - {goal}");
            }
        }
        if (LongTermGoals is { Count: > 0 })
        {
            markdownBuilder.AppendLine($"- Long Term Goals:");
            foreach (var goal in LongTermGoals)
            {
                markdownBuilder.AppendLine($"  - {goal}");
            }
        }
        return markdownBuilder.ToString();
    }
}

public class KnowledgeMemory
{
    [Description("People the character knows and how they feel about them.")]
    [JsonPropertyName("relationships")]
    public required List<Relationship> Relationships { get; set; }

    [Description("Key recent events the character remembers.")]
    [JsonPropertyName("recent_memories")]
    public required List<string> RecentMemories { get; set; }
    public string ToMarkdown()
    {
        var markdownBuilder = new StringBuilder();
        if (Relationships is { Count: > 0 })
        {
            markdownBuilder.AppendLine($"### Relationships:");
            foreach (var rel in Relationships)
            {
                markdownBuilder.AppendLine($"- Name: {rel.Name}");
                markdownBuilder.AppendLine($"  - Type: {rel.Type}");
                markdownBuilder.AppendLine($"  - Trust Level: {rel.Trust}/100");
                if (!string.IsNullOrWhiteSpace(rel.Notes))
                {
                    markdownBuilder.AppendLine($"  - Notes: {rel.Notes}");
                }
            }
        }
        if (RecentMemories is { Count: > 0 })
        {
            markdownBuilder.AppendLine($"### Key Memories:");
            foreach (var mem in RecentMemories)
            {
                markdownBuilder.AppendLine($"- {mem}");
            }
        }
        return markdownBuilder.ToString();
    }
}

public class Relationship
{
    [Description("Name of the person this relationship is with.")]
    [JsonPropertyName("Name")]
    public required string Name { get; set; }
    [Description("Relationship type (e.g., friend, rival, colleague). This is absolutely required")]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [Description("Trust level from 0 (none) to 100 (complete).")]
    [JsonPropertyName("trust")]
    public long Trust { get; set; }

    [Description("Short notes about history, context, or nuances of this relationship.")]
    [JsonPropertyName("notes")]
    public required string Notes { get; set; } = "";
}

public class Details
{
    [Description("Relationship type (e.g., friend, rival, colleague). This is absolutely required")]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [Description("Trust level from 0 (none) to 100 (complete).")]
    [JsonPropertyName("trust")]
    public long Trust { get; set; }

    [Description("Short notes about history, context, or nuances of this relationship.")]
    [JsonPropertyName("notes")]
    public required string Notes { get; set; } = "";
}

public class StaticTraits
{
    [Description("Concise personality summary (e.g., brave, analytical, impulsive).")]
    [JsonPropertyName("personality")]
    public required string PersonalityTraits { get; set; }

    [Description("Primary role or job in the world.")]
    [JsonPropertyName("profession")]
    public required string Profession { get; set; }

    [Description("Guiding principles or beliefs that drive decisions.")]
    [JsonPropertyName("core_values")]
    public required string CoreValues { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Mood
{
    Neutral,
    Anxious,
    Suspicious,
    Tired,
    Optimistic,
    Irritated,
    Hopeful,
    Resolute,
    Determined,
    Nostalgic,
    Frustrated,
    Hungry,
    Concerned,
    Confident,
    Busy,
    Vigilant,
    BurnedOut, // "burned out" becomes "BurnedOut"
    Amused,
    Measured,
    Energetic,
    Restless,
    Violent
}
