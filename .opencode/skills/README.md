# SCP:SL Plugin Development — Agent Skills Suite

A suite of [Claude Agent Skills](https://agentskills.io/specification) for developing
**SCP: Secret Laboratory (SCP:SL)** server plugins against **both** modern frameworks:

- **LabAPI 1.1.7** — Northwood's official plugin loader/framework, bundled with the game.
- **EXILED 9.15.3** — the community high-level framework by the ExMod Team.

All skills are written in English, follow the Claude Agent Skills format
(`SKILL.md` + YAML frontmatter + progressive disclosure via `references/`, `assets/`, `scripts/`),
and pin the **latest stable** releases only. Beta / pre-release / dev-branch APIs are out of scope.

## Pinned versions (verify before every release)

| Component | Pinned version | Notes |
|---|---|---|
| SCP:SL dedicated server | 14.2.x (14.2.8) | Reference assemblies come from `SCPSL_Data/Managed/` of this build. |
| LabAPI | **1.1.7** | NuGet: `Northwood.LabAPI` 1.1.7. Bundled as `LabAPI.dll` with the server. |
| EXILED | **9.15.3** | GitHub release. NuGet `ExMod.Exiled` may lag one patch (9.15.2 at pin time) — see the EXILED skill. |
| Target framework | **net48** | Both frameworks. Build with a modern .NET SDK (8+, 10 recommended by the LabAPI wiki), target `net48`. |
| Language version | `latest` (12/13) | Set explicitly in the `.csproj`. |
| Harmony | 2.2.2 (`Lib.Harmony`) | Only when events are not enough — see `scpsl-harmony-patching`. |
| Coroutines | MEC (`Timing`) | Ships with the game; used by both frameworks. Never use threads/`Task.Delay` for game logic. |

> Version pins live in exactly one place per skill so they are easy to bump.
> When a new stable ships, update `scpsl-plugin-dev/references/version-pins.md`,
> the `compatibility`/`metadata` fields, and the template `.csproj` files, then re-run the validator.

## Engineering standard (prompt strength)

Every skill in this suite carries a **Mandatory Operating Contract** — enforceable rules, not
suggestions — modelled on large-scale production engineering practice (Google-style code health,
small complete changes, testing-as-part-of-the-change; Cloudflare-style reliability, measured
performance, security/privacy by default):

- **Priority hierarchy P0–P6** arbitrates conflicts: server stability > lifecycle/data > security >
  correctness > measured performance > maintainability > style. Trading downward silently is forbidden.
- **Hard gates + stop conditions per skill**: e.g. no Harmony patch without written justification;
  no release on a missing QA sign-off; no API claim shipped unverified against the pinned upstream.
- **Evidence standard**: "builds" needs a command + result, "works" needs scenario IDs + build hash,
  "faster" needs before/after numbers — otherwise the work is reported as in-progress with open gates named.
- **Quality gates G0–G5** by risk tier T1–T4 govern merge/release (`scpsl-plugin-dev/references/quality-gates.md`).
- The validator enforces the contract's presence: a skill without MUST-level rules, Hard Gates, and a
  Definition of Done fails `validate_skills.py`.

## Skills in this suite (14)

### Routing and implementation
| Skill | Use it when |
|---|---|
| `scpsl-plugin-dev` | Starting any SCP:SL plugin task. Routes to LabAPI vs EXILED, dual-target architecture, shared workflow and guardrails. **Start here.** |
| `scpsl-labapi` | Building a LabAPI 1.1.7 plugin: `Plugin`/`Plugin<TConfig>`, `CustomEventsHandler`, wrappers, commands, config, permissions. |
| `scpsl-exiled` | Building an EXILED 9.15.3 plugin: `Plugin<Config>`, event handlers, commands, config/translations, permissions. |
| `scpsl-custom-content` | Custom roles, custom items/weapons, custom abilities, NPCs — EXILED first-class APIs vs LabAPI do-it-yourself patterns. |
| `scpsl-harmony-patching` | Patching base-game methods with Harmony when no event exists, in either framework. |

### Full development lifecycle
| Skill | Use it when |
|---|---|
| `scpsl-planning` | Planning work: requirements, feature specs, milestones, task breakdown, estimation, acceptance criteria. |
| `scpsl-architecture` | Designing solution structure: shared Core/adapters, state management, inter-plugin APIs, persistence, config design. |
| `scpsl-security-permissions` | Designing permission nodes, validating commands/input, protecting player data, dependency hygiene. |
| `scpsl-code-review` | Reviewing/auditing plugin code or PRs (includes a heuristic scanner script): lifecycle symmetry, thread safety, API misuse. |
| `scpsl-testing-qa` | Test strategy, Core unit tests, dedicated-server scenarios, regression and sign-off (includes a test-matrix generator). |
| `scpsl-optimization` | Measuring and fixing lag: hot handlers, allocations, MEC loops, lookups, network fan-out (includes a server-log analyzer). |
| `scpsl-refactoring` | Behaviour-preserving cleanup and legacy modernization (NwPluginAPI/old EXILED → pinned stack). |
| `scpsl-documentation` | READMEs, install/config/command docs, changelogs, and developer/API documentation for server owners. |
| `scpsl-migration-release` | Porting LabAPI ↔ EXILED, building, deploying to a server, debugging load failures, and release checklists. |

Typical end-to-end flow:
`scpsl-planning` → `scpsl-architecture` → framework skill (+ topic skills) →
`scpsl-code-review` → `scpsl-testing-qa` → `scpsl-documentation` → `scpsl-migration-release`,
with `scpsl-optimization`, `scpsl-security-permissions`, and `scpsl-refactoring` pulled in whenever the work calls for them.

## Installation

Copy any (or all) skill folders into your agent's skills directory, keeping the folder name —
it must match the `name` field in `SKILL.md`:

```text
# Claude Code (project scope)
<your-repo>/.claude/skills/scpsl-plugin-dev/
<your-repo>/.claude/skills/scpsl-labapi/
...

# Claude Code (personal scope)
~/.claude/skills/scpsl-plugin-dev/
```

Other Agent-Skills-compatible runtimes follow the same layout: one folder per skill,
`SKILL.md` at the root of the folder, sibling `references/`, `assets/`, and `scripts/` loaded on demand.

## Quick start for agents

1. Load `scpsl-plugin-dev` first. Decide **LabAPI, EXILED, or dual-target** using its decision matrix.
2. For non-trivial work, plan with `scpsl-planning` and structure with `scpsl-architecture`.
3. Load exactly one framework skill (`scpsl-labapi` or `scpsl-exiled`) for the implementation,
   plus topic skills only when needed (`scpsl-custom-content`, `scpsl-harmony-patching`,
   `scpsl-security-permissions`).
4. Before calling anything done, run it through `scpsl-code-review` and `scpsl-testing-qa`;
   ship with `scpsl-documentation` + `scpsl-migration-release`.
5. Scaffold a new project instead of writing boilerplate from scratch:

```bash
python3 scpsl-plugin-dev/scripts/new_plugin.py --api labapi  --name MyPlugin --output ./MyPlugin
python3 scpsl-plugin-dev/scripts/new_plugin.py --api exiled  --name MyPlugin --output ./MyPlugin
```

6. Validate the suite after editing any skill:

```bash
python3 scpsl-plugin-dev/scripts/validate_skills.py .
```

## Repository layout of a typical dual-target plugin solution

```text
MyPlugin/
├── MyPlugin.Core/          # Pure C# logic, NO LabAPI/EXILED/game references (net48)
├── MyPlugin.LabApi/        # LabAPI 1.1.7 adapter: Plugin, handlers, commands
├── MyPlugin.Exiled/        # EXILED 9.15.3 adapter: Plugin<Config>, handlers, commands
└── README.md               # States framework, version pins, install path, config docs
```

Rule of thumb: game/framework calls live in the adapter projects; reusable rules, calculations,
and data models live in `Core` so both adapters — and unit tests — can share them.

## Authoritative sources

- LabAPI repo & wiki: https://github.com/northwood-studios/LabAPI — releases, `LabApi.Examples`
- LabAPI NuGet: https://www.nuget.org/packages/Northwood.LabAPI
- EXILED repo: https://github.com/ExMod-Team/EXILED — releases, `Exiled.Example`, `.github/documentation`
- EXILED docs & API reference: https://docs.exiled-team.net / https://exiled-team.github.io/EXILED
- EXILED NuGet: https://www.nuget.org/packages/ExMod.Exiled
- Agent Skills specification: https://agentskills.io/specification

If any snippet in these skills disagrees with the pinned upstream source, **upstream wins** —
fix the skill and bump its `metadata.version`.
