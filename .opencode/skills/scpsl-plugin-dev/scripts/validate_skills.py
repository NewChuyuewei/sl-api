#!/usr/bin/env python3
"""Validate the SCP:SL Agent Skills suite against the Agent Skills specification.

Usage:
    python3 validate_skills.py [skills_root]

Checks, per skill directory (a directory containing SKILL.md):
  * frontmatter parses and contains required `name` and `description`
  * `name` is 1-64 chars, lowercase letters/numbers/hyphens, no leading/trailing
    or consecutive hyphens, and equals the directory name
  * `description` is 1-1024 chars
  * SKILL.md body stays under 500 lines (progressive-disclosure budget)
  * relative Markdown links in SKILL.md point at files that exist (one level)
  * prompt-strength contract present: a "Mandatory operating contract" section with
    at least one MUST/MUST NOT rule, a skill-specific "Hard gates" block, and a
    "Definition of done" block (suite-wide enforcement standard)
Exits non-zero and prints every error found.
"""

import re
import sys
from pathlib import Path

MAX_NAME = 64
MAX_DESCRIPTION = 1024
MAX_SKILL_LINES = 500
NAME_RE = re.compile(r"^[a-z0-9]+(-[a-z0-9]+)*$")
LINK_RE = re.compile(r"\[[^\]]*\]\(([^)]+)\)")


def parse_frontmatter(text):
    if not text.startswith("---\n"):
        return None, "missing opening '---' frontmatter delimiter"
    end = text.find("\n---", 4)
    if end == -1:
        return None, "missing closing '---' frontmatter delimiter"
    fields = {}
    for line in text[4:end].splitlines():
        # Only top-level keys; nested metadata values are ignored by design.
        match = re.match(r"^([A-Za-z0-9_-]+):\s*(.*)$", line)
        if match:
            fields[match.group(1)] = match.group(2).strip().strip('"').strip("'")
    return fields, None


def validate_skill(skill_dir):
    errors = []
    skill_md = skill_dir / "SKILL.md"
    text = skill_md.read_text(encoding="utf-8")
    fields, err = parse_frontmatter(text)
    if err:
        return [f"{skill_dir.name}: {err}"]
    name = fields.get("name", "")
    description = fields.get("description", "")
    if not name:
        errors.append(f"{skill_dir.name}: frontmatter 'name' is required")
    elif len(name) > MAX_NAME or not NAME_RE.match(name):
        errors.append(f"{skill_dir.name}: invalid skill name {name!r}")
    elif name != skill_dir.name:
        errors.append(f"{skill_dir.name}: name {name!r} must match directory name")
    if not description:
        errors.append(f"{skill_dir.name}: frontmatter 'description' is required")
    elif len(description) > MAX_DESCRIPTION:
        errors.append(
            f"{skill_dir.name}: description is {len(description)} chars (max {MAX_DESCRIPTION})"
        )
    lines = text.count("\n") + 1
    if lines > MAX_SKILL_LINES:
        errors.append(f"{skill_dir.name}: SKILL.md is {lines} lines (max {MAX_SKILL_LINES})")
    for target in LINK_RE.findall(text):
        if target.startswith(("http://", "https://", "#", "mailto:")):
            continue
        target_path = target.split("#", 1)[0]
        if target_path and not (skill_dir / target_path).exists():
            errors.append(f"{skill_dir.name}: broken relative link -> {target}")
    # Prompt-strength contract (engineering standard): enforceable rules only.
    if "Mandatory operating contract" not in text:
        errors.append(f"{skill_dir.name}: missing 'Mandatory operating contract' section")
    if not re.search(r"\bMUST( NOT)?\b", text):
        errors.append(f"{skill_dir.name}: no MUST/MUST NOT rule found (prompt too weak)")
    if "Hard gates" not in text:
        errors.append(f"{skill_dir.name}: missing skill-specific 'Hard gates' block")
    if "Definition of done" not in text:
        errors.append(f"{skill_dir.name}: missing 'Definition of done' block")
    return errors


def main():
    root = Path(sys.argv[1] if len(sys.argv) > 1 else ".").resolve()
    skill_dirs = sorted(p.parent for p in root.rglob("SKILL.md"))
    if not skill_dirs:
        print(f"No skills found under {root}")
        return 1
    all_errors = []
    for skill_dir in skill_dirs:
        errs = validate_skill(skill_dir)
        status = "OK  " if not errs else "FAIL"
        print(f"{status} {skill_dir.relative_to(root)}")
        all_errors.extend(errs)
    for err in all_errors:
        print(f"  ERROR: {err}")
    print(f"\n{len(skill_dirs)} skill(s), {len(all_errors)} error(s)")
    return 1 if all_errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
