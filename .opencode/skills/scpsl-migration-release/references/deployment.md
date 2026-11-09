# Deployment — LabAPI and EXILED Servers

Confirm the exact folders on the target machine after one server boot; hosting panels,
multi-port setups, and loader configs shift details. Paths below are the upstream defaults.

## LabAPI (bundled with SCP:SL since 14.1 — no framework install)

```text
Windows:  %appdata%\SCP Secret Laboratory\LabAPI\
Linux:    ~/.config/SCP Secret Laboratory/LabAPI/
├── plugins/        <- your plugin .dll (a port-specific subfolder may be used per loader config)
├── dependencies/   <- shared non-plugin dependency .dll files
└── configs/<port>/<PluginName>/   <- generated: config.yml + properties.yml
```

1. Drop the plugin DLL into `plugins/`; real dependencies into `dependencies/`.
   Another *plugin* the release depends on also goes into `plugins/` (never `dependencies/`).
2. Restart the server; confirm the enable log line; stop errors at the **first** exception.
3. Edit `configs/<port>/<PluginName>/config.yml`; enable/disable via `properties.yml`.
4. Restart (or supported reload) and verify behaviour in a test round.

## EXILED (server owner installs the framework)

```text
Windows:  %appdata%\EXILED\
Linux:    ~/.config/EXILED/
├── Plugins/                          <- your plugin .dll (+ dependencies subfolder per install)
├── Configs/<port>-config.yml         <- your section = plugin Prefix (snake_case)
└── Translations/<port>-translation.yml
```

1. Confirm EXILED version on the server (`9.15.3` target). An EXILED plugin on a LabAPI-only
   server (or the reverse) loads nowhere — this is the most common "plugin does nothing" report.
2. Drop the DLL into `Plugins/`; restart; confirm the enable line and that a config section
   with your `Prefix` was generated.
3. Edit the section (`is_enabled`, feature keys), grant permission nodes in the EXILED
   permissions config, restart/reload, test.

## Clean-server smoke test (both frameworks)

Fresh server copy → install only your release zip's contents → boot → join with a test client
→ run one round → trigger each feature once → run each command as allowed and denied user →
reload/restart the plugin if supported → disable and confirm behaviour stops cleanly.
If it only works on your dev server, you have an undeclared dependency — find it before release.
