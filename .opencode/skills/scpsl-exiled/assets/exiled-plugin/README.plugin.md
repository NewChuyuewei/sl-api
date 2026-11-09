# __PLUGIN_NAME__

TODO: one-paragraph description of what the plugin does and who it is for.

## Requirements

- SCP: Secret Laboratory dedicated server **14.2.x**
- **EXILED 9.15.3** installed (this plugin does not run on LabAPI-only servers)
- Gameplay impact: TODO (state whether the server must be flagged transparently modded)

## Installation

1. Download `__PLUGIN_NAME__.dll` from the latest release.
2. Copy it to your EXILED plugins folder
   (Windows: `%appdata%\EXILED\Plugins\`,
   Linux: `~/.config/EXILED/Plugins/`).
3. Restart the server and check the console for "`__PLUGIN_NAME__` … has been enabled".
4. Edit the `__plugin_name__` section of `EXILED/Configs/<port>-config.yml`,
   then restart (or reload configs) to apply.

## Configuration

| Key | Default | Meaning |
|---|---|---|
| `is_enabled` | `true` | Loader-level on/off switch for this plugin. |
| `debug` | `false` | Verbose console logging. |
| `join_hint` | `Welcome to the server!` | Hint shown after verification (empty disables). |
| `join_hint_seconds` | `5` | Join hint duration in seconds. |

## Commands & permissions

| Command | Surface | Permission node | Effect |
|---|---|---|---|
| `hello` | Remote Admin | `__plugin_name__.hello` | Prints a greeting. |

Grant nodes to groups in the EXILED permissions configuration.

## Building from source

```bash
dotnet build -c Release -p:EXILED_REFERENCES="C:\path\to\references"
```

Targets `net48` against EXILED 9.15.3.
