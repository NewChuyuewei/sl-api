#!/usr/bin/env python3
"""Scaffold a new SCP:SL plugin project from this suite's bundled templates.

Usage:
    python3 new_plugin.py --api labapi --name MyPlugin --output ./MyPlugin [--author "Me"]
    python3 new_plugin.py --api exiled --name MyPlugin --output ./MyPlugin

The script locates the matching template under the sibling framework skill
(`scpsl-labapi/assets/labapi-plugin` or `scpsl-exiled/assets/exiled-plugin`),
copies it to --output, renames any file whose name contains __PLUGIN_NAME__,
and substitutes the tokens __PLUGIN_NAME__, __AUTHOR__, __YEAR__ in text files.
It never overwrites a non-empty output directory.
"""

import argparse
import re
import shutil
import sys
from datetime import date
from pathlib import Path

TEXT_SUFFIXES = {".cs", ".csproj", ".md", ".yml", ".yaml", ".json", ".gitignore", ".txt"}
API_TEMPLATES = {
    "labapi": ("scpsl-labapi", "labapi-plugin"),
    "exiled": ("scpsl-exiled", "exiled-plugin"),
}


def find_template(api):
    skill_dir, template_name = API_TEMPLATES[api]
    here = Path(__file__).resolve()
    # scripts/ lives in scpsl-plugin-dev; templates live in sibling skills,
    # so search upwards from this file for the skills root.
    for parent in here.parents:
        candidate = parent / skill_dir / "assets" / template_name
        if candidate.is_dir():
            return candidate
        candidate = parent / "skills" / skill_dir / "assets" / template_name
        if candidate.is_dir():
            return candidate
    return None


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--api", choices=sorted(API_TEMPLATES), required=True)
    parser.add_argument("--name", required=True, help="Plugin name, e.g. MyPlugin (PascalCase, no spaces)")
    parser.add_argument("--output", required=True, help="Output directory to create")
    parser.add_argument("--author", default="Unknown author")
    args = parser.parse_args()

    if not args.name.isidentifier() or not args.name[0].isupper():
        parser.error("--name must be a PascalCase C# identifier, e.g. MyPlugin")

    template = find_template(args.api)
    if template is None:
        print(f"Template for '{args.api}' not found next to this suite. "
              "Run from an intact skills/ checkout.", file=sys.stderr)
        return 1

    output = Path(args.output).resolve()
    if output.exists() and any(output.iterdir()):
        print(f"Output directory {output} already exists and is not empty.", file=sys.stderr)
        return 1
    output.mkdir(parents=True, exist_ok=True)

    snake = re.sub(r"(?<!^)(?=[A-Z])", "_", args.name).lower()
    tokens = {
        "__PLUGIN_NAME__": args.name,
        "__plugin_name__": snake,
        "__AUTHOR__": args.author,
        "__YEAR__": str(date.today().year),
    }
    for src in sorted(template.rglob("*")):
        rel = Path(*[part.replace("__PLUGIN_NAME__", args.name) for part in src.relative_to(template).parts])
        dest = output / rel
        if src.is_dir():
            dest.mkdir(parents=True, exist_ok=True)
            continue
        dest.parent.mkdir(parents=True, exist_ok=True)
        if src.suffix in TEXT_SUFFIXES or src.name in {".gitignore"}:
            content = src.read_text(encoding="utf-8")
            for token, value in tokens.items():
                content = content.replace(token, value)
            dest.write_text(content, encoding="utf-8")
        else:
            shutil.copy2(src, dest)

    print(f"Created {args.api} plugin '{args.name}' in {output}")
    print("Next steps:")
    print("  1. Point the game-assembly references at your SCP:SL dedicated server "
          "(SCPSL_Data/Managed) via the SL_REFERENCES / EXILED_REFERENCES property or your IDE.")
    print("  2. dotnet build -c Release   (targets net48; .NET SDK 8+ required)")
    print("  3. Deploy the built .dll using the scpsl-migration-release skill and read the server log.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
