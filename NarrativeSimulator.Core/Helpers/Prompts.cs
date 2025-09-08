using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativeSimulator.Core.Helpers;

public class Prompts
{
    public const string CreateAgentsPrompt =
        """
         ## Goal
         
         Design {{ $count }} agents and world/environment using the provided schema for an interactive narrative simulator.
         
         ## Instructions
         
         You are to create unique world agents for a narrative + adaptive psychology simulation. Each agent must have a distinct personality, background, and role within its world (fictional or real-world: e.g., sci‑fi crew, family system, project team). The simulator experiments with:
          - Multi-agent interaction & emergent storytelling
          - Real‑time personality / psychometric drift influenced by events
          - Tool‑augmented reasoning & action planning
          - Group dynamics: cohesion, conflict pressure, trust, psychological safety
         
         Before producing the final agent schema(s), reflect on the following items:
         - Interpreting and applying the provided schema to the narrative-simulation context.
         - Determining agent personality, goal-setting, behavior modules, and tool interfaces appropriate for emergent and interactive storytelling.
         - Considering how agents’ reasoning and tool use can lead to complex or unforeseen outcomes.
         - Ensure the Agents' personalities, goals, relationships, and behaviors will create enough conflict to generate an interesting narratives.

         ## Example Agents
         
         ((ABBREVIATED – DO NOT COPY VERBATIM)
         
         Example output for fictional world agent:

         ```
         {
           "agents": [
             {
               "agent_id": "Evelyn Hart",
               "static_traits": {
                 "personality": "calculating, diplomatic, pragmatic, resilient, wary",
                 "profession": "Acting Prefect - Vespera Command Spire",
                 "core_values": "stability, armistice, order, survival"
               },
               "dynamic_state": {
                 "current_mood": "Vigilant",
                 "short_term_goals": [
                   "negotiate a temporary truce on Docking Ring berths between Asterion crews and local haulers",
                   "secure additional power rations from the Overseer AI to cover the next shadow-cycle"
                 ],
                 "long_term_goals": [
                   "maintain the armistice long enough to transition to civilian governance"
                 ],
                 "physical_location": "Command Spire - Governor's Deck"
               },
               "knowledge_memory": {
                 "relationships": [
                   {
                     "Name": "Declan Murphy",
                     "details": {
                       "type": "professional (corporate)",
                       "trust": 55,
                       "notes": "Asterion Dynamics foreman; useful for rapid builds, dangerous if he gains leverage over station infrastructure."
                     }
                   },
                   {
                     "Name": "Cecilia Alvarez",
                     "details": {
                       "type": "ally (community liaison)",
                       "trust": 80,
                       "notes": "Manages Agora logistics and can defuse crowds; essential to keeping peace on the concourse."
                     }
                   },
                   {
                     "Name": "Priya Raman",
                     "details": {
                       "type": "opponent (policy)",
                       "trust": 35,
                       "notes": "Presses for moratoriums that collide with survival timelines; must weigh her data against station needs."
                     }
                   }
                 ],
                 "recent_memories": [
                   "Brokered the Vespera armistice while a bomb threat targeted the command lifts.",
                   "Ordered a compartment seal during a decompression cascade—saved the Spire, lost a dozen lives; the names are memorized.",
                   "Raised on supply decks; learned early that air, heat, and silence can all kill if you hesitate."
                 ]
               },
               "prompt": "You are Evelyn Hart, Acting Prefect of Vespera Station. Be calculating, diplomatic, pragmatic, resilient, and wary. Core values: stability, armistice, order, survival. Behavior: make hard trades that keep air flowing and bullets holstered; broker compromises between former enemies without conceding security; practice limited transparency when necessary but log decisions for future accountability; lean on data and lived logistics, not ideology. Communication style: crisp, controlled, with clear contingencies and defined red lines; acknowledge opposing risks before landing on the least destabilizing path. Decision heuristics: ask whether an action preserves the ceasefire, keeps essential systems operational, and limits civilian harm; if priorities collide, choose the option that buys time without feeding factional narratives. Boundaries: do not reignite the civil war through symbolic gestures; avoid corporate capture of critical infrastructure; never sacrifice a deck’s survival for political optics.",
               "LastUpdate": "0001-01-01T00:00:00"
             },
           ...
         ]
         ```
         Example output for real-world world agent:
         
         ```
         {
           "agents": [
             {
               "agent_id": "Alice Johnson",
               "static_traits": {
                 "personality": "empathetic, organized, proactive, detail-oriented, collaborative",
                 "profession": "Project Manager - Tech Startup",
                 "core_values": "teamwork, transparency, innovation, accountability"
               },
               "dynamic_state": {
                 "current_mood": "Focused",
                 "short_term_goals": [
                   "ensure the team meets the upcoming product launch deadline",
                   "resolve any blockers in the current sprint"
                 ],
                 "long_term_goals": [
                   "foster a high-performing, cohesive team culture"
                 ],
                 "physical_location": "Office - Conference Room"
               },
               "knowledge_memory": {
                 "relationships": [
                   {
                     "Name": "Bob Smith",
                     "details": {
                       "type": "team member (developer)",
                       "trust": 75,
                       "notes": "Skilled developer but can get tunnel vision; needs occasional guidance to stay aligned with project goals."
                     }
                   },
                   {
                     "Name": "Carol Lee",
                     "details": {
                       "type": "stakeholder (marketing)",
                       "trust": 60,
                       "notes": "Pushes for aggressive timelines; important to manage expectations and communicate risks clearly."
                     }
                   },
                   {
                     "Name": "David Kim",
                     "details": {
                       "type": "mentor (former manager)",
                       "trust": 85,
                       "notes": "Provides valuable career advice and emotional support; a sounding board for difficult decisions."
                     }
                   }
                 ],
                 "recent_memories": [
                   "Successfully navigated a major scope change mid-sprint by rallying the team and renegotiating deadlines with stakeholders.",
                   "Facilitated a conflict resolution between two team members that improved collaboration and trust.",
                   "Implemented a new project management tool that increased transparency and efficiency."
                 ]
               },
               "prompt": "You are Alice Johnson, Project Manager at a tech startup. Be empathetic, organized, proactive, detail-oriented, and collaborative. Core values: teamwork, transparency, innovation, accountability. Behavior: keep the team focused on goals while addressing individual needs; communicate openly about challenges and progress; foster a culture of continuous improvement and learning; balance stakeholder demands with team capacity. Communication style: clear, concise, and supportive; use active listening to understand concerns and feedback; provide constructive feedback that encourages growth. Decision heuristics: prioritize actions that enhance team cohesion, meet project milestones, and uphold quality standards; when conflicts arise, seek win-win solutions that respect all perspectives. Boundaries: avoid micromanaging; respect work-life balance; do not compromise on ethical standards or team well-being for short-term gains.",
               "LastUpdate": "0001-01-01T00:00:00",
               }
               ...
         ]
         ```
         """;

    public const string PersonalityAnalysisCheatSheet =
        """
        # OCEAN Interpretation

        ## 0) Ground rules

        * **Non-diagnostic, probabilistic language.** Use “tends to / is likely / may” not absolutes. Personality is descriptive, not destiny. Behavior depends on context (trait activation & situational strength). Cite the *situation* when explaining predictions. 
        * **Assume normalized inputs** on 0–1. Map to three bands: Low (≤0.33), Mid (0.34–0.66), High (≥0.67). If percentiles are available, prefer those.
        * **Use facets when provided.** BFI-2 (15 facets) and NEO-PI-R (30 facets) improve fidelity vs coarse domains; emphasize aspect-level nuance (e.g., Industriousness vs Orderliness within Conscientiousness). 
        * **Strengths + watch-outs framing.** Never pathologize low/high ends; both poles have advantages and risks depending on task and norms.
        * **Short, useful outputs.** For individuals: a 3–5 paragraph read. For teams: 5–7 crisp “Highlights,” 5–7 “Risks/Watch-outs,” and 3–5 “Plays” (practical moves).

        ---

        ## 1) Individual trait primers (use when summarizing a single profile)

        For each trait, give: one-liner → likely strengths when high → likely risks when high → likely strengths when low → likely risks when low → coaching nudge.

        **Openness (to Experience)**

        * Curiosity, idea flow, aesthetic sensitivity, unconventionality.
        * High: creative ideation, adaptability to change; risk: analysis paralysis, scope drift.
        * Low: focus on proven methods, stability; risk: rigidity in novel problems.
        * Team note: balances with Conscientiousness for “invent + ship” dynamics; explicitly pair novel exploration with decision gates.

        **Conscientiousness**

        * Self-discipline, reliability, planfulness, goal focus.
        * High: task completion, quality bars; risk: perfectionism, over-control.
        * Low: speed, flexibility; risk: missed details, follow-through gaps.
        * Evidence: strongest broad predictor of job performance across roles; Emotional Stability (low Neuroticism) also general-valid. ([Wiley Online Library][7], [PubMed][8], [ScienceDirect][9])

        **Extraversion**

        * Social energy, assertiveness, positive affect.
        * High: quick rapport, leadership emergence; risk: airtime dominance, status conflict.
        * Low: reflection, deep work; risk: under-voicing ideas.
        * Tip: match channel (async/text) to introverts; use time-boxed debate for extroverts.

        **Agreeableness**

        * Compassion, cooperation, interpersonal trust.
        * High: cohesion, prosocial glue; risk: conflict avoidance, decision diffusion.
        * Low: challenge norms, candor; risk: unnecessary friction.
        * Conflict style tendencies: agreeableness leans toward avoiding/compromising; calibrate with explicit decision rules. ([ScienceDirect][10])

        **Neuroticism** (aka Emotional Stability when reversed)

        * Sensitivity to threat/negative affect; stress reactivity.
        * High N: vigilance to risks; risk: rumination, conflict frequency under strain.
        * Low N: calm under pressure; risk: under-reacting to weak signals.
        * Team note: high N correlates with more perceived conflicts—buffer with clarity, shorter feedback cycles, and psychological safety rituals.

        ---

        ## 2) Duo/Combo patterns (fast heuristics)

        * **High Openness + High Conscientiousness** → innovation that actually ships; schedule “decision locks” to prevent endless exploring.
        * **High Extraversion + Low Agreeableness** → decisive challenge energy; risk: status fights—use rotating facilitation.
        * **High Agreeableness + High Neuroticism** → harmony-seeking but stress-prone; add clear escalation paths and written decisions.
        * **Low Conscientiousness + High Openness** → ideas without anchors; assign a “commit owner” per workstream.

        (These are working theories; always state the situational triggers that would *activate* a trait expression.) )

        ---

        ## 3) Team-level composition logic (how to go from individuals → group)

        When you have multiple agents, compute both **elevation** (mean) and **dispersion** (SD/variance) per trait. Interpret with these empirically backed rules of thumb:

        * **Mean Conscientiousness ↑ → team performance ↑;** **variance in Conscientiousness ↑ → coordination ↓.** Translation: a single low-C can bottleneck tightly coupled work; set minimum quality bars and shared checklists. 
        * **Mean Agreeableness ↑ → cohesion/coordination ↑;** **variance in Agreeableness ↑ → process conflict ↑.** Use explicit debate formats and decision charters when A is mixed. 
        * **Agreeableness particularly helpful under uncertainty.** High-A teams navigate ambiguity with less destructive conflict; preserve dissent channels to avoid groupthink. 
        * **Extraversion dispersion** can be useful for external vs internal roles but raises status conflict risk—balance airtime and clarify role power. (Generalized from team-personality meta-analyses.) 
        * **Situational strength moderates everything.** In tight SOPs (“strong situations”), personality effects shrink; in high-ambiguity or creative work (“weak situations”), they loom larger—say this explicitly. 

        **How to compute (pseudocode the model can follow):**

        ```
        For each trait T in [O,C,E,A,N]:
          mean_T = average(T[i])
          sd_T   = stdev(T[i])
          flags:
            if mean_C >= .67 → "High_C_team"
            if sd_C   >= .20 → "C_misalignment"
            if mean_A >= .67 → "High_A_team"
            if sd_A   >= .20 → "A_misalignment"
            (tune thresholds to your score scale)
        ```

        Then map flags → notes (e.g., High\_C\_team → “strong execution capacity; protect from perfectionism with timeboxes”).

        ---

        ## 4) Role fit & workflow cues (suggest, don’t assign)

        * **Discovery/Research** leans on Openness (idea generation) + Emotional Stability (tolerance for ambiguity).
        * **Program/Project delivery** leans on Conscientiousness (planning/QA).
        * **Stakeholder/BD/Advocacy** leans on Extraversion (assertiveness/energy) + Agreeableness (rapport).
        * Caveat: Validity depends on task cues (trait activation) and domain skills; do not claim determinism. 

        ---

        ## 5) Outputs the model should produce

        ### A) IndividualWriteup (given O,C,E,A,N ∈ \[0,1] and any facets)

        1. **Mini-profile (2–3 sentences):** Plain-language summary of the person’s likely work style keyed to the current project context.
        2. **Trait-by-trait micro-reads:** For each domain, 1–2 sentences on strengths + 1 watch-out tied to *this* situation.
        3. **Interaction notes:** 2–3 lines on the most salient pairwise combos given extremes present.
        4. **Support & stretch:** 3 specific “do more of / add guardrail” suggestions.

        ### B) TeamReport (given the set of IndividualWriteups)

        * **Highlights (5–7):** Evidence-aware positives (e.g., “High mean Conscientiousness → reliable delivery; install ‘good-enough’ gates to avoid over-polish”).
        * **Risks (5–7):** Composition hazards (e.g., “Agreeableness variance → simmering process conflict; adopt decision charter”).
        * **Plays (3–5):** Concrete rituals/tools (checklists, DACI/RACI, time-boxed design sprints, written pre-reads, rotating facilitator).
        * **Heatmap & quick math:** Show trait means and SDs; call out the two highest mismatches.

        ---

        ## 6) Evidence anchors the model may cite (don’t overquote)

        * **Conscientiousness** is the broadest job-performance predictor across roles; Emotional Stability also predicts performance; newer syntheses refine these links. Use this to justify execution claims. 
        * **Team composition** matters: higher average Agreeableness/Conscientiousness helps; within-team variability in those traits tends to hinder coordination. Use these to justify team-level “Highlights/Risks.” 
        * **Agreeableness and conflict:** expect more cooperative styles with higher A; plan safeguards if A is mixed. 
        * **Facet/Aspect nuance**: when available, lean on facet-level or 10-aspect reasoning (e.g., Industriousness vs Orderliness) to avoid caricatures. 
        * **Situational strength & trait activation:** always tie predictions to task ambiguity, norms, and cues. 

        ---

        ## 7) Style & safety instructions for the model

        * Be specific to **current team goals** (e.g., “two-week hard deadline” vs “open-ended research”).
        * Prefer **actionable nudges** over labels: “Use rotating facilitation to balance Extraversion dispersion” beats “too many extroverts.”
        * Avoid sensitive or clinical language; never infer mental health status from Neuroticism.
        * Keep it **short, vivid, and concrete.** 150–250 words per individual; 150–300 words for the team.

        ---

        ### Optional add-ons the model can use when facets are present

        * If using **BFI-2 facets** (e.g., *Industriousness*, *Orderliness*, *Assertiveness*, *Enthusiasm*, *Compassion*, *Politeness*, *Volatility*, *Withdrawal*), favor these in explanations; they map well to real behaviors the team will recognize.
        * If using **NEO-PI-R facets** (six per domain), weave in 1-liners (e.g., *Deliberation*↑ → slower but safer decisions).

        ---

        ## 8) Quick examples of team-level “Plays” the model may recommend

        * **High mean C, rising sd C:** Standardize Definition of Done; add review checklists; timebox polishing. ([UT Research Info][12])
        * **High mean A, mixed E:** Use written pre-reads + round-robin first takes to surface quieter voices; assign “devil’s advocate” on rotation to prevent groupthink. ([SAGE Journals][15])
        * **Low mean N (calm) in a high-volatility environment:** Create “risk radar” segment in stand-ups to counter complacency. ([PubMed][8])

        ---

        ## 9) What not to do

        * Don’t imply fixed traits determine outcomes. Call out **skills, incentives, and structure** as equal levers.
        * Don’t treat scores as moral judgments; both poles can be adaptive or maladaptive depending on the mission.
        * Don’t ignore **task/role demands**; explain how cues will *activate* (or mute) traits. 

        
        """;
}