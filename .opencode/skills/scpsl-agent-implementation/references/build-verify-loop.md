# Build–Verify Loop

## Commands (from the framework templates)

```bash
# LabAPI project
dotnet build -c Release -p:SL_REFERENCES="<server>/SCPSL_Data/Managed"
# EXILED project
dotnet build -c Release -p:EXILED_REFERENCES="<references folder>"
# Core tests (when a Core project exists)
dotnet test -c Release
```

Record the exact command and the result summary (errors/warnings count) in the return packet.
A build that only succeeds with local absolute paths committed to the .csproj is a defect —
references go through the MSBuild properties.

## Failure triage order

1. Missing game/framework assembly → reference path or Private=false set wrong. Fix the path; never downgrade the framework pin.
2. Unknown type/namespace → unverified API. Go to pinned upstream (examples first), correct or mark UNVERIFIED.
3. Version mismatch (LabAPI/EXILED gate types) → wrong reference version. Align to 1.1.7 / 9.15.3 (mind the EXILED NuGet lag note).
4. Real code errors → fix at first error only, rebuild, repeat. No speculative mass edits on a red build.

## Self-review checklist (attach, checked, to the return packet)

- [ ] Every `+=` I added has its `-=` (or CustomHandlersManager pair); coroutines stored + killed; patches (if any) justified + kill-switched
- [ ] Handlers guard null/state/ownership before acting; `IsAllowed` respected
- [ ] Commands permission-checked; arguments validated; targets eligibility-checked
- [ ] Config additions have safe defaults + docs entries; no invariant renamed
- [ ] No Thread.Sleep/Task.Delay/.Result/background game access; no IO in hot handlers
- [ ] Changed files all inside my ownership list; no game/framework DLLs added to output
- [ ] scan_plugin.py run (or unavailable — stated); leads triaged: <count>
