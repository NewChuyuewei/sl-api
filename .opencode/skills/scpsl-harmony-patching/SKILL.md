---
name: scpsl-harmony-patching
description: Patches SCP Secret Laboratory base-game methods with Harmony 2.2.2 inside LabAPI 1.1.7 or EXILED 9.15.3 plugins when no event or wrapper API exists. Use when a feature requires a prefix, postfix, transpiler, or finalizer on game code, when debugging patch conflicts or publicized-assembly issues, or when reviewing whether patching is justified at all.
license: MIT
compatibility: Requires Lib.Harmony 2.2.2 (NuGet Lib.Harmony) and the scpsl-labapi or scpsl-exiled skill for the host plugin (net48, SCP:SL 14.2.x).
metadata:
  version: "1.1.0"
  labapi: "1.1.7"
  exiled: "9.15.3"
  harmony: "2.2.2"
  game: "14.2.x"
  target-framework: "net48"
---

# Harmony Patching for SCP:SL Plugins (Last Resort, Done Safely)

Harmony modifies base-game methods at runtime. It is the most powerful and most fragile tool
in SCP:SL development: game updates silently retarget it, and two plugins patching the same
method compose in load order whether they like it or not.

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
- No patch ships without: written justification (missing event/API named), smallest stable target, fail-open error path, config kill-switch, own-ID unpatch.
- Transpilers additionally require an anchor-missing fallback that yields the original IL unchanged.

### Stop conditions
- An event or wrapper achieves the goal, or the target signature cannot be confirmed in the pinned game assembly: stop — no patch.
- The patch would touch permission checks, loaders, or verification paths: refuse, at any rank.

### Definition of done (this skill)
Done when: patch applies on the exact game build, fails open on error, composes with the server's plugin set, and self-disables cleanly via config.

## The gate — answer before writing any patch

1. Is there a framework **event** for this (check `scpsl-labapi` / `scpsl-exiled` catalogues,
   including `-ing` cancellable args and mutable args)? → Use the event. Stop.
2. Is there a **wrapper/API method** that achieves it (wrapper property, `SetRole`, config of
   the game system itself)? → Use the API. Stop.
3. Can the feature be **re-scoped** to what events allow without ruining it? → Re-scope. Stop.
4. Only then patch — and write the reason (which event/API is missing) in a comment on the
   patch class and in the plugin README. Reviewers must be able to re-run this gate.

Unjustified patches are rejected in review. Every patch is a maintenance debt against the
next game update.

## Setup

- NuGet: `Lib.Harmony` **2.2.2**. Do **not** bundle Harmony into an EXILED plugin's output if
  EXILED already ships it — reference it `Private=false` there; in a LabAPI plugin, follow the
  host server's dependency practice (LabAPI `dependencies/` folder) and never ship a second,
  conflicting Harmony copy inside the plugin DLL. When in doubt, ask how the target server
  loads dependencies and mirror neighbouring plugins.
- One `Harmony` instance per plugin, ID = your plugin's unique reverse-DNS/prefix:
  `new Harmony("com.example.myplugin")` — never another plugin's ID, never a bare name.
- Patch in enable, unpatch in disable, with **your instance only**:

```csharp
private Harmony? _harmony;

public override void OnEnabled()   // EXILED shown; LabAPI: Enable()/Disable()
{
    _harmony = new Harmony("com.example.__plugin_name__");
    _harmony.PatchAll(Assembly.GetExecutingAssembly());
}

public override void OnDisabled()
{
    _harmony?.UnpatchAll("com.example.__plugin_name__");
    _harmony = null;
}
```

`UnpatchAll()` without your ID (or unpatching "everything") breaks other plugins — never do it.
Log patched/failed counts at enable; a patch that fails to apply must be loud (`Log.Error`),
not silent, because the feature is now half-alive.

## Patch shape

```csharp
using System;
using HarmonyLib;
using LabApi.Features.Console; // or Exiled Log — match the host plugin

// JUSTIFICATION: No LabAPI/EXILED event fires before <behaviour>; the closest
// event (<name>) is post-only and cannot <requirement>. Verified against
// LabAPI 1.1.7 / EXILED 9.15.3 event catalogues on the 14.2.x game build.
[HarmonyPatch(typeof(TargetGameType), nameof(TargetGameType.TargetMethod))]
internal static class TargetMethodPatch
{
    // Prefix: return false to skip the original. Keep it allocation-free.
    private static bool Prefix(TargetGameType __instance, ref int __result)
    {
        try
        {
            // Minimal logic; delegate real work to a service class.
            return true; // run the original
        }
        catch (Exception ex)
        {
            Logger.Error($"[__PLUGIN_NAME__] Patch failed, falling back to vanilla: {ex.Message}");
            return true; // fail open to vanilla behaviour
        }
    }
}
```

Rules:

- **Fail open.** A throwing patch must not break the vanilla game loop: catch, log once with
  context, and let the original run (prefix returns `true`, postfix tolerates). A patch that
  can crash the round is worse than a disabled feature. Feature-flag patches in config so
  owners can disable a misbehaving patch without removing the plugin.
- **Minimal surface.** Patch the smallest, most stable method; prefer postfix (observe/adjust
  result) over prefix (skip/replace), and prefix over transpiler. Transpilers (IL rewriting)
  need a written justification of their own and break most often — avoid unless nothing else works.
- **No game-state mutation off-thread, no blocking, no per-frame allocation** in hot methods.
  If the target runs per tick/per bullet, the patch body must be a few field reads at most;
  cache lookups outside the patch.
- **Signature honesty.** Match the exact 14.2.x method (name, parameters, generics) from the
  referenced assembly. `nameof` where possible; string names only for non-public members,
  with the access pattern (`AccessTools.Method(typeof(T), "Name")`) in one place.
- **Non-public members** need the publicized assembly (EXILED dev) or reflection accessors.
  Isolate every such access behind a small `internal static class GameAccess` so the next
  game update breaks exactly one file.

## Conflicts and compatibility

- Assume other plugins patch the same methods (EXILED itself is Harmony-based). Compose:
  postfixes that adjust results must read-modify-write defensively; prefixes that return
  `false` are claiming the behaviour — document that claim in the README's compatibility section.
- Harmony priority/before/after attributes (`[HarmonyPriority]`, `[HarmonyBefore/After]`)
  only when a real ordering requirement exists, naming the other mod ID.
- Version-guard brittle patches: check the game/LabAPI/EXILED version at enable
  (`LabApiProperties.CurrentVersion` / assembly versions) and self-disable the patch with a
  clear log line on unknown builds instead of patching blind.
- Never patch: the plugin loaders themselves, permission checks of other plugins, or
  anti-cheat/verification paths. Never use patches to hide a plugin from admins or server listing.

## Debugging patches

- "Feature dead, no error" → patch didn't apply (signature drift) or another plugin's prefix
  skipped the original first. Log `harmony.GetPatchedMethods()` at debug level during development.
- "Worked until game update" → re-diff the target method in the new `Assembly-CSharp`;
  update, retest, bump the plugin's supported-version note. See
  [references/troubleshooting.md](references/troubleshooting.md).
- "Works alone, breaks with plugin X" → composition conflict; reproduce with only X + yours,
  then choose postfix-vs-prefix / priority / a shared-event alternative.

## References

- [references/patch-patterns.md](references/patch-patterns.md) — prefix/postfix/transpiler/finalizer patterns
- [references/publicized-assemblies.md](references/publicized-assemblies.md) — plain vs publicized Assembly-CSharp, access discipline
- [references/troubleshooting.md](references/troubleshooting.md) — failure modes and diagnostics

Upstream truth: Harmony docs (HarmonyLib 2.2.2) and the pinned game assemblies.
A patch is verified against an assembly build, not against memory.
