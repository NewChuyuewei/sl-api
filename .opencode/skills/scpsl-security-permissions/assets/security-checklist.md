# Security Checklist — <Plugin Name>

Run before M2 sign-off and at every release. Attach to the review report.

## Permissions
- [ ] Every privileged command/path lists its node here: <nodes>
- [ ] Checks happen before side effects; denial changes nothing and says why
- [ ] No wildcard checks, no RA-access-only gates, no hard-coded staff IDs
- [ ] README permission table published (node / allows / suggested default)

## Input handling
- [ ] All command arguments validated (count, TryParse, ranges, caps)
- [ ] Targets resolved with existence + eligibility checks; ambiguity rejected
- [ ] Config validated/clamped at load; broken YAML → defaults + warning
- [ ] Player/config display strings escaped for the presentation layer

## Abuse resistance
- [ ] Player-triggerable actions have config cooldowns/caps
- [ ] Client (.) commands cannot affect others/round/server without a node
- [ ] Privileged actions audit-logged in one line each

## Data
- [ ] Stored player fields listed: <fields> · retention: <...> · deletion path: <...>
- [ ] No IPs/tokens/full IDs at Info log level; no credential handling anywhere

## Supply chain
- [ ] Dependencies pinned + declared in README; release zip contains no game/framework DLLs
- [ ] No dynamic assembly loading, process execution, or runtime self-update
- [ ] Optional co-plugin integrations fail off (absent = feature off, one log line)
