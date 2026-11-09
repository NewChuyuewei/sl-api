using System;
using CommandSystem;
using LabApi.Features.Permissions; // For sender.HasPermission(...) — check the pinned API docs if this moves.

namespace __PLUGIN_NAME__.Commands;

/// <summary>
/// Example Remote Admin command: <c>hello</c>.
/// LabAPI auto-discovers every <see cref="ICommand"/> in this assembly; no manual
/// registration is needed. Add more surfaces (client "." console / server console)
/// by stacking more <see cref="CommandHandlerAttribute"/>s.
/// </summary>
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class HelloCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "hello";

    /// <inheritdoc />
    public string[] Aliases => Array.Empty<string>();

    /// <inheritdoc />
    public string Description => "Prints a greeting. Usage: hello";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        // Every privileged command checks a permission node before doing anything.
        if (!sender.HasPermission("__PLUGIN_NAME__.hello"))
        {
            response = "You do not have permission to use this command (node: __PLUGIN_NAME__.hello).";
            return false;
        }

        response = "Hello from __PLUGIN_NAME__!";
        return true;
    }
}
