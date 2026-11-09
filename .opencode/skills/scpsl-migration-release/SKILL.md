---
name: scpsl-migration-release
description: Ports SCP Secret Laboratory plugins between LabAPI 1.1.7 and EXILED 9.15.3, and handles building, deploying, debugging, and releasing SCP:SL plugins. Use when migrating or porting a plugin to the other framework, creating a dual-target release, fixing plugin load or missing-reference errors, deploying a DLL to a server, or preparing a plugin release checklist.
license: MIT
compatibility: Requires the scpsl-plugin-dev, scpsl-labapi, and scpsl-exiled skills; a .NET SDK 8+ targeting net48; SCP:SL 14.2.x, LabAPI 1.1.7, EXILED 9.15.3.
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  game: "14.2.x"
  target-framework: "net48"
---

# SCP:SL Migration, Deployment, Debugging, and Release

This skill covers the last mile and the framework hop: **porting** LabAPI ↔ EXILED,
**building** reproducibly, **deploying** to a real server layout, **debugging** load/runtime
failures, and **releasing** without breaking server owners.

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
- Gates G0-G5 (scpsl-plugin-dev/references/quality-gates.md) pass with evidence for the change's risk tier; failures follow the gate-failure protocol, never silent acceptance.
- The release zip is inspected: plugin DLL + genuine dependencies only. Any game/framework/Unity/Mirror DLL inside is an automatic release block.

### Stop conditions
- QA sign-off (build hash + scenario results) is missing or names a different build: stop — do not release an untested artifact.
- Config/key/node/ID changes lack a migration note and changelog owner-action line: stop until written.

### Definition of done (this skill)
Done when a clean server installs from the zip alone, boots with a clean log, survives a round + reload, and the checklist (references/release-checklist.md) is fully checked.

## Porting LabAPI ↔ EXILED — method

Porting is translation, not transliteration. In order:

1. **Inventory** the old plugin: entry point + priority, config keys/defaults, every event
   subscribed, every command + permission node, custom content (roles/items/abilities),
   coroutines, Harmony patches, direct game-type uses, persisted data (files, UserID-keyed state).
2. **Extract core logic** into a framework-free net48 `*.Core` project (rules, calculations,
   parsing, state machines). If the timeline forbids it, keep logic in services that take
   primitives — but never let the new adapter call the old framework "temporarily".
3. **Map the surface** with [references/api-mapping-table.md](references/api-mapping-table.md):
   lifecycle, config, events (names **and** cancellation semantics differ), player wrappers,
   commands, permissions, logging, coroutines.
4. **Rebuild the adapter**: scaffold the target framework with
   `../scpsl-plugin-dev/scripts/new_plugin.py`, then move feature-by-feature:
   config → handlers → commands → custom content → patches last (re-run the Harmony gate;
   the other framework often has an event for what needed patching before).
5. **Translate config**: write a conversion table old key → new key (LabAPI per-plugin
   `config.yml` + `properties.yml` vs EXILED port YAML section under `Prefix`). Provide a
   one-time migration note for owners; never read the other framework's config files at runtime.
6. **Parity tests**: same scenario list on both builds (join, round start, feature trigger,
   role change, death, round restart, plugin reload). Divergences are usually event-ordering
   or cancellation defaults — fix in the adapter, not by contorting Core.

Direction-specific traps:

- **LabAPI → EXILED**: `Plugin.Enable/Disable` → `OnEnabled/OnDisabled`; `properties.yml`
  enable → `IConfig.IsEnabled`; `CustomEventsHandler` overrides dissolve into explicit
  `Handlers.X +=/-=` pairs (audit symmetry — see the EXILED skill); hand-rolled custom
  roles/items should usually become real `CustomRole`/`CustomItem` subclasses, not ports
  of the DIY registry.
- **EXILED → LabAPI**: `CustomRole`/`CustomItem` subclasses become DIY definitions + registry
  (see `scpsl-custom-content` LabAPI patterns) — budget real time for spawn/tracking/cleanup;
  translations fold into config or a bespoke file; `CheckPermission` nodes map to LabAPI
  permission checks with the same node strings so owners keep their grants meaningful.
- Both directions: event **arge property names and `IsAllowed` defaults must be re-verified
  per event** in the target framework's pinned docs. Similar names, different contracts
  (e.g. join semantics: LabAPI `PlayerJoined` vs EXILED `Verified` are not the same moment).

## Building

- Release configuration, `net48`, pinned framework versions, `Private=false` on every
  game/framework reference. Build output = **one plugin DLL** (+ genuine third-party deps).
- Reproducibility checks before release: clean clone builds with only `-p:SL_REFERENCES=…`
  / `-p:EXILED_REFERENCES=…` supplied; no absolute paths committed; version in assembly,
  plugin class, and changelog agree.
- Multi-target solutions: `Core` builds standalone (`dotnet build` + unit tests off-server);
  adapters build independently — an EXILED adapter must compile on a machine with no LabAPI
  plugin project quirks and vice versa.

## Deploying (summary — full detail in references/deployment.md)

| | LabAPI 1.1.7 | EXILED 9.15.3 |
|---|---|---|
| Plugin DLL | LabAPI `plugins/` | EXILED `Plugins/` |
| Non-plugin deps | LabAPI `dependencies/` | EXILED plugins dependencies folder per install |
| Config appears at | LabAPI `configs/<port>/<Plugin>/` (`config.yml`, `properties.yml`) | `Configs/<port>-config.yml`, section = plugin `Prefix` |
| Enable/disable | `properties.yml` | `is_enabled` in the config section |
| Framework install | Bundled with the game | Owner installs EXILED release |

Stop/restart (or the framework's reload), then read the **server log from the first error**.
First boot generates config — edit config only after it exists, then restart/reload again.

## Debugging playbook (summary — full detail in references/debugging-playbook.md)

- **Plugin not loaded at all**: wrong folder, wrong framework build (EXILED DLL on LabAPI-only
  server and vice versa), `net48` violated, missing dependency, or loader version gate
  (`RequiredApiVersion` / `RequiredExiledVersion`). Verify each in that order.
- **`FileNotFoundException` / `TypeLoadException`**: a reference was shipped that shouldn't be,
  or not shipped that should; check `Private=false` set and the dependencies folder.
- **Loaded but silent**: events never subscribed (enable threw midway — find the first
  exception), config disabled the plugin, or the event moment differs from your assumption
  (Verified vs Joined). Add a temporary `Debug` log line at each registration.
- **Works until reload**: asymmetric cleanup — diff Register/Unregister lists, kill coroutines,
  unpatch Harmony with your own ID, unregister custom content, null singletons.
- **Works alone, breaks with others**: event veto ordering or Harmony composition; see the
  EXILED/Harmony skills. Reproduce with the minimal plugin set.

## Release checklist (summary — full list in references/release-checklist.md)

Version pins stated (game 14.2.x / LabAPI 1.1.7 or EXILED 9.15.3); enable→round→reload→disable
cycle tested; config migration notes written; permission nodes documented; gameplay-impact /
transparently-modded statement; release zip contains only the plugin DLL (+ real deps, README,
LICENSE, changelog); no game/framework/Unity/Mirror DLLs; install path verified on a clean server.

## References

- [references/api-mapping-table.md](references/api-mapping-table.md)
- [references/deployment.md](references/deployment.md)
- [references/debugging-playbook.md](references/debugging-playbook.md)
- [references/release-checklist.md](references/release-checklist.md)
