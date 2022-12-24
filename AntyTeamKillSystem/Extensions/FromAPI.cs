using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistaken.AntyTeamKillSystem.Extensions;

internal static class FromApi
{
    /// <summary>
    /// Send Broadcast.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="message">Message.</param>
    /// <param name="flags">Flags.</param>
    public static void Broadcast(
        string tag,
        ushort duration,
        string message,
        Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
    {
        if (flags == global::Broadcast.BroadcastFlags.AdminChat)
        {
            var fullMessage = $"<color=orange>[<color=green>{tag}</color>]</color> {message}";
            foreach (var item in Player.GetPlayers().Where(
                         p => p.Connection != null && PermissionsHandler.IsPermitted(
                             p.ReferenceHub.serverRoles.Permissions,
                             PlayerPermissions.AdminChat)))
                item.ReferenceHub.queryProcessor.TargetReply(
                    item.Connection,
                    "@" + fullMessage,
                    true,
                    false,
                    string.Empty);
        }
        else
            Server.SendBroadcast($"<color=orange>[<color=green>{tag}</color>]</color> {message}", duration, flags);
    }
}
