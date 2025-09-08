namespace NarrativeSimulator.Core.Helpers;

public sealed class SentinoOptions
{
    public string ApiKey { get; set; } = string.Empty; // required
    public string BaseUrl { get; set; } = "https://sentino.p.rapidapi.com/"; // override if needed
}
public sealed class SymantoOptions
{
    public string ApiKey { get; set; } = string.Empty; // required
    public string BaseUrl { get; set; } = "https://big-five-personality-insights.p.rapidapi.com"; // override if needed
}