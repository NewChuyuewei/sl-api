# EXILED 9.15.3 — Config, Translations, Permissions

## Config files

- One YAML per server port: `Configs/<port>-config.yml` (Windows `%appdata%\EXILED\Configs\`,
  Linux `~/.config/EXILED/Configs/`). Every plugin's section is keyed by its `Prefix`
  in snake_case; keys are the snake_case of property names.
- The section is generated on first load with `[Description]` texts as comments. Server owners
  edit values, then restart or use EXILED's config reload. A renamed `Prefix` or property
  silently orphans old values — treat both as public API after release.
- `IsEnabled: false` disables the plugin at the loader level (the plugin never runs).
  `Debug: true` turns on EXILED debug logging for that plugin.

```yaml
my_plugin:
  is_enabled: true
  debug: false
  # Hint shown to a player after they are verified. Empty disables it.
  join_hint: Welcome to the server!
  join_hint_seconds: 5
```

## Config class rules

- Implement `Exiled.API.Interfaces.IConfig` (`IsEnabled`, `Debug` mandatory).
- Public properties with getters/setters and initializer defaults; `[Description("...")]`
  from `System.ComponentModel` on every owner-facing property.
- Nested objects, lists, dictionaries, `Vector3`, and EXILED value types (e.g. `Broadcast`)
  serialize fine — keep shapes simple and provide complete defaults (empty collections,
  not null), because owners will delete lines.
- Validate in `OnEnabled` (clamp ranges, fall back + `Log.Warn` on nonsense). Bad config must
  never throw out of `OnEnabled`.

## Translations

For localizable strings use `Plugin<Config, Translation>`:

```csharp
public sealed class Translation : ITranslation
{
    public string JoinMessage { get; set; } = "Welcome, {player}!";
}
```

Access via `plugin.Translation.JoinMessage`. Strings owners merely *customize* (not translate)
belong in Config; strings per-language belong in Translation. Files land in
`Translations/<port>-translation.yml`, again keyed by `Prefix`.

## Permissions

- Nodes are strings you invent: `<prefix>.<area>.<action>` (e.g. `myplugin.hello`).
- Check with `sender.CheckPermission("node")` (`Exiled.Permissions.Extensions`) in commands;
  for player objects use the player permission extensions from the same assembly.
- Server owners grant nodes to groups in the EXILED permissions config
  (`permissions.yml` in the EXILED configs area). Document: node, default group suggestion,
  and what it allows — in the plugin README, every time.
- No node granted = deny. Never gate privileged actions on nickname, SteamID hard-coded in
  source, or "has RA access" alone when a finer node is appropriate.
