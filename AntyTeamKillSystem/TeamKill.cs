// -----------------------------------------------------------------------
// <copyright file="TeamKill.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Extensions;

namespace Mistaken.AntyTeamKillSystem
{
    /// <summary>
    /// Struct containing information about TeamKill.
    /// </summary>
    public struct TeamKill
    {
        /// <summary>
        /// Gets list of all TeamKills in each round.
        /// </summary>
        public static ReadOnlyDictionary<int, List<TeamKill>> TeamKillsReadOnly => new ReadOnlyDictionary<int, List<TeamKill>>(TeamKills);

        /// <summary>
        /// Creates new <see cref="TeamKill"/> based on <paramref name="ev"/>.
        /// </summary>
        /// <param name="ev">Event data that TeamKill is based on.</param>
        /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
        /// <param name="attackerTeam">Attacker Team if diffrent from current.</param>
        /// <param name="attacker">Attacker if diffrent from <paramref name="ev"/>'s <see cref="Exiled.Events.EventArgs.DyingEventArgs.Killer"/>.</param>
        /// <returns>New <see cref="TeamKill"/> instance.</returns>
        public static TeamKill? Create(Exiled.Events.EventArgs.DyingEventArgs ev, string code, Team? attackerTeam = null, Player attacker = null)
            => Create(attacker ?? ev.Killer, ev.Target, ev.HitInformation, code, attackerTeam);

        /// <summary>
        /// Creates new <see cref="TeamKill"/>.
        /// </summary>
        /// <param name="attacker"><inheritdoc cref="Attacker"/></param>
        /// <param name="target"><inheritdoc cref="Victim"/></param>
        /// <param name="hitInfo"><inheritdoc cref="HitInformation"/></param>
        /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
        /// <param name="attackerTeam">Attacker Team if diffrent from current.</param>
        /// <returns>New <see cref="TeamKill"/> instance.</returns>
        public static TeamKill? Create(Player attacker, Player target, PlayerStats.HitInfo hitInfo, string code, Team? attackerTeam = null)
        {
            if (!Round.IsStarted)
                return null;
            int roundId = RoundPlus.RoundId;
            var teamKill = new TeamKill
            {
                Attacker = attacker,
                Victim = target,
                VictimTeam = target.Team,
                AttackerTeam = attackerTeam ?? attacker.Team,
                DetectionCode = code,
                HitInformation = hitInfo,
                RoundId = roundId,
            };
            if (!TeamKills.ContainsKey(roundId))
                TeamKills[roundId] = new List<TeamKill>();
            TeamKills[roundId].Add(teamKill);
            Handler.OnTeamKill(teamKill);
            return teamKill;
        }

        /// <summary>
        /// Id of round in which TeamKill happened.
        /// </summary>
        public int RoundId;

        /// <summary>
        /// TeamKilling player.
        /// </summary>
        public Player Attacker;

        /// <summary>
        /// Team of attacking player.
        /// </summary>
        public Team AttackerTeam;

        /// <summary>
        /// TeamKilled player.
        /// </summary>
        public Player Victim;

        /// <summary>
        /// Team of attacking player.
        /// </summary>
        public Team VictimTeam;

        /// <summary>
        /// Detection Code.
        /// </summary>
        public string DetectionCode;

        /// <summary>
        /// Information about hit.
        /// </summary>
        public PlayerStats.HitInfo HitInformation;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join("\n", PluginHandler.Instance.Translation.TeamKillInfo)
                .Replace("{Attacker}", this.Attacker.ToString(false) + (this.Attacker.IsConnected ? string.Empty : " (DISCONNECTED)"))
                .Replace("{AttackerTeam}", this.AttackerTeam.ToString())
                .Replace("{Victim}", this.Victim.ToString(false))
                .Replace("{VictimTeam}", this.VictimTeam.ToString())
                .Replace("{Tool}", this.HitInformation.GetDamageName())
                .Replace("{Amount}", this.HitInformation.Amount.ToString())
                .Replace("{DetectionCode}", this.DetectionCode)
                ;
        }

        internal static readonly Dictionary<int, List<TeamKill>> TeamKills = new Dictionary<int, List<TeamKill>>();
    }
}
