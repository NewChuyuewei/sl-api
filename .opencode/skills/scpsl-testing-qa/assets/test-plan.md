# Test Plan — <Plugin Name> v<version>

- Build/commit: <hash> · Framework: LabAPI 1.1.7 | EXILED 9.15.3 · Game: SCP:SL 14.2.x
- Test server: <host/panel> · Player count: <n> (human/bot) · Other plugins: <list>
- Acceptance criteria source: <link to scpsl-planning output>

## Layer 1 — Core unit tests
- [ ] Test project green: <command + result>
- [ ] Behaviour-rule coverage: <rules covered / total>

## Layer 2 — Server scenarios
| ID | Scenario | Result (Pass/Fail) | Evidence (log/screenshot) | Bug ID |
|---|---|---|---|---|
| L1 | Cold boot, clean enable |  |  |  |
| L2 | Disable mid-round |  |  |  |
| L3 | Re-enable, no duplication |  |  |  |
| L5 | Round restart state reset |  |  |  |

<Add feature/command/config/coexistence rows from the scenario catalogue.>

## Skipped scenarios (with reason)
- 

## Known defects accepted for this build
| Bug | Accepted by | Follow-up |
|---|---|---|

## Sign-off
Tested by: <name/agent> · Date: <yyyy-mm-dd> · Verdict: Ready | Not ready — <reason>
