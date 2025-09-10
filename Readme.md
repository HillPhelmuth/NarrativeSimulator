<div align="center">

# Group Personality Dynamics & Narrative Simulator

An experimental multi‑agent narrative world that uses open‑weight gpt‑oss reasoning models to simulate evolving group personalities, social dynamics, and emergent story beats in real time.

_Hackathon Submission: OpenAI Open Model Hackathon_

**Categories Entered:** Best Overall, Wildcard

</div>

---

## 1. Why This Exists
Story worlds with many characters usually collapse into scripts or hand‑crafted rules. This project instead lets personality vectors, inter‑agent affect, and contextual memory drive choices. Each “beat” (a simulation tick) invokes an open reasoning model to synthesize plausible next actions grounded in: (a) persistent personality, (b) evolving group mood, (c) recent interaction history, and (d) world constraints. The result: organic team dynamics, shifting alliances, and emergent plot threads you didn’t pre‑author.

## 2. What Makes It Novel (Judging Alignment)
| Judging Dimension | Differentiation |
|-------------------|-----------------|
| Application of gpt‑oss | Orchestrated multi‑agent loop: model drafts next speaker action + optional tool calls; deferred personality scoring (Sentino Big Five) informs a second self‑critique pass; model also performs periodic beat summarization and on‑demand world summaries; rumors & global events are injected as lightweight steering context without breaking autonomy. |
| Design | Dual entry path (curated preset casts vs generated world) with an accessible toggle; Create Agents form (world type, count, scenario prompt) + spinner overlay; snapshot load panel; World Controller (start/stop loop, rumor injection, summaries); live Event Feed & Beat Ticker; Agent Inspector with lazy personality fetch, traits heatmap & radar; consistent dark UI, semantic grouping, keyboard/ARIA labeling. |
| Potential Impact | Rapid prototyping of story worlds, writers’ room brainstorming, organizational team dynamics rehearsal (real vs fictional mode), educational demos of emergent behavior, game AI sandbox, synthetic user research cohorts with controllable narrative stimuli. |
| Novelty | Hybrid real‑time personality acquisition (scored only when an agent is inspected) fused with reasoning‑driven action generation; three narrative abstraction layers (atomic actions → beats → global summary); steerable but non‑authoritative rumor/event injection; provider targeting (e.g., Cerebras / Groq) to minimize latency for multi‑agent cycles. |
| Wildcard | Toggleable curated scenario packs + freeform generation in the same loop; personality‑aware self‑critique depth; soft‑control via rumor catalysts instead of direct commands; deferred psychometric scoring as a cost/latency optimization; pluggable high‑throughput open reasoning backends enabling near real‑time social simulation. |

## 3. Core Concept
Each agent has a personality profile (Big Five + custom facets). The Beat Engine assembles a structured prompt (see `Prompts.cs`) embedding:
1. Agent’s stable traits
2. Dynamic affect deltas (recent interactions)
3. Group personality centroid & variance
4. Last N world events (summary + salient tags)
5. Pending goals / unresolved tensions

The reasoning model (gpt‑oss) runs a multi‑phase chain:
1. Draft plausible actions (JSON schema)
2. Self‑evaluate for trait alignment & narrative coherence
3. Revise / prune
4. Emit structured `WorldAgentAction` set

Actions are ingested; world state updates; dashboards visualize distribution shifts; new summaries feed the next beat. This iterative reasoning loop showcases strengths of open models: introspection transparency, modifiable inner steps, and local controllability.

## 4. High‑Level Architecture
```
┌──────────────────────┐   Prompts     ┌────────────────────────┐
│ Static Personality   │──────────────▶│ Reasoning Model(gpt-oss│
│ Profiles (JSON)      │◀───────────── │   open weights)        │
└──────────┬───────────┘               └───────────┬────────────┘
           │                                       │ structured actions
           ▼                                       ▼
┌──────────────────────┐  world snapshots  ┌──────────────────────┐
│ Beat Engine          │──────────────────▶│ World State          │
│ (tick orchestration) │◀──────────────────│ (Agents, Events)     │
└──────────┬───────────┘   events/logs     └──────────┬───────────┘
           │ UI bind                                     │ persistence (in‑mem / future DB)
           ▼                                             ▼
┌────────────────────────────────────────────────────────────────┐
│ Blazor Components (Dashboards, Agent Inspector, Event Feed)    │
└────────────────────────────────────────────────────────────────┘
```

### Project Layout
| Project | Purpose |
|---------|---------|
| `NarrativeSimulator.Core` | Domain models, Beat Engine, personality interpretation, prompt construction, reasoning handlers, service registration. |
| `AINarrativeSimulator.Components` | Blazor UI components (personality dashboards, beat ticker, agent inspector, world controller). |
| `NarrativeSimulator` | Blazor host / web app shell, routing, configuration, static assets. |

## 5. Key Files & Concepts
| File | Role |
|------|------|
| `WorldState.cs` | Central mutable snapshot container of agents & events. |
| `BeatEngine.cs` | Orchestrates each simulation beat (gathers context → calls model → applies actions → raises events). |
| `OpenRouterReasoningHandler.cs` | Example integration hook to a reasoning endpoint (can swap for local inference). |
| `Prompts.cs` | Template fragments + schema guidelines; ensures structured, parseable output. |
| `BigFiveInterpreter.cs` | Maps numeric trait values to qualitative descriptors & explanation strings for UI. |
| `StaticData/*.json` | Pre‑generated personality profiles & scenario casts (e.g. `DystopiaAgents.json`). |
| `Services/*` | External API clients (Sentino / Symanto scoring enrichment, etc.). |
| `NarrativeOrchestration.cs` | Handles LLM orchestration using Semantic Kernel. |

## 6. Feature Highlights
1. Personality‑conditioned reasoning depth (adaptive multi‑pass prompting)
2. Group Big Five trait aggregation & variance visualization with gpt‑oss write‑up grounded in current personality research and impact on group dynamics
3. Event feed with beat summaries (rapid narrative comprehension)
4. Agent inspector (trait vector, last actions, motivations)
5. Scenario packs (swap entire casts quickly via JSON)
6. Pluggable host providers – currently Cerebras and Groq for lightning‑fast throughput

## 7. Applying gpt‑oss Open Reasoning Models
Advantages over proprietary or other open‑weight models:
* Direct visibility of – and influence over – reasoning tokens
* Variety of provider options allows very high throughput without quickly hitting rate limits (supports perpetual loops & multi‑threaded LLM calls)
* Injecting critique roles without the latency penalty of multiple external round trips
* Potential on‑device / local deployment for sensitive sandboxing (psychometrics / personality & group dynamics studies)

## 8. Running Locally
Prereqs: .NET 9 SDK (preview acceptable). Windows / macOS / Linux supported.

### Quick Start
```powershell
git clone https://github.com/HillPhelmuth/NarrativeSimulator.git
cd NarrativeSimulator
dotnet restore
dotnet build
dotnet run --project NarrativeSimulator/NarrativeSimulator.csproj
```
Then open http://localhost:5000 (or the HTTPS port shown). Use the World Controller panel to load a scenario and start the beat ticker.

### Configuring Model Access
The host expects either:
* Local inference endpoint (configure base URL + model name in `appsettings.Development.json`)
* Or an OpenAI / OpenRouter compatible endpoint (see `OpenRouterReasoningHandler.cs`)

Environment variables (suggested):
* `OpenRouter:ApiKey` (if remote)
* `Sentino:ApiKey` (currently via RapidAPI)

Add them via user secrets or OS environment.

## 9. Roadmap (Post‑Hackathon)
* Memory layer: episodic long‑term embeddings per agent
* Social graph edge weights & visual graph panel
* Conflict / cooperation outcome simulation (probabilistic resolution)
* Multi‑beat narrative arc summarizer (act / scene labeling)
* Fine‑tuned personality calibration (automatic trait drift based on actions)
* Local quantized model runner (ONNX / GGUF loader integration)

## 10. Why It Fits Best Overall & Wildcard
Best Overall: Demonstrates open reasoning models doing structured, introspective multi‑agent simulation—hard to replicate cleanly with proprietary stacks due to combination of high‑quality reasoning + lightning‑fast inference speeds (≈1,000–3,000 tokens/sec per model, provider dependent).

Wildcard: Personality‑adaptive reasoning depth + trait‑conditioned critique style is an uncommon fusion of lightweight psychometrics and open‑weight chain‑of‑thought control; it turns a language model into a configurable social simulation substrate.

---

## Quick Pitch (TL;DR)
Open reasoning models + adaptive personality vectors = emergent, inspectable, and controllable social narrative simulation. Not just text generation—an evolving world you can probe, measure, and steer.

Enjoy exploring the narrative simulator.
