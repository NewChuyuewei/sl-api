# Threat Model for SCP:SL Plugins

## Actors

| Actor | Capabilities assumed | Primary defences |
|---|---|---|
| Unprivileged player | Client commands, gameplay events, crafted arguments/nicknames/display strings, timing/spam | Permission nodes, validation, cooldowns, sanitization, eligibility checks |
| Semi-trusted staff (RA access) | RA commands of every plugin, config-adjacent powers | Granular nodes (RA ≠ your plugin's admin), audit logging |
| Other plugins (buggy/hostile) | Event vetoes/mutations before you, Harmony patches, shared IDs/commands | Respect incoming `IsAllowed`, namespaced nodes/commands/IDs, defensive arg handling |
| Server-owner config | Broken or adversarial YAML, extreme values | Load-time validation, clamping, safe defaults, escaped display strings |
| Supply chain | Compromised/vulnerable dependency, typosquatted package | Pinned minimal dependencies, declared in README, no runtime downloads |

## Classic exploit shapes in this ecosystem

- **Client-command escalation**: a `.` command that changes roles/items without a node check.
- **Argument overflow griefing**: negative/huge amounts (`give -1`, duration `999999`) crashing
  handlers or pinning players. Clamping is the fix; add hostile-input scenarios in QA (H1-H4).
- **Display-string injection**: player-controlled text (nicknames, custom info, config strings)
  re-shown with rich-text interpretation to other clients — fake admin broadcasts, UI breakage.
- **State desync via reload**: permissions or cooldowns cached forever in statics; re-check per
  invocation, clear on disable (QA scenario H4 exists precisely for this).
- **Dependency confusion**: shipping game/framework DLLs in the release zip lets an outdated
  copy shadow the server's — a stability *and* integrity bug; the release checklist forbids it.

## Out of scope (say so, don't half-build)

Client-side anti-cheat, network MITM between game client and server, and platform account
security are base-game/platform concerns. Plugins must not bolt on credential collection
"for verification" — that pattern is how account theft ships in this ecosystem.
