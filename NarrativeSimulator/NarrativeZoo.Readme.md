# Narrative Zoo

An interactive, agent-based narrative simulator for experimenting with multi-agent behavior, emergent stories, adaptive psychology, and tool‑augmented reasoning. The UI is built with reusable Blazor components in `AINarrativeSimulator.Components`, and the simulation / runtime logic lives in `NarrativeSimulator.Core`.

---
## What It Does
- Renders a resizable Blazor dashboard composed of:
  - **Intro / Agent Creation**: Generate new agents with an LLM or load a preset cast.
  - **World Controller**: Start / pause simulation loop, inject rumors & world events, inspect basic world state.
  - **Event Feed**: Live log of agent actions & decisions (function output + narrative responses).
  - **Agent Inspector**: Drill into an agent’s dynamic state, goals, relationships, memories, and personality profile.
  - **Beat Ticker**: Periodic higher-level “dramatic beats” distilled from recent atomic actions (emergent summaries).
- Orchestrates agent turns via `NarrativeOrchestration` which:
  - Selects the next speaking / acting agent (LLM guided fairness heuristic)
  - Produces agent reasoning + text output
  - Invokes tool / function calls (state updates, memory updates, actions) via Semantic Kernel function filters
  - Updates shared `WorldState`
- Maintains rolling `RecentActions`, derived `BeatSummary` windows, and synthesized world summaries.
- Supports **LLM-backed reasoning** through OpenRouter models (`openai/gpt-oss-*`) with resilient HTTP + provider targeting.
- Supports **personality scoring** (Big Five) per agent using the Sentino API and renders a comparative bar chart.
- Allows **runtime injection** of player-authored rumors & global events that enter the world state and influence future reasoning.
- Persists snapshots of agents & world state periodically to browser `localStorage` for light session continuity.

---
## New / Notable Features (Since Earlier Version)
- Agent generation form (`CreateAgentsForm`) with support for: real‑world vs fictional world context + variable agent counts.
- Preset agent groups loaded from embedded JSON (`PreGeneratedOptions.json`, e.g. `RebelAgents.json`).
- Beat extraction & dramatization pipeline (`BeatEngine`) producing structured `BeatSummary` objects.
- Live Beat UI component (`BeatTicker`) with mood-tag styling.
- Agent personality quantification (Big Five) via `SentinoClient` + visualization (`BigFiveVisual`).
- World summarization modal (multi-cycle differential summaries) via `SummarizeCurrentWorldState()`.
- Tool / function invocation filtering (`AutoInvocationFilter`, `FunctionInvocationFilter`) for safe Semantic Kernel function auto-selection.
- OpenRouter resilient HTTP handler (`OpenRouterReasoningHandler`) adds provider routing + retry / timeout / circuit-breaker policies.
- Enhanced agent memory & dynamic state update prompts after each action.
- Rumor & event injection UI (World Controller) mutating `WorldState.Rumors` / `WorldState.GlobalEvents`.
- Resizable client-side grid layout (`wwwroot/resizableGrid.js`).

---
## Key Projects & Files
### UI (Blazor Components)
- `AINarrativeSimulator.Components/Intro.razor` – Entry panel; preset selection or agent generation.
- `AINarrativeSimulator.Components/CreateAgentsForm.razor` – LLM-based agent world creation form.
- `AINarrativeSimulator.Components/Main.razor` – Root dashboard layout + modals + persistence logic.
- `AINarrativeSimulator.Components/WorldController.razor` – Simulation controls + inject rumors/events + world listings.
- `AINarrativeSimulator.Components/EventFeed.razor` – Stream of raw agent actions & reasoning output.
- `AINarrativeSimulator.Components/AgentInspector.razor` – Tabbed agent detail panel (overview, relationships, memories, goals, personality).
- `AINarrativeSimulator.Components/BeatTicker/BeatTicker.razor` – Rolling dramatic beats view.
- `AINarrativeSimulator.Components/PersonalityProfile/BigFiveVisual.razor` – Big Five bar visualization.
- `AINarrativeSimulator.Components/wwwroot/resizableGrid.js` – Panel resizing behavior.

### Core Simulation & Services
- `NarrativeSimulator.Core/NarrativeOrchestration.cs` – Main loop, LLM prompting, function invocation, agent selection, state & memory updates.
- `NarrativeSimulator.Core/WorldState.cs` – Canonical mutable world state (agents, actions, beats, rumors, events, time, weather).
- `NarrativeSimulator.Core/Models/*` – Data models (agents, actions, beats, psych profile, etc.).
- `NarrativeSimulator.Core/Plugins/WorldAgentsPlugin.cs` – Semantic Kernel plugin exposing state/memory/action functions to the model.
- `NarrativeSimulator.Core/Services/BeatEngine.cs` – Time + action windowing → beat summarization (LLM structured output).
- `NarrativeSimulator.Core/Services/SentinoClient.cs` – External personality scoring (Big Five) integration.
- `NarrativeSimulator.Core/Services/OpenRouterReasoningHandler.cs` – Resilient OpenRouter handler (provider targeting, retries, timeouts, circuit breaker).
- `NarrativeSimulator.Core/StaticData/WorldAgents.json` – Sample station scenario cast.
- `NarrativeSimulator.Core/StaticData/RebelAgents.json` – Preset “Eclipse Covenant” infiltration scenario.
- `NarrativeSimulator.Core/StaticData/PreGeneratedOptions.json` – Preset group metadata manifest.
- `NarrativeSimulator.Core/Helpers/Prompts.cs` – Rich templated system prompt for agent world generation.

---
## Running the Simulator
1. Open the solution `GptOssHackathonPocs.sln` in **Visual Studio 2022** (or use `dotnet` CLI).
2. Set `GptOssHackathonPocs` (the Blazor host) as the startup project.
3. Run (F5) or `dotnet run` in `NarrativeSimulator/NarrativeSimulator` project directory.
4. Navigate to `/narrativezoo`.

### Optional Environment / Configuration
| Capability | How to Enable | Notes |
|------------|---------------|-------|
| OpenRouter LLM | Set `OpenRouter:ApiKey` (user secrets or env var) | Models used: `openai/gpt-oss-20b`, `openai/gpt-oss-120b` (reasoning variants) |
| Sentino Personality Scoring | Set `Sentino:ApiKey` + `Sentino:BaseUrl` | Enables Big Five chart per agent (first view triggers scoring) |

If API keys are absent the sim still runs with LLM calls failing gracefully (or you can stub logic) but personality charts won't populate.

---
## Using the UI
- **Intro Panel**: Choose a preset world or generate a new agent set; once agents exist open the main dashboard.
- **Start / Pause**: Toggles continuous narrative ticking (agent selection + reasoning + tool calls).
- **Inject Rumor / Event**: Adds narrative stimuli; appears in world state markdown & influences future prompts.
- **Event Feed**: Expand entries to view structured action markdown.
- **Agent Inspector** Tabs:
  - Overview (personality traits, core values, mood, location)
  - Relationships (trust-scored links)
  - Memories (recent memory strings)
  - Goals (short vs long term)
  - Personality (Big Five visualization – first load triggers scoring call)
- **Beat Ticker**: Higher-level narrative beats every few seconds (or on idle flush) summarizing clusters of actions.
- **World Summary Modal**: Generates a delta-aware summary vs previous snapshot.

---
## Extending
- **Add New Preset Groups**: Drop a `{WorldName}Agents.json` + entry in `PreGeneratedOptions.json` (embedded resource).
- **Add Agent Actions**: Extend `ActionType`, update `WorldAgentAction`, and adjust prompting / parsing where needed.
- **New Reasoning Models**: Modify `CreateKernel()` / model IDs in `NarrativeOrchestration` & `BeatEngine`.
- **Additional Personality / Psychology**: Add new scoring services similar to `SentinoClient` and expose via UI.
- **Custom Summaries**: Replace or extend beat + world summary prompts.
- **Tool Functions**: Add new Semantic Kernel plugin methods to `WorldAgentsPlugin` and register them for auto-selection.

---
## Architectural Highlights
- **Semantic Kernel** for prompt templating + function invocation (Auto function selection for state/memory/action).
- **Resilient LLM Calls**: Retry + timeout + circuit breaker policies for OpenRouter requests.
- **Temporal Aggregation**: `BeatEngine` performs sliding window batching (min actions or idle timeout) → summarization.
- **Diff-Aware World Summaries**: Maintains previous snapshot text to highlight changes.
- **Personality Scoring Pipeline**: Agent self-descriptive prompt → Sentino API → normalized trait visualization.
- **Local Persistence**: Periodic agent & world snapshots to `localStorage` to inspect/debug or survive refresh.

---
## Known Limitations
- No hard guardrails against runaway token growth (basic history trimming only).
- Reasoning + action latency tied to external LLM availability.
- Minimal mobile optimization (desktop grid focus).
- World physics / causality is narrative-layer only; no spatial simulation engine.
- Personality scoring invoked only once per agent per session (no longitudinal drift yet).

---
## Troubleshooting
| Issue | Check |
|-------|-------|
| No events appearing | Ensure you pressed Start; verify agents exist. |
| Beats not showing | Need enough qualifying actions (non-None/Error) before a beat flush. |
| Personality chart blank | Confirm Sentino API key & network access. |
| LLM errors in feed | Confirm `OpenRouter:ApiKey` / network, or switch to deterministic stubs. |
| Layout glitching | Resize panels or reload; `resizableGrid.js` may need a re-init after dynamic panel toggles. |

---
## Future Ideas
- Memory importance & decay scoring.
- Agent-to-agent dialogue threading & conversation intents.
- Multi-model arbitration (reasoning vs execution separation).
- Narrative export (beat anthology / timeline view / comic panels prototype).
- Adaptive mood & goal drift from sentiment + unmet goal pressure.

---
## License / Attribution
Internal hackathon prototype. Ensure compliance with OpenRouter & Sentino usage terms when sharing or deploying externally.


