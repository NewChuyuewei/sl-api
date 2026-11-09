#!/usr/bin/env python3
"""Generate a Markdown test matrix for an SCP:SL plugin QA pass.

Usage:
    python3 generate_test_matrix.py "Feature A" "Feature B" ...
    python3 generate_test_matrix.py --commands hello,spawnitem --features "Custom role"

Prints the matrix to stdout; redirect it into the test plan. Every feature gets
happy-path + boundary rows; lifecycle rows are always included because reload
and round-restart defects are the most common SCP:SL plugin failures.
"""

import argparse

LIFECYCLE = [
    ("Lifecycle", "Cold boot: clean log, enabled line, config generated"),
    ("Lifecycle", "Disable mid-round: behaviour stops immediately"),
    ("Lifecycle", "Re-enable: behaviour fires exactly once per trigger"),
    ("Lifecycle", "Round restart: no state survives into the next round"),
    ("Lifecycle", "5+ rounds longevity: no growth in state or log spam"),
    ("Player", "Disconnect during pending delayed action: no exception"),
    ("Player", "Spectator/NPC does not trigger player-only logic"),
    ("Config", "Defaults boot"),
    ("Config", "Broken YAML: defaults + warning, plugin still enables"),
]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("features", nargs="*", help="Feature names (free text)")
    parser.add_argument("--commands", default="", help="Comma-separated command names")
    args = parser.parse_args()

    rows = list(LIFECYCLE)
    for feature in args.features:
        rows.append(("Feature", f"{feature}: happy path"))
        rows.append(("Feature", f"{feature}: boundary / denied path"))
    for command in (c.strip() for c in args.commands.split(",")):
        if command:
            rows.append(("Command", f"{command}: allowed user succeeds"))
            rows.append(("Command", f"{command}: denied user, bad args, bad target"))

    print("| ID | Area | Scenario | LabAPI 1.1.7 | EXILED 9.15.3 | Evidence/Bug |")
    print("|---|---|---|---|---|---|")
    for i, (area, scenario) in enumerate(rows, 1):
        print(f"| T{i:02d} | {area} | {scenario} |  |  |  |")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
