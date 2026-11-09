---
name: scpsl-security-permissions
description: Secures SCP Secret Laboratory plugins — permission design, command and input validation, Remote Admin abuse prevention, player-data privacy, dependency and supply-chain hygiene for LabAPI 1.1.7 and EXILED 9.15.3 plugins. Use when designing permissions, adding admin commands, handling player data or external input, or auditing an SCP:SL plugin for security issues.
license: MIT
compatibility: Applies to LabAPI 1.1.7 (LabApi.Features.Permissions) and EXILED 9.15.3 (Exiled.Permissions) plugins, net48, SCP:SL 14.2.x.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Security and Permissions

A plugin runs with full server trust, inside the game process, often operated by admins who
did not write it. Your threat model is not "the internet" — it is **unprivileged players,
semi-trusted staff, malicious or careless co-plugins, and your own future config files.**
Design for those four.

Pair with `scpsl-code-review` (which flags these issues) — this skill is how to *build* it right.

## Mandatory operating contract

This skill operates under the suite-wide priority hierarchy
(P0 server stability > P1 lifecycle/data > P2 security > P3 correctness > P4 performance > P5 maintainability > P6 style)
defined in `scpsl-plugin-dev/references/engineering-standard.md`. Conflicts resolve upward; never trade downward silently.

- **MUST** verify every framework API claim (event name, arg property, namespace, signature) against the pinned upstream
  (LabAPI 1.1.7 / EXILED 9.15.3) before using it. Anything unverifiable is written as `UNVERIFIED: <claim>` and must not ship as fact.
- **MUST NOT** invent events, wrappers, config keys, permission nodes, or file paths. Unknown means look it up or stop — never guess plausibly.
- **MUST** distinguish executed evidence from intention: never claim built/tested/reviewed/faster without having actually run it (see Evidence Standard).
- **MUST** be the whole change: code + config + docs + changelog + test evidence land together, or the delivery note names what is missing.
- If a missing fact would change framework, architecture, security, or gameplay behaviour, **STOP and ask**. Otherwise state the assumption in one line and proceed.

### Hard gates (this skill)
- Deny-first: every privileged path checks a documented node before any side effect; errors in the permission system deny, never allow.
- All player/config/command input is validated and clamped; player-supplied display text is escaped for its presentation layer.

### Stop conditions
- A feature would collect credentials/tokens, conceal behaviour from admins/logs, or execute config/player-supplied code or paths: refuse and redesign.
- A privileged action has no natural permission owner (who should hold this node?): stop and resolve the authorization design first.

### Definition of done (this skill)
Done when the security checklist (assets/security-checklist.md) passes line-by-line with the node table published in the README.

## Permission design

1. **Every privileged action gets a node.** Commands, and any event/gameplay path that can be
   used to achieve the same effect without the command. Node scheme `<prefix>.<area>.<action>`
   (`my_plugin.roles.assign`), stable forever — renaming a node silently revokes (or worse,
   the owner's sloppy wildcard re-grants) access on every server.
2. **Check server-side, check first.** LabAPI: `sender.HasPermission("node")`
   (`LabApi.Features.Permissions` extensions — note the server console sender passes by
   design; player senders are actually checked). EXILED: `sender.CheckPermission("node")`
   (`Exiled.Permissions.Extensions`). The check is the first statement after argument-count
   validation, before any state change, lookup side-effect, or expensive work.
3. **Deny by default, deny gracefully.** No grant found = false. Denial returns a clear
   response and changes nothing. Never fail open on a permissions-system error — log it and deny.
4. **Granularity matches blast radius.** Viewing info, mutating one player, mutating the round,
   and mutating the server are four different nodes. Wildcard grants are the owner's explicit
   choice in their permissions config, never your code's assumption (`HasPermission("*")`
   checks are forbidden).
5. **Document the permission surface** (node, what it allows, suggested default group) in the
   README table. Undocumented nodes get wildcard-granted by frustrated owners — your design
   caused that. See [references/permission-design.md](references/permission-design.md).

## Command and input validation

- Treat **every** argument as hostile: count, parse with `TryParse`, range-clamp numbers
  (durations, amounts, counts, probabilities), cap list sizes. Oversized/negative values are
  a crash and griefing vector, not a config preference.
- Resolve targets explicitly: player must exist, be connected, and be *eligible* (alive,
  correct state, not the server host/NPC unless the command says so). Ambiguous name matches
  are rejected with a list, never guessed.
- Client (`.` console) commands get the strictest treatment: the caller is an unprivileged
  player. No client command may mutate other players, the round, or the server without an
  explicit permission node and rate/cooldown limiting.
- Anything parsed from config files is also input: validate at load (types, ranges, lengths)
  and fall back to defaults with warnings. A malformed YAML must never become an injection
  into hints/broadcasts shown raw to other players without considering formatting/control
  codes — sanitize or escape owner/player-supplied display strings where the presentation
  layer interprets markup (rich text in hints/broadcasts).
- Cooldowns/rate limits on player-triggerable expensive actions (spawns, effects, queries)
  are security controls (anti-spam/anti-lag), not just balance. Put them in config.

## Player data and privacy

- Collect the minimum: UserID + what the feature needs. No IP logging at info level, no
  token/password ever (plugins should never see auth tokens — if code handles one, stop and redesign).
- Persistent data (see `scpsl-architecture`): document what is stored, where, and retention;
  provide an owner-facing deletion path; respect that UserIDs are personal data in many
  jurisdictions. Logs: UserIDs at Debug, nicknames preferred at Info; never dump full
  player objects/session data into public-facing outputs.
- Data shown to other players (leaderboards, inspect commands) needs a permission/visibility
  decision: what may a random player learn about others from your plugin? Default: nothing
  beyond what the base game already shows.

## Co-plugin and supply-chain hygiene

- Optional dependencies are resolved defensively: plugin absent → feature off, one Info log.
  Never reflection-load unknown assemblies by name from config. Never execute code, URLs,
  or file paths supplied by config/player input (no dynamic assembly loading, no process starts).
- Harmony (if used): patch only with written justification per `scpsl-harmony-patching`;
  never patch permission checks, the loaders, or verification/anti-cheat paths; never use a
  patch to conceal plugin behaviour from admins or server logs. Concealment features are
  rejected outright, no matter who asks.
- Dependencies: pin versions, prefer framework-provided libraries, ship only real
  dependencies (see release checklist), and record them (name + version + why) in the README.
  A plugin that silently bundles a networking/crypto library it barely uses is a review finding.
- Updates: state your update channel; never auto-download or auto-replace assemblies at
  runtime. Server owners update plugins deliberately, from your releases, with changelogs.

## Secure defaults checklist (apply per plugin)

- [ ] All privileged commands + equivalent gameplay paths check documented nodes, deny-first
- [ ] All numeric/string inputs validated and clamped; targets resolved with eligibility checks
- [ ] Player-triggerable actions have cooldowns/caps in config
- [ ] Display strings from players/config are escaped/sanitized for the presentation layer
- [ ] Stored player data is minimal, documented, deletable; logs avoid IPs/tokens/full IDs at Info
- [ ] No dynamic code loading, no process execution, no runtime self-updating
- [ ] Optional co-plugin integrations degrade silently-off and never crash when absent
- [ ] Dependencies pinned, minimal, declared in README

Full threat catalogue: [references/threat-model.md](references/threat-model.md).
