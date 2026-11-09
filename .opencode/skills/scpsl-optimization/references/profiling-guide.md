# Profiling Guide — Finding the Real Cost

## Baselines to capture (before and after, same scenario)

- Server size and round phase when lag occurs (lobby vs mid-round vs warhead).
- Server-reported performance (console performance lines / panel graphs) with plugin on vs off.
- Log file size per round and exception/warning counts (`analyze_server_log.py`).
- Player-visible symptoms: rubber-banding for all (main thread) vs one player (network/local).

## Isolation method

1. Your plugin alone on the test server: reproduce or clear suspicion.
2. Add the production plugin set back in halves (binary search) for interaction costs —
   event-veto cascades and Harmony composition only appear in company.
3. Inside your plugin: feature-flag subsystems off one at a time (this is why
   `scpsl-architecture` mandates per-subsystem toggles). The flag that removes the lag
   names the subsystem; profile inside it next.

## Timing harness pattern (development only)

```csharp
// Aggregate, don't log per call: counters + total ticks, dumped at round end under Debug.
private long _hurtCalls, _hurtTicks;
private void OnHurting(/* args */)
{
    var start = System.Diagnostics.Stopwatch.GetTimestamp();
    try { /* handler body */ }
    finally
    {
        _hurtCalls++;
        _hurtTicks += System.Diagnostics.Stopwatch.GetTimestamp() - start;
    }
}
```

Remove or keep behind Debug after the investigation; a permanent Stopwatch in production
hot paths needs its own justification. Never profile with per-call `Log.Info` — the logging
becomes the cost and the conclusion is garbage.

## Reading results

- Cost concentrated in one handler → optimize that handler (hot-path patterns reference).
- Cost spread evenly → look at loops and per-player recurring work, not events.
- Cost only with many players → O(players) work hidden in an O(1)-looking event; count
  iterations, not just handler time.
- Memory growth across rounds → state cleanup failure; diff dictionary counts at round end.
