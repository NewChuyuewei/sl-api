# Harmony 2.2.2 — Patch Patterns for SCP:SL

Choose in this order: **postfix → prefix → finalizer → transpiler.**

## Postfix (default): observe or adjust

Runs after the original. Use to adjust results, fire your own logic, or record state.
Cannot prevent the original — if you need prevention, you needed the cancellable event first.

```csharp
private static void Postfix(TargetGameType __instance, ref int __result)
{
    // Read __result / __instance state; adjust minimally.
}
```

Special parameters (by exact name): `__instance` (target object; omitted for static methods),
`__result` (`ref`, non-void methods), `___fieldName` (private field access — brittle, isolate it),
named method arguments (original parameter names/types), `__state` (prefix→postfix handoff).

## Prefix: gate or replace (claiming behaviour)

Return `false` to skip the original (and later prefixes). Skipping is a public claim on that
behaviour: every other plugin and the base game lose the method. Set `__result` when skipping
a non-void method. Keep the skip path complete — half-simulated vanilla logic rots fastest.

## Finalizer: exception policy

Wraps the whole call; return `null` to suppress an exception or return a replacement exception.
Use sparingly: suppressing game exceptions hides bugs. Acceptable use: containing *your own*
patch logic's failures in methods where a postfix alone cannot (document why).

## Transpiler: IL rewriting (special justification required)

```csharp
private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    // Match a minimal, anchored pattern; yield everything else untouched.
}
```

- Anchor on the most stable IL landmarks (specific method calls/fields), never on instruction
  counts or local indices alone.
- If the anchor is missing, yield the original sequence unchanged and log an error (fail open),
  rather than throwing inside patch application.
- Expect to re-verify transpilers on **every** game patch release. No exceptions.

## Patch registration patterns

- Assembly scanning: `[HarmonyPatch]` classes + `harmony.PatchAll(assembly)` — simple, but
  patches everything attributed; keep patch classes in a dedicated `Patches/` folder so the
  set is auditable at a glance.
- Manual: `harmony.Patch(AccessTools.Method(...), prefix: new HarmonyMethod(...), …)` —
  use when a patch must be conditional (config flag, game-version check) or ordered explicitly.
- One class per target method, named `<Type><Method>Patch`, with the justification comment
  from the skill gate on top. Patch classes are `internal static`; logic delegates to services.
