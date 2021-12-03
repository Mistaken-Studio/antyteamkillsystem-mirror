// -----------------------------------------------------------------------
// <copyright file="GetAttackerCommand.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Mistaken.API;
using Mistaken.API.Commands;

namespace Mistaken.AntyTeamKillSystem.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class GetAttackerCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "ga";

        public override string Description => "Get Attacker";

        public string PluginName => PluginHandler.Instance.Name;

        public override string Command => "getattacker";

        public override string[] Aliases => new string[] { "ga" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool s)
        {
            s = false;
            if (args.Length == 0)
                return new string[] { "GA (Id)" };
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                var teamAttacks = TeamAttack.TeamAttacks.SelectMany(x => x.Value).Where(x => x.Victim.UserId == player.UserId).ToArray();
                var teamKills = TeamKill.TeamKills.SelectMany(x => x.Value).Where(x => x.Victim.UserId == player.UserId).ToArray();
                List<(long Time, string[] Info)> tmp = new List<(long Time, string[] Info)>();
                foreach (var teamAttack in teamAttacks)
                {
                    tmp.Add((teamAttack.Timestamp.Ticks, new string[]
                    {
                        "=============================================================",
                        "TeamAttack",
                        $"Attacker: ({teamAttack.Attacker.Id}) {teamAttack.Attacker.Nickname} ({teamAttack.AttackerTeam})",
                        $"Attacker UserId: {teamAttack.Attacker.UserId}",
                        $"Damage: {teamAttack.Handler.Amount}",
                        $"Tool: {teamAttack.Handler.Type}",
                        $"Code: {teamAttack.DetectionCode}",
                        $"RoundsAgo: {RoundPlus.RoundId - teamAttack.RoundId}",
                    }));
                }

                foreach (var teamKill in teamKills)
                {
                    tmp.Add((teamKill.Timestamp.Ticks, new string[]
                    {
                        "=============================================================",
                        "TeamKill",
                        $"Attacker: ({teamKill.Attacker.Id}) {teamKill.Attacker.Nickname} ({teamKill.AttackerTeam})",
                        $"Attacker UserId: {teamKill.Attacker.UserId}",
                        $"Tool: {teamKill.Handler.Type}",
                        $"Code: {teamKill.DetectionCode}",
                        $"RoundsAgo: {RoundPlus.RoundId - teamKill.RoundId}",
                    }));
                }

                foreach (var item in tmp.OrderByDescending(x => x.Time).Select(x => x.Info))
                    tor.AddRange(item);

                var torArray = tor.ToArray();
                NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
                return torArray;
            });
            if (!success)
                return new string[] { "Player not found", "GA (Id)" };
            s = true;
            return output;
        }
    }
}
