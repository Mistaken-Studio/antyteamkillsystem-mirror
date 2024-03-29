﻿// -----------------------------------------------------------------------
// <copyright file="TeamAttack.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
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
            => Create(attacker ?? ev.Attacker, ev.Target, ev.Handler, code, attackerTeam);

        /// <summary>
        /// Creates new <see cref="TeamAttack"/>.
        /// </summary>
        /// <param name="attacker"><inheritdoc cref="Attacker"/></param>
        /// <param name="target"><inheritdoc cref="Victim"/></param>
        /// <param name="handler"><inheritdoc cref="Handler"/></param>
        /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
        /// <param name="attackerTeam">Attacker Team if diffrent from current.</param>
        /// <returns>New <see cref="TeamAttack"/> instance.</returns>
        public static TeamAttack? Create(Player attacker, Player target, DamageHandler handler, string code, Team? attackerTeam = null)
        {
            try
            {
                if (!Round.IsStarted)
                    return null;
            }
            catch
            {
                return null;
            }

            int roundId = RoundPlus.RoundId;
            var teamAttack = new TeamAttack
            {
                Attacker = attacker,
                Victim = target,
                VictimTeam = target.Role.Team,
                AttackerTeam = attackerTeam ?? attacker.Role.Team,
                DetectionCode = code,
                Handler = handler,
                RoundId = roundId,
                Timestamp = DateTime.Now,
            };
            if (!TeamAttacks.ContainsKey(roundId))
                TeamAttacks[roundId] = new List<TeamAttack>();
            TeamAttacks[roundId].Add(teamAttack);
            AntyTeamkillHandler.OnTeamAttack(teamAttack);
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
        public DamageHandler Handler;

        /// <summary>
        /// Time when TeamAttack happend.
        /// </summary>
        public DateTime Timestamp;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join("\n", PluginHandler.Instance.Translation.TeamAttackInfo)
                .Replace("{Attacker}", this.Attacker.ToString(false) + (this.Attacker.IsConnected && !string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.Disconnected) ? string.Empty : $" ({PluginHandler.Instance.Translation.Disconnected})"))
                .Replace("{AttackerTeam}", this.AttackerTeam.ToString())
                .Replace("{Victim}", this.Victim.ToString(false))
                .Replace("{VictimTeam}", this.VictimTeam.ToString())
                .Replace("{Tool}", this.Handler.Type.ToString())
                .Replace("{Amount}", this.Handler.DealtHealthDamage.ToString()) // nwm czy tu ma być DealtHealthDamage czy Damage
                .Replace("{DetectionCode}", this.DetectionCode)
                ;
        }

        internal static readonly Dictionary<int, List<TeamAttack>> TeamAttacks = new Dictionary<int, List<TeamAttack>>();
    }
}
