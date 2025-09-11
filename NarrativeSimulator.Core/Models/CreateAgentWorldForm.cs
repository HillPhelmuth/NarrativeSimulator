using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativeSimulator.Core.Models;

public class CreateAgentWorldForm
{
    [Required]
    public string Prompt { get; set; }
    public WorldType WorldType { get; set; } = WorldType.RealWorld;
    public string? FictionalWorldDescription { get; set; } // only displayed if WorldType is Fictional
    [Range(1, 12)]
    public int NumberOfAgents { get; set; } = 5;
}

public enum WorldType
{
    Fictional,
    RealWorld
}