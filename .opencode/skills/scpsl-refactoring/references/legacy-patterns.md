# Legacy Patterns — Recognition and Replacement

| Legacy pattern (seen in old plugins/tutorials) | Why it fails now | Replacement on the pinned stack |
|---|---|---|
| NwPluginAPI namespaces / plugin base | Loader archived; LabAPI replaced it | LabAPI 1.1.7 `Plugin`/`Plugin<TConfig>` + wiki layout |
| LabAPI 0.x event/handler shapes | Renamed/restructured before 1.0 | `CustomEventsHandler` overrides; args in `LabApi.Events.Arguments.*` |
| EXILED 8-era args/namespaces, old CustomItems API | 9.x moved args (some LabAPI-backed) and rebuilt custom-content APIs | 9.15.3 `Exiled.Events.EventArgs.*`, `Exiled.CustomItems/Roles.API.Features` per pinned docs |
| `Task.Delay` / timers / `Thread.Sleep` for delays | Off-thread or blocking on the game loop | MEC `Timing.CallDelayed` / coroutines with owned handles |
| Harmony patch for anything (first resort) | Breaks per game build; conflicts silently | Event/wrapper first; patch only through the scpsl-harmony-patching gate |
| Plain `Assembly-CSharp` assumptions in EXILED code, publicized refs in LabAPI code | Compile/run mismatch, missing members | Framework-correct reference set per template (plain vs publicized) |
| Config via manual file parsing in plugin folder | Reinvents loader config; breaks per-port layout | LabAPI `Plugin<TConfig>`/advanced config; EXILED `IConfig` sections |
| Player dictionaries keyed by `Player`/`ReferenceHub` | Leaks + stale wrappers after disconnect | UserID/serial keys + cleanup matrix (scpsl-architecture) |
| Giant `Plugin.cs` with all handlers/logic | Untestable, reload-fragile | Thin entry point + handlers + services (+ Core when justified) |
| Copy-pasted snippets from old forum/Discord posts | Target dead API versions | Pinned upstream examples: `LabApi.Examples`, `Exiled.Example` |

Recognition tip: any snippet whose usings, base class, or folder structure does not match the
pinned templates in `scpsl-labapi`/`scpsl-exiled` assets is legacy until proven current.
