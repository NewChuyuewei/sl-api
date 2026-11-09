# Test Strategy — What to Test Where

## The layering rule

| Behaviour kind | Layer | Why |
|---|---|---|
| Rules, maths, chance, cooldowns, parsing, clamping | Unit (Core) | Deterministic, fast, catches regressions before a server exists |
| Event wiring, wrapper calls, presentation, commands | Server scenario | Only real in the game; keep steps scripted and repeatable |
| Lifecycle (reload/round/disconnect) | Server scenario, always manual-checklist | The highest-defect area; automate log assertions where possible |
| Performance under population | Server scenario + optimization skill | Needs realistic player counts and the real plugin set |
| Harmony patch behaviour | Server scenario on the exact game build | Patch validity is per-assembly; there is no meaningful off-server test |

## Writing scenarios an agent can execute

Each scenario: **Setup** (server state, config values, plugins) → **Steps** (numbered,
in-game actions or commands verbatim) → **Expected** (observable: hint text, log line, state
after) → **Evidence** (log excerpt/screenshot placeholder). Avoid "verify it works" —
name the observation.

## Coverage bars before M2 sign-off

- 100% of spec behaviour rules have at least one scenario.
- 100% of commands have allowed + denied + bad-args rows.
- 100% of config keys appear in the config matrix (even if "default only").
- Lifecycle family (boot, disable/enable, round restart) is all-pass with zero exceptions
  in the log. Exceptions in *unrelated* base-game log noise must be identified as such,
  not waved away.

## Bots and population

Dummy/NPC players and bot plugins help fill servers but do not replicate human timing,
voice, or client behaviour. Mark scenarios as bot-verified vs human-verified; release
sign-off for player-facing features needs at least one human pass.
