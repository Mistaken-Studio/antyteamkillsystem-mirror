// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Mistaken.Updater.Config;

namespace Mistaken.AntyTeamKillSystem
{
    /// <inheritdoc/>
    public class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

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
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public System.Collections.Generic.Dictionary<string, string> AutoUpdateConfig { get; set; } = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Url", "https://git.mistaken.pl/api/v4/projects/27" },
            { "Token", string.Empty },
            { "Type", "GITLAB" },
            { "VerbouseOutput", "false" },
        };
    }
}
