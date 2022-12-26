// -----------------------------------------------------------------------
// <copyright file="TeamKill.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;

namespace Mistaken.AntyTeamKillSystem;

/// <summary>
/// Struct containing information about TeamKill.
/// </summary>
public struct TeamKill
{
    /// <summary>
    /// Gets list of all TeamKills in each round.
    /// </summary>
    public static ReadOnlyDictionary<int, List<TeamKill>> TeamKillsReadOnly => new(TeamKills);

    /// <summary>
    /// Creates new <see cref="TeamKill"/> based on <paramref name="ev"/>.
    /// </summary>
    /// <param name="ev">Event data that TeamKill is based on.</param>
    /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
    /// <param name="attackerTeam">Attacker Team if different from current.</param>
    /// <param name="attacker">Attacker if different from <paramref name="ev"/>'s <see cref="EventArgs.DyingEventArgs.Killer"/>.</param>
    /// <returns>New <see cref="TeamKill"/> instance.</returns>
    public static TeamKill? Create(Player killer, Player target, DamageHandlerBase handler, string code, Team? attackerTeam = null, Player attacker = null)
        => Create(attacker ?? killer, target, handler, code, attackerTeam);

    /// <summary>
    /// Creates new <see cref="TeamKill"/>.
    /// </summary>
    /// <param name="attacker"><inheritdoc cref="Attacker"/></param>
    /// <param name="target"><inheritdoc cref="Victim"/></param>
    /// <param name="handler"><inheritdoc cref="Handler"/></param>
    /// <param name="code"><inheritdoc cref="DetectionCode"/></param>
    /// <param name="attackerTeam">Attacker Team if different from current.</param>
    /// <returns>New <see cref="TeamKill"/> instance.</returns>
    public static TeamKill? Create(Player attacker, Player target, DamageHandlerBase handler, string code, Team? attackerTeam = null)
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
        var teamKill = new TeamKill
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
        if (!TeamKills.ContainsKey(roundId))
            TeamKills[roundId] = new List<TeamKill>();
        TeamKills[roundId].Add(teamKill);
        AntyTeamkillHandler.OnTeamKill(teamKill);
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
    public StandardDamageHandler Handler;

    /// <summary>
    /// Time when TeamKill happen.
    /// </summary>
    public DateTime Timestamp;

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join("\n", Plugin.Instance.Translation.TeamKillInfo)
                .Replace("{Attacker}", this.Attacker.ToString(false) + (this.Attacker.Connection?.isReady ?? false ? string.Empty : " (DISCONNECTED)"))
                .Replace("{AttackerTeam}", this.AttackerTeam.ToString())
                .Replace("{Victim}", this.Victim.ToString(false))
                .Replace("{VictimTeam}", this.VictimTeam.ToString())
                .Replace("{Tool}", this.Handler.ServerLogsText)
                .Replace("{Amount}", this.Handler.Damage.ToString())
                .Replace("{DetectionCode}", this.DetectionCode)
            ;
    }

    internal static readonly Dictionary<int, List<TeamKill>> TeamKills = new();
}

/// <summary>
/// Damage Extensions.
/// </summary>
public static class DamageExtensions
{
    /// <summary>
    /// Returns if player will die because of damage caused by <paramref name="handler"/>.
    /// </summary>
    /// <param name="player">Player.</param>
    /// <param name="handler">Damage Cause.</param>
    /// <returns>If player will die because of this damage.</returns>
    public static bool WillDie(this Player player, StandardDamageHandler handler)
    {
        var tmp = player.GetStatModule<AhpStat>()._activeProcesses.Select(x => new { Process = x, x.CurrentAmount });
        var hp = player.Health;
        var damage = handler.Damage;
        var death = handler.ApplyDamage(player.ReferenceHub) == DamageHandlerBase.HandlerOutput.Death;
        handler.Damage = damage;
        player.Health = hp;
        foreach (var item in tmp)
            item.Process.CurrentAmount = item.CurrentAmount;
        return death;
    }

    /// <summary>
    /// Returns real dealt damage to the player.
    /// </summary>
    /// <param name="player">Player.</param>
    /// <param name="handler">Damage Cause.</param>
    /// <param name="dealtHealthDamage">Damage Absorbed by HP.</param>
    /// <param name="absorbedAhpDamage">Damage Absorbed by AHP.</param>
    /// <returns>Real dealt damage, damage absorbed by AHP and damage absorbed by HP.</returns>
    public static float GetRealDamageAmount(this Player player, StandardDamageHandler handler, out float dealtHealthDamage, out float absorbedAhpDamage)
    {
        var tmp = player.GetStatModule<AhpStat>()._activeProcesses.Select(x => new { Process = x, x.CurrentAmount });
        var hp = player.Health;
        var damage = handler.Damage;
        handler.ApplyDamage(player.ReferenceHub);
        var realDamage = handler.Damage;
        absorbedAhpDamage = handler.AbsorbedAhpDamage;
        dealtHealthDamage = handler.DealtHealthDamage;
        handler.Damage = damage;
        player.Health = hp;
        foreach (var item in tmp)
            item.Process.CurrentAmount = item.CurrentAmount;
        return realDamage;
    }

    /// <summary>
    /// Returns real dealt damage to the player.
    /// </summary>
    /// <param name="player">Player.</param>
    /// <param name="handler">Damage Cause.</param>
    /// <returns>Real dealt damage.</returns>
    public static float GetRealDamageAmount(this Player player, StandardDamageHandler handler)
        => GetRealDamageAmount(player, handler, out _, out _);
}

public static class PlayerExtensions
{
    /// <summary>
    /// Converts player to string.
    /// </summary>
    /// <param name="me">Player.</param>
    /// <param name="userId">If userId should be shown.</param>
    /// <returns>String version of player.</returns>
    public static string ToString(this Player me, bool userId)
    {
        return userId
            ? $"({me.PlayerId}) {me.GetDisplayName()} | {me.UserId}"
            : $"({me.PlayerId}) {me.GetDisplayName()}";
    }

    /// <summary>
    /// Returns <see cref="Player.DisplayNickname"/> or <see cref="Player.Nickname"/> if first is null or "NULL" if player is null.
    /// </summary>
    /// <param name="player">Player.</param>
    /// <returns>Name.</returns>
    public static string GetDisplayName(this Player player)
        => player == null ? "NULL" : player.DisplayNickname ?? player.Nickname;

    public static string FormatUserId(this Player player)
    {
        if (player is null)
            return "NONE";

        var split = player.UserId.Split('@');

        return split[1] switch
        {
            "steam" => $"[{player.Nickname}](https://steamcommunity.com/profiles/{split[0]}) ({player.UserId})",
            "discord" => $"{player.Nickname} (<@{split[0]}>) ({player.UserId})",
            "server" => "Server",
            _ => player.UserId
        };
    }
}