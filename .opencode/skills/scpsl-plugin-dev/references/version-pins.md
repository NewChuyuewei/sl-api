# Version Pins — Latest Stable Only

Pinned on 2026-11-07 against upstream releases. If an agent has network access, re-verify
before a release; if upstream has moved, follow the bump checklist instead of silently mixing versions.

## Pins

| Component | Version | Source of truth |
|---|---|---|
| SCP: Secret Laboratory (dedicated server) | 14.2.x (14.2.8) | Steam dedicated server build; `SCPSL_Data/Managed/` supplies reference assemblies |
| LabAPI | **1.1.7** (Latest) | https://github.com/northwood-studios/LabAPI/releases · NuGet `Northwood.LabAPI` 1.1.7 |
| EXILED | **9.15.3** | https://github.com/ExMod-Team/EXILED/releases (tag `v9.15.3`) |
| EXILED NuGet | 9.15.2 at pin time | NuGet `ExMod.Exiled` — publishing can lag the GitHub release by one patch. Check NuGet before setting `PackageReference`; if 9.15.3 is absent, reference the 9.15.3 release DLLs directly instead of downgrading the plugin's target. |
| Target framework | `net48` | Both `LabApi.csproj` and EXILED `EXILED.props` target .NET Framework 4.8 |
| Build SDK | .NET SDK 8 or newer (10 recommended) | LabAPI wiki "Creating the Project": create an SDK-style project, then set `<TargetFramework>net48</TargetFramework>` and `<LangVersion>latest</LangVersion>` |
| Harmony | 2.2.2 | NuGet `Lib.Harmony`; EXILED `EXILED.props` `HarmonyVersion` |
| YamlDotNet | ≥ 11.0.1 (LabAPI dependency) | Comes with LabAPI; do not bundle a conflicting copy in a plugin |

## What "latest stable" excludes

- EXILED 10 / dev branches, LabAPI pre-releases, SCP:SL public beta branches.
- Old lines still found in tutorials: NwPluginAPI (archived, replaced by LabAPI),
  EXILED 8.x and earlier, LabAPI 0.x. Their namespaces and lifecycles differ — treat snippets
  using them as historical, not copyable.

## Bump checklist (when a new stable ships)

1. Update the table above and `scpsl-plugin-dev/SKILL.md` (frontmatter `metadata` + pin table).
2. Update the frontmatter `metadata`/`compatibility` and pin tables in all five sibling skills.
3. Update `PackageReference` versions in every `assets/**/*.csproj` template.
4. Diff the upstream examples (`LabApi.Examples`, `Exiled.Example`) and release notes for
   breaking changes; update event catalogues and templates accordingly.
5. Run `scripts/validate_skills.py` and build both scaffolded templates in Release.
6. Bump each edited skill's `metadata.version` (semver: breaking API change → major).
