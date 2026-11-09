# Evidence Protocol

| Claim under verification | Acceptable evidence | Not evidence |
|---|---|---|
| API used correctly | Pinned upstream file/example/release named; or code matches framework template | "Standard pattern", memory of old EXILED/LabAPI versions |
| Builds | Command + config + result, this build hash | "It compiled earlier", IDE screenshot without command |
| Behaviour works | Scenario ID + framework + hash + observed result/log excerpt | Agent narrative, code reading alone |
| No lifecycle leak | Register/unregister list diff + reload/round scenario result | Reading Enable() alone |
| Faster | Before/after, same scenario, population stated | Removing "obvious" allocations, micro-benchmarks off-server for game loops |
| Secure | Traced path per privileged action: entry → permission check → validation → effect; node table vs code cross-check | Absence of scanner hits |
| Ready to release | G0–G5 gate lines with evidence pointers (quality-gates.md) | A green build plus optimism |

Escalation: missing evidence → request the specific artifact once, by name. Still missing →
the claim is recorded UNVERIFIED and the phase gate stays closed. Verifiers do not fill
gaps with their own quick fixes to production code.
