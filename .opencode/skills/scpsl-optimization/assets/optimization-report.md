# Optimization Report — <Plugin Name>

## Symptom and reproduction
- Complaint: <lag / CPU / memory / log size> during <round phase> at <player count>
- Scenario used for all measurements: <server, plugin set, steps>
- Isolation result: <plugin alone / subsystem flag that removes it>

## Baseline (before)
| Metric | Value | Source |
|---|---|---|
| Server performance indicator |  | console/panel |
| Log size per round |  | analyze_server_log.py |
| Exceptions per round |  | analyze_server_log.py |
| Suspect handler/loop cost |  | timing harness |

## Changes (one at a time, measured)
| # | Change | File | Delta measured | Kept? |
|---|---|---|---|---|
| 1 |  |  |  |  |

## After
| Metric | Before | After |
|---|---|---|
|  |  |  |

## Budget locked in
<e.g. "Hurting handler <0.05 ms avg at 30 players; loop iterates tracked holders only">

## Regression check
- [ ] scpsl-testing-qa lifecycle + feature scenarios re-run green on build <hash>
