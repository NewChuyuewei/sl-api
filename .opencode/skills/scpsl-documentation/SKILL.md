---
name: scpsl-documentation
description: Writes documentation for SCP Secret Laboratory plugins — README files, installation and configuration guides, command and permission tables, changelogs, and developer/API docs for LabAPI 1.1.7 and EXILED 9.15.3 plugins. Use when documenting a plugin, writing a README or changelog, or preparing release notes for server owners.
license: MIT
compatibility: Documentation applies to LabAPI 1.1.7 / EXILED 9.15.3 plugins (net48, SCP:SL 14.2.x). Markdown only; no SDK required.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Plugin Documentation

Your readers are **server owners at 1 a.m. with a broken round**, then players, then other
developers. Docs are written for the first reader first: they must be able to install,
configure, and disable your plugin from the README alone, without reading source.

Documentation is part of the feature: a config key, command, or permission node that ships
undocumented is unfinished work (`scpsl-code-review` flags it; release blocks on it).

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
- Config/command/permission tables are checked against the shipped classes/build, character-for-character for keys and defaults — never retyped from a plan.
- README's first screen answers: what, framework + exact versions, gameplay impact, install steps, config location.

### Stop conditions
- Documented behaviour cannot be confirmed in the current build (docs ahead of code): stop and reconcile — reconcile toward the build, then file the gap.
- A release has config/key/node/ID changes with no owner-action migration note: block the release docs until written.

### Definition of done (this skill)
Done when a server owner can install, configure, permission, and disable the plugin from the README alone, verified against a clean-server install.

## The required doc set

| Document | Audience | Template |
|---|---|---|
| `README.md` (repo + release) | Server owners | [assets/readme-template.md](assets/readme-template.md) |
| `CHANGELOG.md` | Owners upgrading | [assets/changelog-template.md](assets/changelog-template.md) |
| Config documentation | Owners | Table inside README, generated from the actual config class |
| Commands & permissions table | Owners/admins | Table inside README (node, allows, suggested default) |
| Developer notes (only if others integrate/build) | Developers | API assembly docs, build instructions, architecture link |

## README rules

1. **First screen answers five questions**: what it does (2-3 sentences, no lore wall),
   framework + exact versions (LabAPI 1.1.7 *or* EXILED 9.15.3 — an EXILED plugin must say
   "requires EXILED; does not run on LabAPI-only servers" and vice versa), gameplay impact /
   transparently-modded statement, install steps, and where the config file appears.
2. **Install steps are copy-paste accurate** for the framework's real folders
   (LabAPI `plugins/` + `configs/<port>/<Plugin>/`; EXILED `Plugins/` + `Configs/<port>-config.yml`
   section named by `Prefix`) — verified on a clean server during QA, not from memory.
3. **Every config key is documented** with default and effect, matching the shipped defaults
   character-for-character (snake_case keys for EXILED YAML; LabAPI `config.yml` keys).
   Document by generating/checking against the config class, never by retyping from the plan.
4. **Every command is documented**: syntax, surface (RA/client `.`/server console), permission
   node, example. Every permission node: what it allows + suggested default group
   (format in `scpsl-security-permissions`).
5. **Custom content is listed**: roles/items/abilities with IDs, base types, spawn rules —
   owners balancing a server need this without decompiling you.
6. **Behaviour honesty**: known incompatibilities and plugin-ordering requirements named;
   Harmony patches (if any) disclosed with what they patch and the config kill-switch;
   data collected/stored (if any) with location and deletion path.
7. **Troubleshooting section**: plugin-not-loading (wrong framework/folder/version gate),
   where to find the log, what to include in a bug report (point at `scpsl-testing-qa`'s
   bug-report shape). Three real entries beat ten generic ones.

## Changelog rules

Keep-a-Changelog shape (`Added / Changed / Fixed / Removed`, newest first), written for owners:
lead with effects ("Custom role X now respects spawn protection"), then the owner action
required — **config changes, renamed keys, new permission nodes, ID changes** get their own
`⚠ Owner action` line with the old→new mapping. Version numbers match the assembly/plugin
`Version` and the git tag exactly; unreleased work stays under `## [Unreleased]`.
Framework/game support changes ("now targets EXILED 9.15.3, drops 9.13") are headline entries,
never footnotes.

## Developer/API docs (when applicable)

- Build instructions that work from a clean clone: SDK, the `-p:SL_REFERENCES` /
  `-p:EXILED_REFERENCES` properties, net48 target, which assemblies are reference-only.
- Published API assemblies (see `scpsl-architecture`): document discovery (loader lookup),
  null-handling when the dependency is absent, API versioning, and a minimal consumer example.
- Architecture decisions worth keeping live in ADRs next to the code; the README links them,
  it does not duplicate them.

## Docs workflow and freshness

Write docs in the same change as the code (the implementation plan from `scpsl-planning`
carries a docs task per feature). At release, `scpsl-migration-release`'s checklist verifies:
README versions == build pins, config table == shipped config, changelog entry exists,
install steps re-verified clean. A doc bug found by an owner ("wrong folder in README")
is triaged like a code bug — it breaks installs just as hard.

Style: plain English, short sections, tables over prose for config/commands, code fences for
anything typed into a console. Screenshots only where the panel/UI genuinely confuses.
