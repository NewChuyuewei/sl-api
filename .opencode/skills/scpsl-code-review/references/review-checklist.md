# Review Checklist (pass / fail / n-a per line)

## Lifecycle
- [ ] Every `+=` has a matching `-=` (same handler class, event, method) — lists diffed mechanically
- [ ] LabAPI `RegisterEventsHandler` paired with `UnregisterEventsHandler` on the same instance
- [ ] All MEC handles stored and killed in disable; loops also stop on round end where needed
- [ ] Harmony unpatched with the plugin's own ID; instance nulled
- [ ] Custom roles/items unregistered; NPCs despawned; singletons and statics cleared
- [ ] Enable is idempotent-safe after a disable (no double registration possible)

## Events and wrappers
- [ ] Each handler uses the correct event moment and respects incoming `IsAllowed`
- [ ] Null guards precede every `ev.Player` / item / door use; NPC and spectator cases handled
- [ ] Wrappers preferred; any `ReferenceHub`/game-type use is isolated, null-checked, justified
- [ ] No handler assumes round state without checking it

## Commands, config, permissions
- [ ] Every privileged command checks a permission node before acting; denial response is clear
- [ ] Arguments validated (count, parse, ranges, target exists and is eligible)
- [ ] Config: safe defaults, properties-only (LabAPI) / `IConfig`+`[Description]` (EXILED), broken YAML cannot crash enable
- [ ] No hard-coded UserIDs, tokens, absolute paths, or magic balance numbers

## Performance and safety
- [ ] No Thread.Sleep/Task.Delay/.Result/background game access; no blocking IO in handlers
- [ ] Hot handlers allocation-light; lookups cached; logging lazy/gated behind Debug
- [ ] State keyed by UserID/serial with round-end + leave cleanup; bounded growth
- [ ] Exceptions cannot escape into the game loop; unexpected failures log once with context
