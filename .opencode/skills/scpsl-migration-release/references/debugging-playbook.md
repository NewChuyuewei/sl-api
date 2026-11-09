# Debugging Playbook — SCP:SL Plugins

Always start from the **first** error in the server log after a clean boot. Later errors are
usually cascades. Reproduce on the smallest server (your plugin alone, then add suspects).

## Plugin never loads

| Check | How |
|---|---|
| Right framework build? | EXILED DLL needs EXILED installed; LabAPI DLL needs the LabAPI loader (bundled). Mismatch = invisible plugin. |
| Right folder? | See deployment.md; confirm against the generated folders on that server. |
| Loader version gate | `RequiredApiVersion` (LabAPI) / `RequiredExiledVersion` (EXILED) vs installed framework version in the boot log. |
| Target framework | Assembly must be net48. A net8/netstandard build will not load. |
| Missing dependency | First `FileNotFoundException` names it. Non-plugin deps → dependencies folder; never ship game/framework DLLs to "fix" it. |
| Duplicate/old DLL | Two versions in the plugins folder load unpredictably. Remove, don't rename-in-place (`Plugin.dll.bak` can still be picked up by naive loaders on some setups — move it out). |

## Loads, then exceptions in Enable/OnEnabled

Enable-time code runs before any round exists: no players, no map, no round state.
Usual causes: touching `Player.List`/map wrappers at enable, config values used before load,
event subscription throwing midway (leaving half-registered handlers — fix the throw, then
audit Register/Unregister symmetry). Config must be null-safe with defaults; validate and clamp.

## Feature silent in-game

- Is the event the right *moment*? (EXILED `Verified` vs LabAPI `Joined`; `-ing` vs past-tense.)
- Did an earlier plugin veto (`IsAllowed=false` before your handler)? Log the incoming value at debug.
- Is the player actually eligible (role/team/alive/NPC checks filtering everyone out)?
- Coroutine/feature flag: config toggles and MEC handles killed on round end are frequent culprits.
- One debug line per decision point, behind the Debug config flag; remove per-event spam after.

## Reload makes it worse (double events, ghosts, duplicates)

Asymmetric cleanup, almost always. Diff: event `+=` vs `-=` lists; MEC handles killed;
Harmony `UnpatchAll(yourId)` called; custom roles/items unregistered; singletons nulled;
NPCs despawned; per-player dictionaries cleared. Test cycle: enable → round → reload →
round → disable → enable, watching counts (handlers must fire exactly once per event).

## Inter-plugin conflicts

- Event ordering follows registration/load priority — don't "fix" ordering by raising your
  priority silently; document required ordering and the other plugin's name.
- Harmony composition: see scpsl-harmony-patching troubleshooting (postfix composes, prefix claims).
- Shared resources (custom role/item IDs, permission nodes, command names) collide by string/number:
  namespace yours (`Prefix`-based nodes and command names, documented ID ranges).

## Performance

Symptom: server tick hitches with your plugin on. Suspects in order: per-frame/per-tick loops
without throttling, LINQ + allocations in hot event handlers (damage, shots, movement-adjacent
events), string logging built even when disabled, reflection in patches, disk/network IO on the
main thread. Fix by caching, early-outs, MEC-throttled loops (seconds, not frames), and
moving IO off the event path (results applied back via MEC on the main thread).
