# __PLUGIN_NAME__

TODO: one-paragraph description of what the plugin does and who it is for.

## Requirements

- SCP: Secret Laboratory dedicated server **14.2.x** with the bundled **LabAPI 1.1.7**
- This build (`.dll`) dropped into the LabAPI `plugins/` folder; no EXILED required
- Gameplay impact: TODO (state whether the server must be flagged transparently modded)

## Installation

1. Download `__PLUGIN_NAME__.dll` from the latest release.
2. Copy it to your LabAPI plugins folder
   (Windows: `%appdata%\SCP Secret Laboratory\LabAPI\plugins\`,
   Linux: `~/.config/SCP Secret Laboratory/LabAPI/plugins/`).
3. Restart the server and check the console for "`__PLUGIN_NAME__` … has been enabled".
4. Edit the generated config at `LabAPI/configs/<port>/__PLUGIN_NAME__/config.yml`,
   then restart (or reload) to apply.

## Configuration

| Key | Default | Meaning |
|---|---|---|
| `debug` | `false` | Verbose console logging. |
| `join_broadcast` | `Welcome to the server!` | Broadcast shown on join (empty disables). |
| `join_broadcast_seconds` | `6` | Join broadcast duration in seconds. |

## Commands & permissions

| Command | Surface | Permission node | Effect |
|---|---|---|---|
| `hello` | Remote Admin | `__PLUGIN_NAME__.hello` | Prints a greeting. |

## Building from source

```bash
dotnet build -c Release -p:SL_REFERENCES="C:\path\to\SCPSL_Data\Managed"
```

Targets `net48` against LabAPI 1.1.7.
