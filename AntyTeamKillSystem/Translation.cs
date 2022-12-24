// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Mistaken.AntyTeamKillSystem;

internal sealed class Translation
{
    /// <summary>
    /// Gets or sets pattern how to display <see cref="TeamKill"/>.
    /// </summary>
    [Description("Defines how TeamKill info is displayed in client console")]
    public string[] TeamKillInfo { get; set; } = {
        "Attacker: {Attacker}",
        "Attacker Team: {AttackerTeam}",
        "Victim: {Victim}",
        "Victim Team: {VictimTeam}",
        "Used Tool: {Tool}",
        "Detection Code: {DetectionCode}",
    };

    /// <summary>
    /// Gets or sets pattern how to display <see cref="TeamAttack"/>.
    /// </summary>
    [Description("Defines how TeamAttack info is displayed in client console")]
    public string[] TeamAttackInfo { get; set; } = {
        "Attacker: {Attacker}",
        "Attacker Team: {AttackerTeam}",
        "Victim: {Victim}",
        "Victim Team: {VictimTeam}",
        "Used Tool: {Tool}",
        "Done Damage: {Amount}",
        "Detection Code: {DetectionCode}",
    };

    /// <summary>
    /// Gets or sets translation of word "Disconnected".
    /// </summary>
    [Description("Translation of word \"Disconnected\"")]
    public string Disconnected { get; set; } = "DISCONNECTED";

    /// <summary>
    /// Gets or sets broadcast message sent to TeamAttack Victim (Set empty to disable).
    /// </summary>
    [Description("Broadcast sent to TeamAttack Victim (Set empty to disable)")]
    public string TeamAttackVictimBroadcast { get; set; } = "<color=yellow>You have <b>been</b> TeamAttacked by {AttackerName}</color>";

    /// <summary>
    /// Gets or sets broadcast message sent to TeamAttack Attacker (Set empty to disable).
    /// </summary>
    [Description("Broadcast sent to TeamAttack Attacker (Set empty to disable)")]
    public string TeamAttackAttackerBroadcast { get; set; } = "<color=yellow>You have <b>TeamAttacked</b> {VictimName}</color>\\n<size=50%><color=red><b>This will not be tolerated</b></color></size>";

    /// <summary>
    /// Gets or sets console message sent to TeamAttack Victim (Set empty to disable).
    /// </summary>
    [Description("Console message sent to TeamAttack Victim (Set empty to disable)")]
    public string TeamAttackVictimConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] You have <b>been</b> TeamAttacked by {AttackerName}\\n{TeamAttackInfo}";

    /// <summary>
    /// Gets or sets console message sent to TeamAttack Attacker (Set empty to disable).
    /// </summary>
    [Description("Console message sent to TeamAttack Attacker (Set empty to disable)")]
    public string TeamAttackAttackerConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] You have TeamAttacked {VictimName}\\n{TeamAttackInfo}";

    /// <summary>
    /// Gets or sets broadcast message sent to TeamKill Victim (Set empty to disable).
    /// </summary>
    [Description("Broadcast sent to TeamKill Victim (Set empty to disable)")]
    public string TeamKillVictimBroadcast { get; set; } = "<color=yellow>You have <b>been</b> TeamKilled by {AttackerName}</color>";

    /// <summary>
    /// Gets or sets broadcast message sent to TeamKill Attacker (Set empty to disable).
    /// </summary>
    [Description("Broadcast sent to TeamKill Attacker (Set empty to disable)")]
    public string TeamKillAttackerBroadcast { get; set; } = "<color=red>You have <b>TeamKilled</b> {VictimName}</color>\\n<color=red><b>This will not be tolerated</b></color>";

    /// <summary>
    /// Gets or sets console message sent to TeamKill Victim (Set empty to disable).
    /// </summary>
    [Description("Console message sent to TeamKill Victim (Set empty to disable)")]
    public string TeamKillVictimConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] You have <b>been</b> TeamKilled by {AttackerName}\\n{TeamKillInfo}";

    /// <summary>
    /// Gets or sets console message sent to TeamKill Attacker (Set empty to disable).
    /// </summary>
    [Description("Console message sent to TeamKill Attacker (Set empty to disable)")]
    public string TeamKillAttackerConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] You have TeamKilled {VictimName}\\n{TeamKillInfo}";

    /// <summary>
    /// Gets or sets broadcast message sent globaly when MassTK is detected (Set empty to disable).
    /// </summary>
    [Description("Broadcast sent globaly when MassTK is detected (Set empty to disable)")]
    public string MassTKGlobalBroadcast { get; set; } = "Detected MassTK, respawning {TKCount} players ...";

    /// <summary>
    /// Gets or sets console message sent to user when plugin can't find previous role (Set empty to disable).
    /// </summary>
    [Description("Console message sent to user when plugin can't find previous role (Set empty to disable)")]
    public string Error51ConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] Failed to find role before death (Error Code: 5.1)";

    /// <summary>
    /// Gets or sets flashed victim broadcast displayed to user when the player is flashed by his teammate (Set empty to disable).
    /// </summary>
    [Description("Flashed victim broadcast displayed to user when the player is flashed by his teammate (Set empty to disable)")]
    public string FlashedTeammateVictimBroadcast { get; set; } = "<color=yellow>You have <b>been</b> flashed by {AttackerName}</color>";

    /// <summary>
    /// Gets or sets flashed attacker broadcast displayed to user when the player flashed his teammates (Set empty to disable).
    /// </summary>
    [Description("Flashed victim broadcast displayed to user when the player is flashed by his teammate (Set empty to disable)")]
    public string FlashedTeammateAttackerBroadcast { get; set; } = "<color=yellow>You have <b>flashed</b> a teammate</color>";

    /// <summary>
    /// Gets or sets console message sent to flashed teammate Victim (Set empty to disable).
    /// </summary>
    [Description("Console message sent to flashed teammate Victim (Set empty to disable)")]
    public string FlashedTeammateVictimConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] You have <b>been</b> flashed by {AttackerName}";

    /// <summary>
    /// Gets or sets console message sent to flashed teammate Attacker (Set empty to disable).
    /// </summary>
    [Description("Console message sent to flashed teammate Attacker (Set empty to disable)")]
    public string FlashedTeammateAttackerConsoleMessage { get; set; } = "[<b>Anty TeamKill System</b>] You have flashed a teammate: {VictimName}";
}