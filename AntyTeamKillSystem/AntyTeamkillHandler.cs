// -----------------------------------------------------------------------
// <copyright file="AntyTeamkillHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.RoundLogger;

namespace Mistaken.AntyTeamKillSystem
{
    internal class AntyTeamkillHandler : Module
    {
        public static readonly (Team Attacker, Team Victim)[] TeamKillTeams = new (Team, Team)[]
        {
            (Team.CHI, Team.CHI),
            (Team.CHI, Team.CDP),
            (Team.CDP, Team.CHI),
            (Team.MTF, Team.MTF),
            (Team.MTF, Team.RSC),
            (Team.RSC, Team.MTF),
            (Team.RSC, Team.RSC),
            (Team.SCP, Team.SCP),
        };

        public static readonly Dictionary<Player, (Team Team, RoleType Role)> LastDead = new Dictionary<Player, (Team Team, RoleType Role)>();

        public static AntyTeamkillHandler Instance { get; private set; }

        public static bool IsTeamKill(Player attacker, Player victim, Team? attackerTeam = null)
            => attacker != Server.Host && attacker != null && attacker != victim && IsTeamKill(attackerTeam ?? attacker.Role.Team, victim.Role.Team);

        public static bool IsTeamKill(Team attackerTeam, Team victimTeam)
            => TeamKillTeams.Any(x => x.Attacker == attackerTeam && x.Victim == victimTeam);

        public AntyTeamkillHandler(PluginHandler p)
           : base(p)
        {
            Instance = this;
        }

        public override string Name => "AntyTeamKillHandler";

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Map_ExplodingGrenade;
            Exiled.Events.Handlers.Player.Destroying += this.Player_Destroying;
            Exiled.Events.Handlers.Scp079.InteractingTesla += this.Scp079_InteractingTesla;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Map_ExplodingGrenade;
            Exiled.Events.Handlers.Player.Destroying -= this.Player_Destroying;
            Exiled.Events.Handlers.Scp079.InteractingTesla -= this.Scp079_InteractingTesla;
        }

        internal static void OnTeamKill(TeamKill teamKill)
        {
            RLogger.Log("Anty TeamKill System", "DETECT TK", $"{teamKill.Attacker.PlayerToString()} TeamKilled {teamKill.Victim.PlayerToString()}. Detection Code: {teamKill.DetectionCode}");

            if (teamKill.Attacker.IsConnected)
            {
                if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamKillAttackerConsoleMessage))
                    teamKill.Attacker.SendConsoleMessage(PluginHandler.Instance.Translation.TeamKillAttackerConsoleMessage.Replace("\\n", "\n").Replace("{VictimName}", teamKill.Victim.GetDisplayName()).Replace("{TeamKillInfo}", teamKill.ToString()), "red");

                if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamKillAttackerBroadcast))
                    teamKill.Attacker.Broadcast(5, PluginHandler.Instance.Translation.TeamKillAttackerBroadcast.Replace("\\n", "\n").Replace("{VictimName}", teamKill.Victim.GetDisplayName()), shouldClearPrevious: true);
            }

            if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamKillVictimConsoleMessage))
                teamKill.Victim.SendConsoleMessage(PluginHandler.Instance.Translation.TeamKillVictimConsoleMessage.Replace("\\n", "\n").Replace("{AttackerName}", teamKill.Attacker.GetDisplayName()).Replace("{TeamKillInfo}", teamKill.ToString()), "yellow");

            if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamKillVictimBroadcast))
                teamKill.Victim.Broadcast(5, PluginHandler.Instance.Translation.TeamKillVictimBroadcast.Replace("\\n", "\n").Replace("{AttackerName}", teamKill.Attacker.GetDisplayName()), shouldClearPrevious: true);

            Instance.PunishPlayer(teamKill.Attacker, teamKill.Handler.Type == Exiled.API.Enums.DamageType.Explosion);
            PluginHandler.InvokeOnTeamKill(teamKill);
        }

        internal static void OnTeamAttack(TeamAttack teamAttack)
        {
            RLogger.Log("Anty TeamKill System", "DETECT TA", $"{teamAttack.Attacker.PlayerToString()} TeamAttacked {teamAttack.Victim.PlayerToString()}. Detection Code: {teamAttack.DetectionCode}");

            if (teamAttack.Attacker.IsConnected)
            {
                if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamAttackAttackerConsoleMessage))
                    teamAttack.Attacker.SendConsoleMessage(PluginHandler.Instance.Translation.TeamAttackAttackerConsoleMessage.Replace("\\n", "\n").Replace("{VictimName}", teamAttack.Victim.GetDisplayName()).Replace("{TeamAttackInfo}", teamAttack.ToString()), "yellow");

                if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamAttackAttackerBroadcast))
                    teamAttack.Attacker.Broadcast(1, PluginHandler.Instance.Translation.TeamAttackAttackerBroadcast.Replace("\\n", "\n").Replace("{VictimName}", teamAttack.Victim.GetDisplayName()), shouldClearPrevious: true);
            }

            if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamAttackVictimConsoleMessage))
                teamAttack.Victim.SendConsoleMessage(PluginHandler.Instance.Translation.TeamAttackVictimConsoleMessage.Replace("\\n", "\n").Replace("{AttackerName}", teamAttack.Attacker.GetDisplayName()).Replace("{TeamAttackInfo}", teamAttack.ToString()), "yellow");

            if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.TeamAttackVictimBroadcast))
                teamAttack.Victim.Broadcast(1, PluginHandler.Instance.Translation.TeamAttackVictimBroadcast.Replace("\\n", "\n").Replace("{AttackerName}", teamAttack.Attacker.GetDisplayName()), shouldClearPrevious: true);

            PluginHandler.InvokeOnTeamAttack(teamAttack);
        }

        private readonly Dictionary<Exiled.API.Features.TeslaGate, Player> teslaTrigger = new Dictionary<Exiled.API.Features.TeslaGate, Player>();
        private readonly Dictionary<string, string> leftPlayersIPs = new Dictionary<string, string>();
        private readonly Dictionary<string, Player> leftPlayers = new Dictionary<string, Player>();
        private readonly Dictionary<Player, (Player Thrower, string ThrowerUserId, Team ThrowerTeam)> grenadeAttacks = new Dictionary<Player, (Player Thrower, string ThrowerUserId, Team ThrowerTeam)>();
        private readonly HashSet<Player> delayedPunishPlayers = new HashSet<Player>();

        private void Player_Destroying(Exiled.Events.EventArgs.DestroyingEventArgs ev)
        {
            try
            {
                if (!Round.IsStarted)
                    return;
            }
            catch
            {
                return;
            }

            this.leftPlayers[ev.Player.UserId] = ev.Player;
            this.leftPlayersIPs[ev.Player.UserId] = ev.Player.IPAddress;
        }

        private void Scp079_InteractingTesla(Exiled.Events.EventArgs.InteractingTeslaEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                var tesla = ev.Tesla;
                this.teslaTrigger[tesla] = ev.Player;
                this.CallDelayed(
                    1,
                    () => this.teslaTrigger.Remove(tesla),
                    "RemoveTeslaLate");
            }
        }

        private void Server_RestartingRound()
        {
            this.grenadeAttacks.Clear();
            this.leftPlayers.Clear();
            this.leftPlayersIPs.Clear();
            LastDead.Clear();
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                this.Log.Debug("Skip Code: 3.4", PluginHandler.Instance.Config.VerbouseOutput);
                return; // Skip Code: 3.4
            }

            if (ev.GrenadeType == Exiled.API.Enums.GrenadeType.Flashbang)
            {
                if (ev.Thrower == null)
                    return;

                string victims = string.Empty;
                foreach (var victim in ev.TargetsToAffect.ToArray())
                {
                    if (IsTeamKill(ev.Thrower, victim))
                    {
                        if (!string.IsNullOrEmpty(PluginHandler.Instance.Translation.FlashedTeammateVictimBroadcast))
                            victim.Broadcast(5, PluginHandler.Instance.Translation.FlashedTeammateVictimBroadcast.Replace("{AttackerName}", ev.Thrower.GetDisplayName()), shouldClearPrevious: true);
                        if (!string.IsNullOrEmpty(PluginHandler.Instance.Translation.FlashedTeammateVictimConsoleMessage))
                            victim.SendConsoleMessage(PluginHandler.Instance.Translation.FlashedTeammateVictimConsoleMessage.Replace("{AttackerName}", ev.Thrower.GetDisplayName()), "yellow");
                        victims += $"{victim.GetDisplayName()}\n";
                    }
                }

                if (!string.IsNullOrEmpty(victims))
                {
                    if (!string.IsNullOrEmpty(PluginHandler.Instance.Translation.FlashedTeammateAttackerBroadcast))
                        ev.Thrower.Broadcast(5, PluginHandler.Instance.Translation.FlashedTeammateAttackerBroadcast, shouldClearPrevious: true);
                    if (!string.IsNullOrEmpty(PluginHandler.Instance.Translation.FlashedTeammateAttackerConsoleMessage))
                        ev.Thrower.SendConsoleMessage(PluginHandler.Instance.Translation.FlashedTeammateAttackerConsoleMessage.Replace("{VictimName}", victims), "yellow");
                }
            }

            if (ev.GrenadeType != Exiled.API.Enums.GrenadeType.FragGrenade)
            {
                this.Log.Debug("Skip Code: 3.5", PluginHandler.Instance.Config.VerbouseOutput);
                return; // Skip Code: 3.5
            }

            var thrower = ev.Thrower;
            var throwerUserId = ev.Thrower?.UserId;
            var throwerTeam = ev.Thrower?.Role.Team ?? Team.RIP;
            if (ev.Thrower == null || ev.Thrower == Server.Host)
            {
                var grenade = ev.Grenade.GetComponent<InventorySystem.Items.ThrowableProjectiles.ExplosionGrenade>();
                throwerTeam = grenade.PreviousOwner.Role.GetTeam();
                if (!grenade.PreviousOwner.IsSet)
                {
                    RLogger.Log("Anty TeamKill System", "SKIP GRENADE", $"Skip Code: 3.6 | Thrown by null");
                    return; // Skip Code: 3.6
                }

                try
                {
                    throwerUserId = grenade.PreviousOwner.LogUserID;
                }
                catch (System.Exception ex)
                {
                    this.Log.Error("Error Code: 3.9");
                    this.Log.Error(grenade.PreviousOwner.Nickname);
                    this.Log.Error(ex.Message);
                    this.Log.Error(ex.StackTrace);
                }

                if (string.IsNullOrEmpty(throwerUserId))
                {
                    RLogger.Log("Anty TeamKill System", "SKIP GRENADE", $"Skip Code: 3.10 | Thrower userid was null");
                    return; // Skip Code: 3.10
                }

                thrower = Player.Get(throwerUserId);
                if (thrower == null)
                {
                    if (!this.leftPlayers.TryGetValue(throwerUserId, out thrower))
                    {
                        RLogger.Log("Anty TeamKill System", "SKIP GRENADE", $"Skip Code: 3.8 | Thrower left server and was not logged");
                        return; // Skip Code: 3.8
                    }

                    this.Log.Debug("Status Code: 3.7", PluginHandler.Instance.Config.VerbouseOutput);
                }
            }

            HashSet<Player> friendlies = new HashSet<Player>();
            var targets = ev.TargetsToAffect.ToArray();
            foreach (var target in targets)
            {
                this.grenadeAttacks[target] = (thrower, throwerUserId, throwerTeam);
                if (IsTeamKill(thrower, target, throwerTeam) || thrower == target)
                    friendlies.Add(target);
            }

            MEC.Timing.CallDelayed(0.1f, () =>
            {
                int tks = 0;
                foreach (var target in targets)
                {
                    if (this.grenadeAttacks.TryGetValue(target, out var attacker) && attacker.Thrower == thrower)
                        this.grenadeAttacks.Remove(target);
                }

                foreach (var target in targets)
                {
                    if (!friendlies.Contains(target))
                    {
                        tks = int.MinValue;
                        break;
                    }

                    if (target.IsDead)
                        tks++;
                }

                if (tks > 3)
                {
                    RLogger.Log("Anty TeamKill System", "MASS TK", $"Detected Mass TeamKill ({tks} players), Respawning ...");
                    if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.MassTKGlobalBroadcast))
                        MapPlus.Broadcast("Anty TeamKill System", 5, PluginHandler.Instance.Translation.MassTKGlobalBroadcast.Replace("\\n", "\n").Replace("{TKCount}", tks.ToString()).Replace("{Code}", "5.4"));
                    foreach (var player in friendlies.Where(x => x != thrower))
                    {
                        if (!player.IsDead)
                        {
                            this.Log.Debug($"Skip Code: 5.2 ({player.Role})", PluginHandler.Instance.Config.VerbouseOutput);
                            continue;
                        }

                        if (!LastDead.TryGetValue(player, out var playerInfo))
                        {
                            this.Log.Warn($"Error Code: 5.1 ({player.ToString(true)})");
                            if (!string.IsNullOrWhiteSpace(PluginHandler.Instance.Translation.Error51ConsoleMessage))
                                player.SendConsoleMessage(PluginHandler.Instance.Translation.Error51ConsoleMessage.Replace("\\n", "\n"), "red");
                            continue;
                        }

                        player.Role.Type = playerInfo.Role;
                    }
                }
                else
                    this.Log.Debug($"Skip Code: 5.0 ({tks})", PluginHandler.Instance.Config.VerbouseOutput);
            });
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                this.Log.Debug("Skip Code: 2.4", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.4
            }

            if (!ev.Target.IsReadyPlayer())
            {
                this.Log.Debug("Skip Code: 2.0", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.0
            }

            if (ev.Target.IsGodModeEnabled)
            {
                this.Log.Debug("Skip Code: 2.6", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.6
            }

            /*if (ev.Attacker == Server.Host)
            {
                this.Log.Debug("Skip Code: 2.7", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.7
            }*/

            if (ev.Attacker is null)
            {
                this.Log.Debug("Skip Code: 2.8", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.8
            }

            if (IsTeamKill(ev.Attacker, ev.Target))
            {
                // TeamAttack
                // ExecuteCode: 2.2
                TeamAttack.Create(ev, "2.2");
                return;
            }
            else if (LastDead.TryGetValue(ev.Attacker, out var attackerInfo) && IsTeamKill(ev.Attacker, ev.Target, attackerInfo.Team))
            {
                // TeamAttack but attacker already died
                // ExecuteCode: 2.3
                TeamAttack.Create(ev, "2.3", attackerInfo.Team);
                return;
            }
            else if (ev.Handler.Type == Exiled.API.Enums.DamageType.Explosion && this.grenadeAttacks.TryGetValue(ev.Target, out var grenadeAttacker))
            {
                if (IsTeamKill(grenadeAttacker.Thrower, ev.Target, grenadeAttacker.ThrowerTeam))
                {
                    // ExecuteCode: 2.5
                    TeamAttack.Create(ev, "2.5", grenadeAttacker.ThrowerTeam, grenadeAttacker.Thrower);
                }
                else
                {
                    // Not TeamAttack
                    // SkipCode: 2.4
                    RLogger.Log("Anty TeamKill System", "SKIP TA", $"Grenade Hurting was not detected as TeamAttack. Skip Code: 2.4");
                }

                return;
            }
            else if (ev.Handler.Type == Exiled.API.Enums.DamageType.Tesla)
            {
                foreach (var item in this.teslaTrigger)
                {
                    if (item.Key.PlayerInHurtRange(ev.Target))
                    {
                        if (IsTeamKill(item.Value, ev.Target, item.Value.Role.Team))
                        {
                            // ExecuteCode: 2.9
                            TeamAttack.Create(ev, "2.5", attackerInfo.Team, item.Value);
                        }
                        else
                        {
                            // Not TeamAttack
                            // SkipCode: 2.10
                            RLogger.Log("Anty TeamKill System", "SKIP TA", $"Tesla Hurting was not detected as TeamAttack. Skip Code: 2.10");
                        }

                        return;
                    }
                }
            }

            // Not TeamAttack
            // SkipCode: 2.1
            RLogger.Log("Anty TeamKill System", "SKIP TA", $"Hurting was not detected as TeamAttack. Skip Code: 2.1");
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                this.Log.Debug("Skip Code: 1.4", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.4
            }

            if (!ev.Target.IsReadyPlayer())
            {
                this.Log.Debug("Skip Code: 1.0", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.0
            }

            if (!LastDead.ContainsKey(ev.Target))
            {
                LastDead.Add(ev.Target, (ev.Target.Role.Team, ev.Target.Role));
                this.CallDelayed(10, () => LastDead.Remove(ev.Target), "RemoveLastDead");
            }

            /*if (ev.Killer == Server.Host)
            {
                this.Log.Debug("Skip Code: 1.7", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.7
            }*/

            if (ev.Killer is null)
            {
                this.Log.Debug("Skip Code: 1.8", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.8
            }

            if (IsTeamKill(ev.Killer, ev.Target))
            {
                // TeamKill -> Punish Player
                // ExecuteCode: 1.2
                TeamKill.Create(ev, "1.2");

                return;
            }
            else if (LastDead.TryGetValue(ev.Killer, out var attackerInfo) && IsTeamKill(ev.Killer, ev.Target, attackerInfo.Team))
            {
                // TeamKill but killer already died -> Punish Player
                // ExecuteCode: 1.3
                TeamKill.Create(ev, "1.3", attackerInfo.Team);

                return;
            }
            else if (ev.Handler.Type == Exiled.API.Enums.DamageType.Explosion && this.grenadeAttacks.TryGetValue(ev.Target, out var grenadeAttacker))
            {
                if (IsTeamKill(grenadeAttacker.Thrower, ev.Target, grenadeAttacker.ThrowerTeam))
                {
                    // ExecuteCode: 1.5
                    TeamKill.Create(ev, "1.5", grenadeAttacker.ThrowerTeam, grenadeAttacker.Thrower);
                }
                else
                {
                    // Not TeamKill
                    // SkipCode: 1.4
                    RLogger.Log("Anty TeamKill System", "SKIP TK", $"Grenade Death was not detected as TeamKill. Skip Code: 1.4");
                }

                return;
            }
            else if (ev.Handler.Type == Exiled.API.Enums.DamageType.Tesla)
            {
                foreach (var item in this.teslaTrigger)
                {
                    if (item.Key.PlayerInHurtRange(ev.Target))
                    {
                        if (IsTeamKill(item.Value, ev.Target, item.Value.Role.Team))
                        {
                            // ExecuteCode: 1.9
                            TeamKill.Create(ev, "1.9", attackerInfo.Team, item.Value);
                        }
                        else
                        {
                            // Not TeamAttack
                            // SkipCode: 1.10
                            RLogger.Log("Anty TeamKill System", "SKIP TK", $"Tesla kill was not detected as TeamKill. Skip Code: 1.10");
                        }

                        return;
                    }
                }
            }

            // Not TeamKill
            // SkipCode: 1.1
            RLogger.Log("Anty TeamKill System", "SKIP TK", $"Death was not detected as TeamKill. Skip Code: 1.1");
        }

        private void PunishPlayer(Player player, bool grenade)
        {
            if (player.CheckPermission("ATKS.PunishBlock"))
            {
                this.Log.Debug("Skip Code: 4.4", PluginHandler.Instance.Config.VerbouseOutput);
                return; // Skip Code: 4.4
            }

            if (this.delayedPunishPlayers.Contains(player))
            {
                this.Log.Debug("Skip Code: 4.0", PluginHandler.Instance.Config.VerbouseOutput);
                return; // Skip Code: 4.0
            }

            this.delayedPunishPlayers.Add(player);
            MEC.Timing.CallDelayed(grenade ? 2 : 8, () =>
            {
                var tks = TeamKill.TeamKills[RoundPlus.RoundId].Where(x => x.Attacker.UserId == player.UserId).Count();

                RLogger.Log("Anty TeamKill System", "PUNISH", $"Punishing {player.PlayerToString()} for TeamKilling {tks} players");

                object[] rawBanInfo = null;
                if (!PluginHandler.Instance.Config.BanLevels.TryGetValue(tks, out rawBanInfo))
                {
                    for (int i = tks; i > 0; i--)
                    {
                        if (PluginHandler.Instance.Config.BanLevels.TryGetValue(i, out rawBanInfo))
                            break;
                    }
                }

                if (rawBanInfo != null)
                {
                    int duration;
                    string message;
                    try
                    {
                        message = (string)rawBanInfo[0];
                        duration = int.Parse((string)rawBanInfo[1]);
                    }
                    catch (System.InvalidCastException ex)
                    {
                        this.Log.Error("Error Code: 4.3");
                        this.Log.Error(rawBanInfo[0].GetType());
                        this.Log.Error(rawBanInfo[1].GetType());
                        this.Log.Error(ex.Message);
                        this.Log.Error(ex.StackTrace);
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        this.Log.Error("Error Code: 4.4");
                        this.Log.Error(ex.Message);
                        this.Log.Error(ex.StackTrace);
                        return;
                    }

                    this.Log.Info($"Player {player.ToString(true)} has been {(duration == 0 ? "kicked" : "banned")} for teamkilling {tks} players.");
                    this.Ban(player, duration, message);
                }

                this.delayedPunishPlayers.Remove(player);
            });
        }

        private void Ban(Player player, long duration, string message)
        {
            duration *= 60; // Change from minutes to seconds

            if (player.IsConnected)
            {
                player.Ban((int)duration, message, "Anty TeamKill System");
            }
            else
            {
                string nickname = player.Nickname;
                int maxLength = GameCore.ConfigFile.ServerConfig.GetInt("ban_nickname_maxlength", 30);
                bool trimUnicode = GameCore.ConfigFile.ServerConfig.GetBool("ban_nickname_trimunicode", true);
                nickname = string.IsNullOrEmpty(nickname) ? "(no nick)" : nickname;
                if (trimUnicode)
                    nickname = NorthwoodLib.StringUtils.StripUnicodeCharacters(nickname, string.Empty);

                if (nickname.Length > maxLength)
                    nickname = nickname.Substring(0, maxLength);

                this.Log.Debug("Status Code: 4.2", PluginHandler.Instance.Config.VerbouseOutput);
                this.Log.Info($"Player ({nickname}) left before ban, banning offline");

                var ev = new Exiled.Events.EventArgs.BanningEventArgs(player, Server.Host, duration, message, message, true);
                Exiled.Events.Handlers.Player.OnBanning(ev);
                if (!ev.IsAllowed)
                {
                    this.Log.Debug("Skip Code: 4.1", PluginHandler.Instance.Config.VerbouseOutput);
                    return; // Skip Code: 4.1
                }

                duration = ev.Duration;
                message = ev.Reason;

                long issuanceTime = global::TimeBehaviour.CurrentTimestamp();
                long banExpirationTime = global::TimeBehaviour.GetBanExpirationTime((uint)duration);
                var userIdBan = new BanDetails
                {
                    Id = player.UserId,
                    Issuer = "Anty TeamKill System",
                    OriginalName = nickname,
                    Reason = message,
                    IssuanceTime = issuanceTime,
                    Expires = banExpirationTime,
                };
                BanHandler.IssueBan(userIdBan, BanHandler.BanType.UserId);
                this.Log.Info($"Offline banned {player.UserId}");
                Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, userIdBan, BanHandler.BanType.UserId));

                if (this.leftPlayersIPs.TryGetValue(player.UserId, out string ip))
                {
                    var ipBan = new BanDetails
                    {
                        Id = ip,
                        Issuer = "Anty TeamKill System",
                        OriginalName = nickname,
                        Reason = message,
                        IssuanceTime = issuanceTime,
                        Expires = banExpirationTime,
                    };
                    BanHandler.IssueBan(ipBan, BanHandler.BanType.IP);
                    this.Log.Info($"Offline banned {ip}");
                    Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, ipBan, BanHandler.BanType.IP));
                }
            }
        }
    }
}
