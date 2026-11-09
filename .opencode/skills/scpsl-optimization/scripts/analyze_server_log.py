#!/usr/bin/env python3
"""Summarize an SCP:SL server log for performance/quality triage.

Usage:
    python3 analyze_server_log.py <server-log.txt> [--top N]

Reports: exception count and first exception line, repeated-line hot spots
(log spam usually means a per-event log or a failing loop), WARN/ERROR counts,
and plugin enable lines. Heuristic triage for scpsl-optimization — confirm
findings in the full log before optimizing anything.
"""

import argparse
import re
from collections import Counter
from pathlib import Path

EXCEPTION_RE = re.compile(r"(Exception|NullReference|IndexOutOfRange|KeyNotFound)")
TIMESTAMP_RE = re.compile(r"\[?(\d{2,4}[-/]\d{1,2}[-/]\d{1,2}|\d{1,2}:\d{2}:\d{2})[^\]]*\]?\s*")
LEVEL_RE = re.compile(r"^\s*\[?(INFO|WARN|WARNING|ERROR|DEBUG)\]?\b", re.IGNORECASE)


def normalize(line):
    """Strip timestamps/numbers so repeated spam collapses into one bucket."""
    line = TIMESTAMP_RE.sub("", line).strip()
    line = re.sub(r"\d+", "#", line)
    return line[:140]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("logfile")
    parser.add_argument("--top", type=int, default=10)
    args = parser.parse_args()

    path = Path(args.logfile)
    if not path.is_file():
        print(f"Not a file: {path}")
        return 2
    lines = path.read_text(encoding="utf-8", errors="replace").splitlines()

    levels = Counter()
    repeated = Counter()
    exceptions = 0
    first_exception = None
    enabled = []
    for raw in lines:
        match = LEVEL_RE.match(raw)
        if match:
            levels[match.group(1).upper()] += 1
        if EXCEPTION_RE.search(raw):
            exceptions += 1
            if first_exception is None:
                first_exception = raw.strip()[:200]
        if "has been enabled" in raw or "enabled!" in raw:
            enabled.append(raw.strip()[:140])
        repeated[normalize(raw)] += 1

    print(f"Log: {path} ({len(lines)} lines)")
    print(f"Levels: {dict(levels)}")
    print(f"Exception-mentioning lines: {exceptions}")
    if first_exception:
        print(f"First exception line: {first_exception}")
    print(f"\nPlugin enable lines ({len(enabled)}):")
    for line in enabled[:20]:
        print(f"  {line}")
    print(f"\nTop repeated lines (spam candidates):")
    for line, count in repeated.most_common(args.top):
        if count > 1:
            print(f"  x{count:<5} {line}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
