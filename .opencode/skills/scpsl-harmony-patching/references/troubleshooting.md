# Harmony Troubleshooting Playbook

## Patch did not apply

Symptoms: feature inert, no exceptions, enable log may show patch failures.
Check: exact method signature in the **current** game assembly (name, parameter types/order,
generic arity, static vs instance); target type in the right assembly (game vs Mirror/Unity);
`PatchAll` actually ran (assembly passed is the plugin's, patch class is attributed and
non-generic); another loader didn't already fail enabling your plugin earlier (read the log
from the first error, not the last).

## Patch applied but never fires

The method may be inlined, virtual-dispatched to an override, or simply not the code path the
game takes on a dedicated server (client-only paths never run). Verify with a debug log inside
the patch during a local test round; if the path is client-only, the feature needs re-scoping
(server-side equivalent or nothing), not a different patch on the same dead method.

## Exceptions after a game update

Signature/IL drift. Re-open the new `Assembly-CSharp` (decompiler), diff the target method,
update accessors/transpiler anchors, retest enable → round → disable, bump the plugin version
and its verified-game-version note. Never "fix" update breakage by widening catch blocks to
swallow everything silently — fail-open logging is what tells owners the feature is off.

## Double behaviour / conflicts with another plugin

Both patches fire; ordering is load/registration order unless Harmony priority attributes say
otherwise. Reproduce minimally (your plugin + the other). Prefer converting your side to a
postfix that composes, or gate on shared state. Record known interactions in the README.

## Reload duplicates

Behaviour happens twice after `reload` / plugin re-enable: the old Harmony instance was not
unpatched (wrong ID to `UnpatchAll`, instance leaked in a static field, or `PatchAll` ran
twice). Unpatch with the plugin's own ID in disable, null the field, and make enable idempotent.

## Performance complaints

Profile before optimizing: log call counts at debug level off-hot-path. Usual culprits:
reflection lookups per call, LINQ/allocations in per-frame methods, string building for logging
that is disabled anyway. Cache, precompute, and early-out on the cheapest checks first
(is the feature enabled? is this the relevant player/item?).
