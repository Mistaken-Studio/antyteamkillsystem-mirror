// -----------------------------------------------------------------------
// <copyright file="TeamAttack.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;

namespace Mistaken.AntyTeamKillSystem;

/// <summary>
/// Struct containing information about TeamAttack.
/// </summary>
public struct TeamAttack
{
    /// <summary>
    /// Gets list of all TeamAttacks in each round.
    /// </summary>
    public static ReadOnlyDictionary<int, List<TeamAttack>> TeamAttacksReadOnly => new(TeamAttacks);

    /// <summary>
    /// Creates new <see cref="TeamAttack"/> based on <paramref name="ev"/>.
    /// </summary>
    /// <param name="ev">Event data that TeamKill is based on.</param>
    /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
    /// <param name="attackerTeam">Attacker Team if different from current.</param>
    /// <param name="attacker">Attacker if different from <paramref name="ev"/>'s <see cref="EventArgs.HurtingEventArgs.Attacker"/>.</param>
    /// <returns>New <see cref="TeamAttack"/> instance.</returns>
    public static TeamAttack? Create(
        Player killer,
        Player target,
        DamageHandlerBase handler,
        string code,
        Team? attackerTeam = null,
        Player attacker = null)
        => Create(attacker ?? killer, target, handler, code, attackerTeam);

    /// <summary>
    /// Creates new <see cref="TeamAttack"/>.
    /// </summary>
    /// <param name="attacker"><inheritdoc cref="Attacker"/></param>
    /// <param name="target"><inheritdoc cref="Victim"/></param>
    /// <param name="handler"><inheritdoc cref="Handler"/></param>
    /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
    /// <param name="attackerTeam">Attacker Team if different from current.</param>
    /// <returns>New <see cref="TeamAttack"/> instance.</returns>
    public static TeamAttack? Create(
        Player attacker,
        Player target,
        DamageHandlerBase handler,
        string code,
        Team? attackerTeam = null)
    {
        try
        {
            if (!Round.IsRoundStarted)
                return null;
        }
        catch
        {
            return null;
        }

        var roundId = AntyTeamkillHandler.RoundIdCounter;
        var teamAttack = new TeamAttack
        {
            Attacker = attacker,
            Victim = target,
            VictimTeam = target.Role.GetTeam(),
            AttackerTeam = attackerTeam ?? attacker.Role.GetTeam(),
            DetectionCode = code,
            Handler = handler as StandardDamageHandler,
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
    public StandardDamageHandler Handler;

    /// <summary>
    /// Time when TeamAttack happen.
    /// </summary>
    public DateTime Timestamp;

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join("\n", Plugin.Instance.Translation.TeamAttackInfo)
                .Replace(
                    "{Attacker}",
                    this.Attacker.ToString(false) +
                    ((this.Attacker.Connection?.isReady ?? false) &&
                     !string.IsNullOrWhiteSpace(Plugin.Instance.Translation.Disconnected)
                        ? string.Empty
                        : $" ({Plugin.Instance.Translation.Disconnected})"))
                .Replace("{AttackerTeam}", this.AttackerTeam.ToString())
                .Replace("{Victim}", this.Victim.ToString(false))
                .Replace("{VictimTeam}", this.VictimTeam.ToString())
                .Replace("{Tool}", this.Handler.ServerLogsText)
                .Replace(
                    "{Amount}",
                    this.Handler.Damage.ToString())
                .Replace("{DetectionCode}", this.DetectionCode)
            ;
    }

    internal static readonly Dictionary<int, List<TeamAttack>> TeamAttacks = new();
}