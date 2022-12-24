// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Mistaken.AntyTeamKillSystem;

/// <inheritdoc/>
public class Config
{
    /// <summary>
    /// Gets or sets dictionary containing bans for specific TeamKill counts.
    /// </summary>
    [Description("Bans bound to number of TeamKills")]
    public System.Collections.Generic.Dictionary<int, object[]> BanLevels { get; set; } = new System.Collections.Generic.Dictionary<int, object[]>()
    {
        { 3, new object[] { "TK: You have been automaticly kicked for TeamKilling too many times", 0 } },
        { 4, new object[] { "TK: You have been automaticly banned for 1 hour for TeamKilling too many times", 60 } },
        { 5, new object[] { "TK: You have been automaticly banned for 10 hours for TeamKilling too many times", 600 } },
        { 6, new object[] { "TK: You have been automaticly banned for 1 day for TeamKilling too many times", 1440 } },
        { 7, new object[] { "TK: You have been automaticly banned for 2 days for TeamKilling too many times", 2880 } },
        { 10, new object[] { "TK: You have been automaticly banned for 1 month for TeamKilling too many times", 43200 } },
    };

    /// <summary>
    /// Gets or sets time after which the teamkill is no longer counted when punishing the player.
    /// </summary>
    [Description("Time (in seconds) after which the teamkill is no longer counted when punishing the player. (Set to 0 to disable)")]
    public int TeamkillPunishmentInvalidateTime { get; set; } = 180;

    /// <summary>
    /// Gets or sets a value indicating whether debug should be displayed.
    /// </summary>
    [Description("If true then debug will be displayed")]
    public bool VerbouseOutput { get; set; }
}