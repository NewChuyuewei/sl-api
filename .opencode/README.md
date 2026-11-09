# SCP:SL Agent Team for OpenCode v2

A commander + 10 specialist agents for SCP: Secret Laboratory plugin development
(LabAPI 1.1.7 / EXILED 9.15.3, net48), backed by the SCP:SL skills suite.

## Layout

```text
<workspace>/
├── opencode.jsonc            # default_agent: scpsl-commander, project safety rails
├── AGENTS.md                 # project instructions every session loads
├── skills/                   # CANONICAL skills (17) — edit here
└── .opencode/
    ├── agents/               # 11 agent definitions (Markdown, v2 frontmatter)
    └── skills/               # synced copy of ../skills for OpenCode discovery
```

## The team

| Agent | Mode | Edits code? | Purpose |
|---|---|---|---|
| `scpsl-commander` | primary (default) | No (by permission) | Plans, delegates with task packets, enforces gates G0–G5, integrates evidence |
| `scpsl-planner` | subagent | No | Specs, milestones, estimation, acceptance criteria |
| `scpsl-labapi-coder` | subagent | Yes | LabAPI 1.1.7 implementation |
| `scpsl-exiled-coder` | subagent | Yes | EXILED 9.15.3 implementation |
| `scpsl-reviewer` | subagent | No | Code review: lifecycle → thread safety → events → security → perf |
| `scpsl-tester` | subagent | Tests/reports only | Builds, unit tests, server scenarios, bug reports |
| `scpsl-optimizer` | subagent | Measured fixes | Profiling and optimization with before/after numbers |
| `scpsl-security-auditor` | subagent | No | Permission/privacy/input/supply-chain audit |
| `scpsl-refactorer` | subagent | Yes | Behaviour-preserving cleanup and legacy modernization |
| `scpsl-documenter` | subagent | Docs only | README/config/command docs, changelog |
| `scpsl-release-manager` | subagent | Ask-first | Quality gates, packaging, deployment, port sign-off |

No agent pins a model — all inherit your session model. To pin one later, add
`"model": "provider/model-id"` to that agent's frontmatter (see https://opencode.ai/docs/agents).

## Usage

```text
# Start OpenCode in this workspace: scpsl-commander is the default primary agent.
opencode

# Then talk to the commander, or @-mention a specialist directly:
@scpsl-commander Build a LabAPI plugin that gives Scientists a keycard on spawn.
@scpsl-reviewer Review the current plugin changes.
@scpsl-tester Run the lifecycle scenarios for build <hash>.
```

Typical flow the commander runs:
planner → labapi/exiled coder → reviewer → tester → (security-auditor / optimizer as needed)
→ documenter → release-manager. Subagents run at depth 1 by design: specialists return
evidence to the commander instead of delegating onward.

## Permissions model

Permissions are OpenCode v2 ordered rule lists (`action`/`resource`/`effect`, last match wins):

- Read-only roles (planner, reviewer, security-auditor): `edit`/`shell`/`subagent` denied.
- Commander: cannot edit files; may only launch `scpsl-*` subagents.
- Coders/refactorer/optimizer: may edit; shell is ask-by-default with `dotnet build/test`,
  `git status/diff`, and the suite's Python helpers pre-allowed.
- Tester: edits only make sense for tests/reports (enforced by prompt; production fixes are reported, not patched).
- Documenter: may edit `**/*.md`; shell denied except read-only git inspection.
- Release manager: edits and shell ask-first; may call tester/reviewer/documenter for gate evidence.

Tighten or loosen per-agent in its frontmatter. `opencode.jsonc` adds project-wide rails
(`git push`, `rm -rf`, `.env` edits always ask).

## Keeping skills in sync

Canonical skills live in `../skills/` (portable Agent Skills format, also usable outside OpenCode).
After editing them:

```bash
python3 ../skills/scpsl-agent-orchestration/scripts/sync_opencode_skills.py          # mirror into .opencode/skills
python3 ../skills/scpsl-agent-orchestration/scripts/sync_opencode_skills.py --check  # drift check (CI)
python3 ../skills/scpsl-plugin-dev/scripts/validate_skills.py ../skills              # format + prompt-strength validation
```

(Paths above are relative to this `.opencode/` folder; from the workspace root drop the `../`.)

## Troubleshooting discovery

- Agent not listed: file must be `.opencode/agents/<name>.md`, filename = agent ID; check frontmatter parses.
- Skill not advertised: folder must be `.opencode/skills/<id>/SKILL.md` with a `description`; IDs are path-derived and case-sensitive.
- Wrong skill wins: project `.opencode/skills` outranks global/compat locations; check for duplicate IDs.
- Agent can't launch subagents: check the commander's `subagent` permission rules and OpenCode's subagent depth setting (commander → specialists is depth 1, which is the design).
