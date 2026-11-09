# Delegation Protocol

## Packet completeness check (commander self-audit before sending)

- [ ] Objective in one sentence with observable outcome
- [ ] Framework + pins (LabAPI 1.1.7 / EXILED 9.15.3 / dual) and risk tier (T1–T4)
- [ ] Inputs linked: spec, ADR, files, prior findings — not "see conversation"
- [ ] File ownership: exact directories/files this agent may edit; everyone else's are read-only to it
- [ ] Skills to load, by exact ID (e.g. `scpsl-labapi`)
- [ ] Hard constraints repeated (lifecycle symmetry, permission gates, no unverified APIs)
- [ ] Acceptance criteria copied from the plan, not paraphrased
- [ ] Return evidence specified (build result, scenario IDs, report path)

## Parallelism rules

- Same-file or same-assembly work is sequential, always.
- Read-only agents (reviewer, security-auditor, planner) parallelize freely with anything.
- Two coders parallelize only on disjoint assemblies (e.g. LabApi adapter vs EXILED adapter,
  or Core tests vs docs) with a frozen shared contract (Core API) decided beforehand.
- If two agents' outputs disagree, both are frozen until the commander resolves against
  spec > priority hierarchy > user. Majority vote is not a resolution mechanism.

## Anti-patterns

- **Telephone game**: coder summarizing the spec to the tester from memory. Packets carry the spec itself.
- **Shadow work**: commander editing code "while waiting" in files a coder owns.
- **Rubber-stamp integration**: pasting subagent output without checking its evidence fields exist.
- **Depth abuse**: asking a subagent to delegate onward (depth limits make this unreliable); sequence from the top.
