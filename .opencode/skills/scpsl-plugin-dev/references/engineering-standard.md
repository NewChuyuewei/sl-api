# Engineering Standard — Google/Cloudflare-Grade Bar for SCP:SL Plugins

This suite holds SCP:SL plugin work to the engineering bar used by large-scale production
teams (Google's code-health/readability culture; Cloudflare's reliability/performance/
security-at-scale culture), translated to a Unity game-server context. These are not
aspirations — they are acceptance criteria. Work that misses them is not done.

## The priority hierarchy (arbitrates every conflict)

When two good practices collide, the higher one wins. Never trade downward silently.

| Rank | Principle | In SCP:SL terms |
|---|---|---|
| P0 | **Do not destabilize the server** | No crash, freeze, or round corruption. Fail open to vanilla. |
| P1 | **Lifecycle & data integrity** | Symmetric cleanup, no state bleed, no lost/corrupted owner data. |
| P2 | **Security** | Permission gates, validated input, minimal player-data exposure. |
| P3 | **Correctness** | The spec's behaviour rules hold, including edge and denial paths. |
| P4 | **Measurable performance** | Budgets met at full population, proven by measurement. |
| P5 | **Maintainability** | Readable, testable, portable between LabAPI/EXILED where required. |
| P6 | **Style** | Formatting and naming. Never a reason to block P0–P5 work. |

Example arbitration: a "faster" handler that drops a permission check violates P2 for P4.
Rejected at any rank, automatically, without discussion.

## The six production disciplines (mapped)

1. **Code health over cleverness (Google).** Readable beats compact. Every change must leave
   the touched code easier to review than it was found. Banned: unexplained magic numbers,
   handler classes doing five jobs, commented-out code, TODO without owner/condition.
2. **Small, complete changes (Google).** One concern per change: feature, refactor, and
   version-bump never mix (see `scpsl-refactoring` invariants). A change is complete only
   with its config, docs, tests, and cleanup — a handler without its unsubscribe is not 90%
   done, it is broken.
3. **Testing is part of the change, not a phase (Google).** Core logic ships with unit tests
   in the same change. Server-facing behaviour ships with named scenarios
   (`scpsl-testing-qa`). "Tested" without build hash + scenario IDs is an unverified claim.
4. **Reliability is a feature (Cloudflare/SRE).** Every subsystem has a kill switch (config),
   a failure mode (fail open/degrade), and an error budget mindset: exceptions in your
   handlers are defects counted against the plugin, not "server noise". Clean logs are a
   release criterion.
5. **Performance is measured or it didn't happen (Cloudflare).** Budgets (quality-gates.md)
   are checked at realistic population. No optimization without before/after numbers;
   no numbers at 3 test players claimed as full-server proof.
6. **Security & privacy by default (Cloudflare).** Deny-first permissions, validated input,
   least player data, no concealment from admins, no silent network calls. Security findings
   outrank feature completeness at release.

## Evidence standard (applies to every claim an agent makes)

| Claim | Required evidence |
|---|---|
| "Uses the correct API" | Pinned upstream source/docs named (file, example, or release), or the claim is labelled UNVERIFIED |
| "It builds" | Build command + configuration + result, actually executed |
| "It works" | Scenario ID(s) from the test plan, framework + build hash, actually executed |
| "It's faster" | Before/after measurement, same scenario, player count stated |
| "It's reviewed" | Findings report; scanner output alone is explicitly not a review |
| "It's ready to release" | Quality gates G0–G5 (quality-gates.md) each marked pass with evidence |

An agent that cannot produce the evidence downgrades the claim ("implemented, not yet
server-tested") — it never upgrades the language to compensate.

## Review culture rules

- **Comments resolve by fix or by filed task**, never by argument from authority.
- **Blockers are objective**: each maps to P0–P2 or a named gate. "I would write it
  differently" is a Nit and cannot block.
- **The author owns the whole change**: docs, changelog, config migration notes, and test
  evidence land together. Reviewers verify the set, not just the .cs files.
- **Blameless, specific postmortems for shipped incidents**: timeline, first log line,
  root cause class (lifecycle/state/API-drift/conflict), and the new regression scenario
  the incident buys. The scenario is mandatory; the blame is forbidden.

## What "done" means globally

Global Definition of Done = skill-specific Definition of Done + quality gates for the
change's risk tier (quality-gates.md). If any applicable gate is unchecked, the work is
reported as in-progress with the open gates named. Shipping vocabulary is binary:
**verified** (evidence exists) or **unverified** (it does not). There is no third state.
