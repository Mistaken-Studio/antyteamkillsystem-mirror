// -----------------------------------------------------------------------
// <copyright file="TeamAttack.cs" company="Mistaken">
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
    /// Struct containing information about TeamAttack.
    /// </summary>
    public struct TeamAttack
    {
        /// <summary>
         /// Gets list of all TeamAttacks in each round.
         /// </summary>
        public static ReadOnlyDictionary<int, List<TeamAttack>> TeamAttacksReadOnly => new ReadOnlyDictionary<int, List<TeamAttack>>(TeamAttacks);

        /// <summary>
        /// Creates new <see cref="TeamAttack"/> based on <paramref name="ev"/>.
        /// </summary>
        /// <param name="ev">Event data that TeamKill is based on.</param>
        /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
        /// <param name="attackerTeam">Attacker Team if diffrent from current.</param>
        /// <param name="attacker">Attacker if diffrent from <paramref name="ev"/>'s <see cref="Exiled.Events.EventArgs.HurtingEventArgs.Attacker"/>.</param>
        /// <returns>New <see cref="TeamAttack"/> instance.</returns>
        public static TeamAttack? Create(Exiled.Events.EventArgs.HurtingEventArgs ev, string code, Team? attackerTeam = null, Player attacker = null)
            => Create(attacker ?? ev.Attacker, ev.Target, ev.HitInformations, code, attackerTeam);

        /// <summary>
        /// Creates new <see cref="TeamAttack"/>.
        /// </summary>
        /// <param name="attacker"><inheritdoc cref="Attacker"/></param>
        /// <param name="target"><inheritdoc cref="Victim"/></param>
        /// <param name="hitInfo"><inheritdoc cref="HitInformation"/></param>
        /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
        /// <param name="attackerTeam">Attacker Team if diffrent from current.</param>
        /// <returns>New <see cref="TeamAttack"/> instance.</returns>
        public static TeamAttack? Create(Player attacker, Player target, PlayerStats.HitInfo hitInfo, string code, Team? attackerTeam = null)
        {
            if (!Round.IsStarted)
                return null;
            int roundId = RoundPlus.RoundId;
            var teamAttack = new TeamAttack
            {
                Attacker = attacker,
                Victim = target,
                VictimTeam = target.Team,
                AttackerTeam = attackerTeam ?? attacker.Team,
                DetectionCode = code,
                HitInformation = hitInfo,
                RoundId = roundId,
            };
            if (!TeamAttacks.ContainsKey(roundId))
                TeamAttacks[roundId] = new List<TeamAttack>();
            TeamAttacks[roundId].Add(teamAttack);
            Handler.OnTeamAttack(teamAttack);
            return teamAttack;
        }

        /// <summary>
        /// Id of round in which TeamAttack happened.
        /// </summary>
        public int RoundId;

        /// <summary>
        /// TeamAttacking player.
        /// </summary>
        public Player Attacker;

        /// <summary>
        /// Team of attacking player.
        /// </summary>
        public Team AttackerTeam;

        /// <summary>
        /// TeamAttacked player.
        /// </summary>
        public Player Victim;

        /// <summary>
        /// Team of attacked player.
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
            return string.Join("\n", PluginHandler.Instance.Translation.TeamAttackInfo)
                .Replace("{Attacker}", this.Attacker.ToString(false) + (this.Attacker.IsConnected && !string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.Disconnected) ? string.Empty : $" ({PluginHandler.Instance.Translation.Disconnected})"))
                .Replace("{AttackerTeam}", this.AttackerTeam.ToString())
                .Replace("{Victim}", this.Victim.ToString(false))
                .Replace("{VictimTeam}", this.VictimTeam.ToString())
                .Replace("{Tool}", this.HitInformation.GetDamageName())
                .Replace("{Amount}", this.HitInformation.Amount.ToString())
                .Replace("{DetectionCode}", this.DetectionCode)
                ;
        }

        internal static readonly Dictionary<int, List<TeamAttack>> TeamAttacks = new Dictionary<int, List<TeamAttack>>();
    }
}
