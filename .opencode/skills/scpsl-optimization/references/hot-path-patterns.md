# Hot-Path Patterns — Before / After

## Guard order (every hot handler)

```csharp
// Before: work first, questions later
var targets = Player.List.Where(p => p.IsAlive && ...).ToList();   // allocates, scans all
foreach (var p in targets) { ... }

// After: cheapest rejections first, zero allocation on the common path
if (!Config.Enabled || ev.Player is null) return;
if (!_service.TryGetHolder(ev.Player.UserId, out var state)) return;  // only affected players pay
```

## Loops

```csharp
// Before: frame-cadence full scan
yield return Timing.WaitForSeconds(0.1f);  // 10x/sec over every player, forever

// After: feature cadence over tracked participants only
yield return Timing.WaitForSeconds(2f);
foreach (var userId in _activeUsersSnapshot) { /* O(active), not O(server) */ }
```

## Lookups and caching

- Cache per-round immutable-ish data (room/door lists, spawn points) at round start;
  invalidate on round end and map regeneration. Key player lookups by UserID via the
  framework's own dictionaries instead of scanning `Player.List` per event.
- Cache `AccessTools`/reflection members in `static readonly` fields (Harmony code);
  reflection per call in a patched hot method is a profiling finding by itself.

## Presentation fan-out

Hints/broadcasts/CASSIE/DisplayKit: address affected players, not `Player.List`, unless the
announcement is genuinely global; coalesce repeat sends (a hint refreshed per second per
player is a config decision, not an accident); clear per-player UI state on leave/disable
so the server stops updating ghosts.

## IO batching

Accumulate stats/events in memory; flush on round end or a 30-60s MEC timer; write files
atomically. A `File.WriteAllText` inside a damage event is a release blocker
(`scpsl-code-review` treats it as such).
