using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace NarrativeSimulator.Core.Models;

/// <summary>
/// Represents an action performed by a world agent, including the action type, target, and optional details.
/// </summary>
[Description("Represents a world agent action, including type, target, and optional details.")]
public class WorldAgentAction
{
    [JsonIgnore]
    public string? ActingAgent { get; set; }
    [Description("Optional brief description of action")]
    public string? BriefDescription { get; set; }
    [JsonIgnore]
    public string? AgentImagePath { get; set; }
    /// <summary>
    /// The kind of action to perform.
    /// </summary>
    [Description("Action type (e.g., SpeakTo, MoveTo, Decide).")]
    [JsonPropertyName("type")]
    public required ActionType Type { get; set; }

    /// <summary>
    /// The entity or location the action is directed at (e.g., character name or location).
    /// </summary>
    [Description("The target of the action, such as a character name or a location.")]
    [JsonPropertyName("target")]
    public string? Target { get; set; } // e.g., character name or location

    /// <summary>
    /// Optional extra information for the action, such as dialogue content or decision rationale.
    /// </summary>
    [Description("Details for the action (e.g., what to say or decide).")]
    [JsonPropertyName("details")]
    public required string Details { get; set; } // e.g., what to say or decide

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public (ActionType, string) ToTypeMarkdown()
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(Target))
        {
            sb.AppendLine($"- **Target:** {Target}");
        }
        sb.AppendLine($"- **Details:** {Details}");
        sb.AppendLine($"- **Timestamp:** {Timestamp:u}");
        return (Type, sb.ToString());
    }
}
public class UpdateAgentStateRequest
{
    [Description("Description of the update")]
    public required string Description { get; set; }
    [Description("The updated agent dynamic state")]
    public required DynamicState UpdatedDynamicState { get; set; }
}
public class UpdateAgentMemoryRequest
{
    [Description("Description of the update")]
    public required string Description { get; set; }
    [Description("The updated agent knowledge memory and relationships")]
    public required KnowledgeMemory UpdatedKnowledgeMemory { get; set; }
}
/// <summary>
/// Enumerates the possible world agent actions.
/// </summary>
[Description("World agent action types.")]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionType
{
    None,
    Error,
    [Description("Speak to a target character.")]
    SpeakTo,
    [Description("Move to a target location.")]
    MoveTo,
    [Description("Make a decision (no direct target interaction required).")]
    Decide,
    [Description("Discover something new in the environment.")]
    Discover,
    [Description("Trade or purchase items from a target.")]
    Purchase,
    [Description("Engage in a combat or hostile action.")]
    Attack,
    [Description("Defend against an attack or threat.")]
    Defend,
    [Description("Flee from a dangerous situation or threat.")]
    Flee,
    [Description("Undermine or sabotage a target.")]
    Undermine,
}

public static class EnumHelpers
{
    public static string ToDescriptionString<T>(this T val) where T : Enum
    {
        var fi = val.GetType().GetField(val.ToString());
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : val.ToString();
    }
    public static Dictionary<T, string> AllEnumDescriptions<T>() where T : Enum
    {
        // Get all values of the enum
        var enumType = typeof(T);
        if (!enumType.IsEnum)
            throw new ArgumentException("T must be an enumerated type");
        var values = Enum.GetValues(enumType).Cast<T>();
        // Convert each value to its description
        return values.ToDictionary(v => v, v => v.ToDescriptionString());

    }
}