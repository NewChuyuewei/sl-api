#!/usr/bin/env python3
"""Heuristic first-pass scanner for SCP:SL plugin source (LabAPI / EXILED).

Usage:
    python3 scan_plugin.py <plugin-source-dir>

Flags common defect patterns so a human/agent reviewer can confirm them in
context. Output is leads, not verdicts: every hit needs an eyes-on check.
Exit code is always 0 unless the path is unusable.
"""

import re
import sys
from pathlib import Path

RULES = [
    ("BLOCKING_THREAD", re.compile(r"Thread\.Sleep|\.Wait\(\)|\.Result\b|Task\.Delay"),
     "Blocking wait on a possible main-thread path. Game delays must use MEC Timing."),
    ("ASYNC_VOID_HANDLER", re.compile(r"\basync\s+void\b"),
     "async void: exceptions vanish and ordering is lost. Use MEC coroutines."),
    ("UNPATCH_ALL_NO_ID", re.compile(r"UnpatchAll\(\s*\)"),
     "UnpatchAll() without the plugin's own Harmony ID removes other plugins' patches."),
    ("HARMONY_NO_FAILOPEN", re.compile(r"\[HarmonyPatch"),
     "Harmony patch found: verify written justification, fail-open handling, own-ID unpatch."),
    ("STATIC_PLAYER_STATE", re.compile(r"static\s+.*(Dictionary|List|HashSet)<[^>]*Player"),
     "Static player-keyed state: key by UserID and clean up on leave/round end/disable."),
    ("FILE_IO", re.compile(r"File\.(WriteAll|AppendAll|ReadAll)|new\s+StreamWriter"),
     "File IO: keep out of hot event handlers; write atomically and off the hot path."),
    ("HARDCODED_ID", re.compile(r"\b\d{17}@(steam|discord)|\b7656119\d{10}\b"),
     "Possible hard-coded user ID. Staff/permission data belongs in config/permissions."),
    ("TODO_MARKER", re.compile(r"//\s*(TODO|FIXME|HACK|XXX)\b"),
     "Leftover TODO/FIXME/HACK marker."),
]

SUBSCRIBE_RE = re.compile(r"(\w+(?:\.\w+)*)\s*\+=\s*(\w+)")
UNSUBSCRIBE_RE = re.compile(r"(\w+(?:\.\w+)*)\s*-=\s*(\w+)")


def main():
    if len(sys.argv) != 2:
        print(__doc__)
        return 2
    root = Path(sys.argv[1])
    if not root.is_dir():
        print(f"Not a directory: {root}")
        return 2

    subscribed, unsubscribed = {}, {}
    hits = []
    for cs in sorted(root.rglob("*.cs")):
        text = cs.read_text(encoding="utf-8", errors="replace")
        rel = cs.relative_to(root)
        for lineno, line in enumerate(text.splitlines(), 1):
            for rule_id, pattern, advice in RULES:
                if pattern.search(line):
                    hits.append(f"{rel}:{lineno} [{rule_id}] {line.strip()[:90]}\n    -> {advice}")
            for match in SUBSCRIBE_RE.finditer(line):
                subscribed.setdefault((match.group(1), match.group(2)), f"{rel}:{lineno}")
            for match in UNSUBSCRIBE_RE.finditer(line):
                unsubscribed.setdefault((match.group(1), match.group(2)), f"{rel}:{lineno}")

    for key, where in sorted(subscribed.items()):
        if key not in unsubscribed:
            hits.append(
                f"{where} [UNPAIRED_SUBSCRIBE] {key[0]} += {key[1]} has no matching -= anywhere in the project\n"
                "    -> Likely reload-duplication bug; verify (CustomEventsHandler users are exempt)."
            )
    for key, where in sorted(unsubscribed.items()):
        if key not in subscribed:
            hits.append(
                f"{where} [UNPAIRED_UNSUBSCRIBE] {key[0]} -= {key[1]} has no matching += anywhere in the project\n"
                "    -> Dead unsubscribe or a subscribe written differently; verify."
            )

    print(f"Scanned {root} — {len(hits)} lead(s):\n")
    for hit in hits:
        print(hit)
    if not hits:
        print("No heuristic leads. This is not a clean bill of health — do the manual review pass.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
