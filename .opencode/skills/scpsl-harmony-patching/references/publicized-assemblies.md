# Plain vs Publicized Game Assemblies

## The two flavours

- **Plain `Assembly-CSharp.dll`** — what ships in `SCPSL_Data/Managed/`. Non-public game members
  are inaccessible at compile time. LabAPI development normally references this plus the LabAPI
  wrappers; EXILED plugins execute against the game with EXILED's runtime handling.
- **`Assembly-CSharp-Publicized.dll`** — a build with member accessibility loosened
  (publicized), used for EXILED development so plugins can compile against game internals.
  Referencing it changes what *compiles*, not what exists at runtime.

## Discipline

1. Compile-time flavour must match the framework skill's template (LabAPI template: plain;
   EXILED template: publicized). Mixing them produces plugins that compile and then fail
   with missing-member errors on real servers.
2. Every direct use of a non-public game member (whether via publicized reference or
   `AccessTools` reflection) is a fragility point: collect them in one `GameAccess`-style
   file, comment the game version verified (14.2.x), and fail gracefully if a lookup returns null.
3. Wrappers first: if `LabApi.Features.Wrappers` or `Exiled.API.Features` exposes the member,
   the direct access is deleted, not kept "for convenience".
4. Never redistribute game assemblies (plain or publicized), LabAPI, or EXILED assemblies with
   a plugin release. Reference-only (`Private=false`) everywhere; release zips contain the
   plugin DLL and genuine third-party dependencies only.
5. AccessTools lookups (`AccessTools.Method/Field/Property(typeof(T), "name")`) are cached in
   static readonly fields — reflection lookup per call in a hot patch is a performance bug.
