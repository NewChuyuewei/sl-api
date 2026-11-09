#!/usr/bin/env python3
"""Sync canonical SCP:SL skills into OpenCode's project skill directory.

The suite is authored once in ./skills (portable, Claude Agent Skills format)
and consumed by OpenCode from ./.opencode/skills. This script mirrors the
canonical tree into the OpenCode tree so the two never drift by hand-editing.

Usage (from the workspace root):
    python3 skills/scpsl-agent-orchestration/scripts/sync_opencode_skills.py
    python3 .../sync_opencode_skills.py --source ./skills --dest ./.opencode/skills --dry-run

Copies/updates files that differ, reports files present only in the destination
(possible hand edits) without deleting them. Exit 1 on --check if drift exists.
"""

import argparse
import filecmp
import shutil
import sys
from pathlib import Path

SKIP = {"__pycache__", ".git"}


def iter_files(root):
    for path in sorted(root.rglob("*")):
        if any(part in SKIP for part in path.parts):
            continue
        # The suite-level README in the source root is documentation, not a skill.
        if path.parent == root and path.name == "README.md":
            continue
        if path.is_file():
            yield path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", default="./skills")
    parser.add_argument("--dest", default="./.opencode/skills")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--check", action="store_true", help="Exit 1 if any drift (CI use)")
    args = parser.parse_args()

    source, dest = Path(args.source), Path(args.dest)
    if not source.is_dir():
        print(f"Source not found: {source}")
        return 2

    copied, same, dest_only = 0, 0, []
    for src in iter_files(source):
        rel = src.relative_to(source)
        target = dest / rel
        if target.is_file() and filecmp.cmp(src, target, shallow=False):
            same += 1
            continue
        copied += 1
        print(f"{'WOULD COPY' if args.dry_run else 'COPY'} {rel}")
        if not args.dry_run:
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, target)

    if dest.is_dir():
        src_rels = {p.relative_to(source) for p in iter_files(source)}
        for existing in iter_files(dest):
            if existing.relative_to(dest) not in src_rels:
                dest_only.append(str(existing.relative_to(dest)))

    print(f"\n{copied} copied/updated, {same} identical, {len(dest_only)} destination-only")
    for path in dest_only:
        print(f"  DEST-ONLY (hand edit? not deleted): {path}")
    if args.check and (copied or dest_only):
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
