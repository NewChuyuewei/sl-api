# Glossary and Folder Layouts

## Glossary

- **LabAPI** — Northwood's official SCP:SL plugin loader/framework. Bundled with the dedicated
  server (`SCPSL_Data/Managed/LabAPI.dll`) since game 14.1. Successor to the archived NwPluginAPI.
- **EXILED** — Community high-level plugin framework (ExMod Team). Installed by the server owner
  on top of the game; EXILED 9.15 internally references LabAPI assemblies, but EXILED plugins
  are still EXILED plugins — they need EXILED present at runtime.
- **Wrapper** — Framework class standing in for a game object (`Player`, `Door`, `Room`, `Item`,
  `Pickup`, …). Prefer wrappers over `ReferenceHub`/game types; wrappers survive game updates better.
- **MEC** — More Effective Coroutines. `Timing.CallDelayed(seconds, action)`,
  `Timing.RunCoroutine(IEnumerator<float>)`, `Timing.WaitForSeconds`. The only sanctioned
  "delay" mechanism; runs on the main thread.
- **RA** — Remote Admin (in-game admin panel/console). **Dot command** — client console command
  prefixed with `.`. **Game console** — the server's own console.
- **CASSIE** — The facility announcement system; both frameworks expose announcement helpers.
- **Publicized assembly** — `Assembly-CSharp-Publicized.dll` with access modifiers loosened,
  used by EXILED development. LabAPI development normally uses the plain `Assembly-CSharp.dll`.
- **Transparently modded** — Northwood server-listing flag for servers whose modifications are
  non-intrusive. Gameplay-changing plugins affect verification/listing — flag it in the README
  (LabAPI: `Plugin.IsTransparent`).

## Server folder layouts (deployment targets)

### LabAPI (no separate install — bundled with the game)

```text
Windows:  %appdata%\SCP Secret Laboratory\LabAPI\
Linux:    ~/.config/SCP Secret Laboratory/LabAPI/
├── plugins/          # plugin .dll files (port-specific subfolders may appear per loader config)
├── dependencies/     # shared dependency .dll files (NOT other plugins)
└── configs/<port>/   # per-plugin config folders (config.yml, properties.yml, ...)
```

Enable/disable per plugin is controlled by its `properties.yml`, not by an `is_enabled`
config key (contrast with EXILED). Core loader file: `LabApi-<port>.yml`.

### EXILED (owner-installed)

```text
Windows:  %appdata%\EXILED\
Linux:    ~/.config/EXILED/
├── Plugins/                  # plugin .dll files (dependencies in Plugins/dependencies on many installs)
├── Configs/<port>-config.yml       # all plugin configs, keyed by plugin prefix
└── Translations/<port>-translation.yml
```

Always confirm the exact generated layout on the target server after one boot —
hosting panels and multi-port setups vary. Never hard-code these paths in plugin code;
use framework path APIs (`LabApi` `PathManager` / EXILED `Paths`).

## Development reference assemblies

From a local dedicated-server install, `SCPSL_Data/Managed/` typically provides:
`Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `CommandSystem.Core.dll`, `Mirror.dll`,
`NorthwoodLib.dll`, `UnityEngine.CoreModule.dll` (+ other Unity modules as needed), `LabAPI.dll`.
EXILED development additionally uses the EXILED assemblies (`Exiled.API.dll`, `Exiled.Events.dll`,
optionally `Exiled.CustomItems.dll`, `Exiled.CustomRoles.dll`, `Exiled.Permissions.dll`, …)
and the publicized game assembly. Keep `Private=false` / `CopyLocal=false` on all game and
framework references so they are never shipped with the plugin.
