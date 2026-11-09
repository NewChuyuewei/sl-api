# <Plugin Name>

<2-3 sentences: what it does in a round, who it is for. No lore wall.>

## Requirements
- SCP: Secret Laboratory dedicated server **14.2.x**
- Framework: **LabAPI 1.1.7** (bundled with the game) <!-- or: EXILED 9.15.3 (must be installed; this plugin does not run on LabAPI-only servers) -->
- Gameplay impact: <changes gameplay / admin tooling only> — transparently modded: <state honestly>

## Installation
1. Download `<PluginName>.dll` from the latest release.
2. Copy it to <exact plugins folder for the framework>.
3. Restart the server; confirm "`<Plugin Name>` ... has been enabled" in the console.
4. Edit the generated config at <exact config path/section>, then restart/reload.

## Configuration
| Key | Default | Effect |
|---|---|---|
|  |  |  |

## Commands
| Command | Surface | Permission | Usage/effect |
|---|---|---|---|
|  | RA / client `.` / server | `prefix.area.action` |  |

## Permissions
| Node | Allows | Suggested default |
|---|---|---|
|  |  | Admins |

## Custom content (if any)
| Type | Name | ID | Base | Spawn rule |
|---|---|---|---|---|
| Role/Item |  |  |  |  |

## Compatibility and notes
<Known incompatibilities, load-order needs, Harmony patches + kill-switch, data stored + deletion.>

## Troubleshooting
- **Plugin does not load**: check framework (above), folder, and the first error in the server log after boot.
- **Config changes ignored**: confirm you edited <path> for the right server port, then restart/reload.
- **Bug reports**: include framework/game/plugin versions and the full log from boot — see CONTRIBUTING/tests.

## Building from source
```bash
dotnet build -c Release -p:<SL_REFERENCES|EXILED_REFERENCES>="C:\path\to\references"
```
Targets net48. Reference assemblies are never shipped with the plugin.
