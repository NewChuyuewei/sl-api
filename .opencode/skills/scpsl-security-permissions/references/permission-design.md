# Permission Design — Nodes, Groups, Documentation

## Node scheme

```text
<prefix>.<area>.<action>          e.g.  arena.roles.assign
                                arena.roles.list      (read-only, lower trust)
                                arena.round.start     (round-wide blast radius)
                                arena.admin.reload    (server-level)
```

- Read vs write vs round vs server are different nodes even when one command exposes them all
  (check the *specific* sub-action's node inside the command, not one umbrella node at the door).
- Player-self-service actions (e.g. a personal preference command) may legitimately need no
  node — state that explicitly in the docs ("no permission: affects only the caller").

## Worked check (both frameworks — same discipline)

```csharp
// 1. Shape check (cheap, no side effects)
if (arguments.Count < 1) { response = "Usage: give <player> <item>"; return false; }

// 2. Permission check (before resolving/mutating anything)
if (!sender.HasPermission("myplugin.items.give"))       // LabAPI
// if (!sender.CheckPermission("myplugin.items.give"))  // EXILED
{ response = "Missing permission: myplugin.items.give"; return false; }

// 3. Target resolution + eligibility, 4. validated values, 5. act, 6. audit log
```

Audit-log privileged actions at Info (`admin X gave Y to Z`) — owners need the trail;
keep it one line, IDs at most, no sensitive payloads.

## README permission table (required format)

| Node | Allows | Suggested default |
|---|---|---|
| `myplugin.items.list` | List custom item IDs | All staff |
| `myplugin.items.give` | Give custom items to players | Admins |
| `myplugin.round.restart-event` | Start the special event round | Senior admins |

"Give everyone `myplugin.*`" is never the documented suggestion.
