// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace Mistaken.AntyTeamKillSystem
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config, Translation>
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
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "AntyTeamKillSystem";

        /// <inheritdoc/>
        public override string Prefix => "MATKS";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Low;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(4, 1, 2);

#pragma warning disable SA1202 // Elements should be ordered by access
        private Version version;

        /// <inheritdoc/>
        public override Version Version
        {
            get
            {
                if (this.version == null)
                    this.version = this.Assembly.GetName().Version;
                return this.version;
            }
        }
#pragma warning restore SA1202 // Elements should be ordered by access

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            new AntyTeamkillHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        internal static void InvokeOnTeamKill(TeamKill ev)
            => OnTeamKill?.Invoke(ev);

        internal static void InvokeOnTeamAttack(TeamAttack ev)
            => OnTeamAttack?.Invoke(ev);
    }
}
