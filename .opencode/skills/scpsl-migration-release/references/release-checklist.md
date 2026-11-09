# Release Checklist — SCP:SL Plugin

Copy into the release PR/issue and check every line. "N/A" with a reason beats an unchecked box.

## Versioning and compatibility

- [ ] Built against pinned targets: SCP:SL 14.2.x, LabAPI 1.1.7 **or** EXILED 9.15.3 (state which), net48
- [ ] Plugin `Version`, assembly version, changelog, and git tag all agree
- [ ] `RequiredApiVersion` / `RequiredExiledVersion` correct (not bypassed)
- [ ] README states framework + versions, install path, and config location for this build
- [ ] Gameplay-impact / transparently-modded statement included in README

## Code health

- [ ] Register/Unregister symmetry audited (events, coroutines killed, Harmony unpatched by own ID,
      custom roles/items unregistered, singletons nulled, NPCs despawned)
- [ ] Enable → round → reload → disable → enable cycle tested with no duplicate behaviour
- [ ] Every command checks a permission node; nodes documented; argument validation in place
- [ ] Config: safe defaults, broken-YAML boot tested, key renames (if any) have a migration note
- [ ] No main-thread blocking (no Thread.Sleep / sync IO / Task.Delay game delays); MEC used
- [ ] No per-frame allocations or hot-path LINQ introduced; debug logging behind config flag
- [ ] Direct game-type/Harmony uses isolated, justified (Harmony gate), and version-noted
- [ ] No secrets, tokens, hard-coded staff IDs, or server-specific paths in source or defaults

## Package contents

- [ ] Zip contains: plugin `.dll`, genuine third-party dependencies only, README, LICENSE, CHANGELOG
- [ ] Zip does NOT contain: `Assembly-CSharp*.dll`, `LabAPI.dll`, `Exiled.*.dll`, Unity/Mirror
      assemblies, `.pdb` (unless intentionally shipped), your local config files, test servers
- [ ] Clean-server smoke test passed from the zip alone (see deployment.md)
- [ ] Dual-target releases: LabAPI and EXILED zips built, named, and documented separately;
      shared-core version identical in both

## Documentation

- [ ] Feature list with config keys/defaults table and command table (command, surface, permission)
- [ ] Custom content (roles/items/abilities) listed with IDs and spawn rules
- [ ] Known incompatibilities / ordering requirements named, if any
- [ ] Upgrade notes from the previous release (config changes, ID changes — ideally none)
