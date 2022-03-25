// -----------------------------------------------------------------------
// <copyright file="PlayerTeamKillsDataCommand.cs" company="Mistaken">
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
    internal class PlayerTeamKillsDataCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "ptkd";

        public override string Description => "Return Player TK Data";

        public string PluginName => PluginHandler.Instance.Name;

        public override string Command => "playerTKData";

        public override string[] Aliases => new string[] { "ptkd" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool s)
        {
            s = false;
            if (args.Length == 0)
                return new string[] { "PTKD [Id]" };
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                var teamAttacks = TeamAttack.TeamAttacks.SelectMany(x => x.Value).Where(x => x.Attacker.UserId == player.UserId).ToArray();
                var teamKills = TeamKill.TeamKills.SelectMany(x => x.Value).Where(x => x.Attacker.UserId == player.UserId).ToArray();
                List<(long Time, string[] Info)> tmp = new List<(long Time, string[] Info)>();
                foreach (var teamAttack in teamAttacks)
                {
                    tmp.Add((teamAttack.Timestamp.Ticks, new string[]
                    {
                        "=============================================================",
                        "TeamAttack",
                        $"Victim: ({teamAttack.Victim.Id}) {teamAttack.Victim.Nickname} ({teamAttack.VictimTeam})",
                        $"Attacker Team: {teamAttack.AttackerTeam}",
                        $"Damage: {teamAttack.Handler.DealtHealthDamage}", // też nwm
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
                        $"Victim: ({teamKill.Victim.Id}) {teamKill.Victim.Nickname} ({teamKill.VictimTeam})",
                        $"Attacker Team: {teamKill.AttackerTeam}",
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
                return new string[] { "Player not found", "PTKD [Id]" };
            s = true;
            return output;
        }
    }
}
