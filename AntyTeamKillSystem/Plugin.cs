// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace Mistaken.AntyTeamKillSystem
{
    /// <inheritdoc/>
    public sealed class Plugin
    {
        /// <summary>
        /// Event called when TeamKill happens.
        /// </summary>
        public static event Action<TeamKill> OnTeamKill;

        /// <summary>
        /// Event called when TeamAttack happens.
        /// </summary>
        public static event Action<TeamAttack> OnTeamAttack;

        /// <inheritdoc/>
        [PluginPriority(LoadPriority.Low + 1)]
        [PluginEntryPoint("AntyTeamKillSystem", "3.0.0", "", "Mistaken Devs")]
        public void OnEnabled()
        {
            Instance = this;
            EventManager.RegisterEvents<AntyTeamkillHandler>(this);
        }

        /// <inheritdoc/>
        [PluginUnload]
        public void OnDisabled()
        {
            EventManager.UnregisterEvents<AntyTeamkillHandler>(this);
        }

        internal static Plugin Instance { get; private set; }

        [PluginConfig] public Config Config;

        internal Translation Translation = new(); // Todo: add missing translations

        internal static void InvokeOnTeamKill(TeamKill ev)
            => OnTeamKill?.Invoke(ev);

        internal static void InvokeOnTeamAttack(TeamAttack ev)
            => OnTeamAttack?.Invoke(ev);
    }
}
