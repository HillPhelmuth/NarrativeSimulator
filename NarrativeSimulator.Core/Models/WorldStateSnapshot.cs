using System;
using System.Collections.Generic;

namespace NarrativeSimulator.Core.Models;

public class WorldStateSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public WorldAgents? WorldAgents { get; set; }
    public List<string>? Rumors { get; set; }
    public List<string>? GlobalEvents { get; set; }
    public List<WorldAgentAction>? RecentActions { get; set; }
    public List<BeatSummary>? Beats { get; set; }
}
