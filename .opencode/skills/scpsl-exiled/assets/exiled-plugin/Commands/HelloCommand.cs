using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace __PLUGIN_NAME__.Commands;

/// <summary>
/// Example Remote Admin command: <c>hello</c>.
/// EXILED auto-registers every <see cref="ICommand"/> in this assembly when the
/// plugin enables — do not register it manually. Add more surfaces (client "."
/// console / server console) by stacking more
/// <see cref="CommandHandlerAttribute"/>s.
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
        // Permission node granted via the EXILED permissions config. Deny by default.
        if (!sender.CheckPermission("__plugin_name__.hello"))
        {
            response = "You do not have permission to use this command (node: __plugin_name__.hello).";
            return false;
        }

        response = "Hello from __PLUGIN_NAME__!";
        return true;
    }
}
