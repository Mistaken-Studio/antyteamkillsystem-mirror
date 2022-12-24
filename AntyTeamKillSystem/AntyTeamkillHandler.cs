// -----------------------------------------------------------------------
// <copyright file="AntyTeamkillHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace Mistaken.AntyTeamKillSystem
{
    internal class AntyTeamkillHandler
    {
        public static readonly (Team Attacker, Team Victim)[] TeamKillTeams = {
            (Team.ChaosInsurgency, Team.ChaosInsurgency),
            (Team.ChaosInsurgency, Team.ClassD),
            (Team.ClassD, Team.ChaosInsurgency),
            (Team.FoundationForces, Team.FoundationForces),
            (Team.FoundationForces , Team.Scientists),
            (Team.Scientists, Team.FoundationForces),
            (Team.Scientists, Team.Scientists),
            (Team.SCPs, Team.SCPs),
        };

        public static readonly Dictionary<Player, (Team Team, RoleTypeId Role)> LastDead = new();

        public static AntyTeamkillHandler Instance { get; private set; }

        public static bool IsTeamKill(Player attacker, Player victim, Team? attackerTeam = null)
            => attacker != Server.Instance && attacker != null && attacker != victim && IsTeamKill(attackerTeam ?? attacker.Role.GetTeam(), victim.Role.GetTeam());

        public static bool IsTeamKill(Team attackerTeam, Team victimTeam)
            => TeamKillTeams.Any(x => x.Attacker == attackerTeam && x.Victim == victimTeam);

        public AntyTeamkillHandler()
        {
            Instance = this;
        }

        internal static void OnTeamKill(TeamKill teamKill)
        {
            // RLogger.Log("Anty TeamKill System", "DETECT TK", $"{teamKill.Attacker.PlayerToString()} TeamKilled {teamKill.Victim.PlayerToString()}. Detection Code: {teamKill.DetectionCode}");

            if (teamKill.Attacker.Connection?.isAuthenticated ?? false)
            {
                if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamKillAttackerConsoleMessage))
                    teamKill.Attacker.SendConsoleMessage(Plugin.Instance.Translation.TeamKillAttackerConsoleMessage.Replace("\\n", "\n").Replace("{VictimName}", teamKill.Victim.GetDisplayName()).Replace("{TeamKillInfo}", teamKill.ToString()), "red");

                if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamKillAttackerBroadcast))
                    teamKill.Attacker.SendBroadcast(Plugin.Instance.Translation.TeamKillAttackerBroadcast.Replace("\\n", "\n").Replace("{VictimName}", teamKill.Victim.GetDisplayName()), 5, shouldClearPrevious: true);
            }

            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamKillVictimConsoleMessage))
                teamKill.Victim.SendConsoleMessage(Plugin.Instance.Translation.TeamKillVictimConsoleMessage.Replace("\\n", "\n").Replace("{AttackerName}", teamKill.Attacker.GetDisplayName()).Replace("{TeamKillInfo}", teamKill.ToString()), "yellow");

            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamKillVictimBroadcast))
                teamKill.Victim.SendBroadcast(Plugin.Instance.Translation.TeamKillVictimBroadcast.Replace("\\n", "\n").Replace("{AttackerName}", teamKill.Attacker.GetDisplayName()), 5, shouldClearPrevious: true);

            Instance.PunishPlayer(teamKill.Attacker, teamKill.Handler is ExplosionDamageHandler);
            Plugin.InvokeOnTeamKill(teamKill);
        }

        internal static void OnTeamAttack(TeamAttack teamAttack)
        {
            // RLogger.Log("Anty TeamKill System", "DETECT TA", $"{teamAttack.Attacker.PlayerToString()} TeamAttacked {teamAttack.Victim.PlayerToString()}. Detection Code: {teamAttack.DetectionCode}");

            if (teamAttack.Attacker.Connection?.isAuthenticated ?? false)
            {
                if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamAttackAttackerConsoleMessage))
                    teamAttack.Attacker.SendConsoleMessage(Plugin.Instance.Translation.TeamAttackAttackerConsoleMessage.Replace("\\n", "\n").Replace("{VictimName}", teamAttack.Victim.GetDisplayName()).Replace("{TeamAttackInfo}", teamAttack.ToString()), "yellow");

                if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamAttackAttackerBroadcast))
                    teamAttack.Attacker.SendBroadcast(Plugin.Instance.Translation.TeamAttackAttackerBroadcast.Replace("\\n", "\n").Replace("{VictimName}", teamAttack.Victim.GetDisplayName()), 1, shouldClearPrevious: true);
            }

            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamAttackVictimConsoleMessage))
                teamAttack.Victim.SendConsoleMessage(Plugin.Instance.Translation.TeamAttackVictimConsoleMessage.Replace("\\n", "\n").Replace("{AttackerName}", teamAttack.Attacker.GetDisplayName()).Replace("{TeamAttackInfo}", teamAttack.ToString()), "yellow");

            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.TeamAttackVictimBroadcast))
                teamAttack.Victim.SendBroadcast(Plugin.Instance.Translation.TeamAttackVictimBroadcast.Replace("\\n", "\n").Replace("{AttackerName}", teamAttack.Attacker.GetDisplayName()), 1, shouldClearPrevious: true);

            Plugin.InvokeOnTeamAttack(teamAttack);
        }

        private readonly Dictionary<TeslaGate, Player> teslaTrigger = new();
        private readonly Dictionary<string, string> leftPlayersIPs = new();
        private readonly Dictionary<string, Player> leftPlayers = new();
        private readonly Dictionary<Player, (Player Thrower, string ThrowerUserId, Team ThrowerTeam)> grenadeAttacks = new();
        private readonly HashSet<Player> delayedPunishPlayers = new();

        [PluginEvent(ServerEventType.PlayerLeft)]
        private void Player_Destroying(Player player)
        {
            try
            {
                if (!Round.IsRoundStarted)
                    return;
            }
            catch
            {
                return;
            }

            leftPlayers[player.UserId] = player;
            leftPlayersIPs[player.UserId] = player.IpAddress;
        }

        /*private void Scp079_InteractingTesla(Exiled.Events.EventArgs.InteractingTeslaEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                var tesla = ev.Tesla;
                teslaTrigger[tesla] = ev.Player;
                this.CallDelayed(
                    1,
                    () => teslaTrigger.Remove(tesla),
                    "RemoveTeslaLate");
            }
        }*/

        internal static int RoundIdCounter;
        [PluginEvent(ServerEventType.RoundRestart)]
        private void Server_RestartingRound()
        {
            grenadeAttacks.Clear();
            leftPlayers.Clear();
            leftPlayersIPs.Clear();
            LastDead.Clear();

            RoundIdCounter++;
        }

        /*private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                Log.Debug("Skip Code: 3.4", Plugin.Instance.Config.VerbouseOutput);
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
                        if (!string.IsNullOrEmpty(Plugin.Instance.Translation.FlashedTeammateVictimBroadcast))
                            victim.Broadcast(5, Plugin.Instance.Translation.FlashedTeammateVictimBroadcast.Replace("{AttackerName}", ev.Thrower.GetDisplayName()), shouldClearPrevious: true);
                        if (!string.IsNullOrEmpty(Plugin.Instance.Translation.FlashedTeammateVictimConsoleMessage))
                            victim.SendConsoleMessage(Plugin.Instance.Translation.FlashedTeammateVictimConsoleMessage.Replace("{AttackerName}", ev.Thrower.GetDisplayName()), "yellow");
                        victims += $"{victim.GetDisplayName()}\n";
                    }
                }

                if (!string.IsNullOrEmpty(victims))
                {
                    if (!string.IsNullOrEmpty(Plugin.Instance.Translation.FlashedTeammateAttackerBroadcast))
                        ev.Thrower.Broadcast(5, Plugin.Instance.Translation.FlashedTeammateAttackerBroadcast, shouldClearPrevious: true);
                    if (!string.IsNullOrEmpty(Plugin.Instance.Translation.FlashedTeammateAttackerConsoleMessage))
                        ev.Thrower.SendConsoleMessage(Plugin.Instance.Translation.FlashedTeammateAttackerConsoleMessage.Replace("{VictimName}", victims), "yellow");
                }
            }

            if (ev.GrenadeType != Exiled.API.Enums.GrenadeType.FragGrenade)
            {
                Log.Debug("Skip Code: 3.5", Plugin.Instance.Config.VerbouseOutput);
                return; // Skip Code: 3.5
            }

            var thrower = ev.Thrower;
            var throwerUserId = ev.Thrower?.UserId;
            var throwerTeam = ev.Thrower?.Role.Team ?? Team.Dead;
            if (ev.Thrower == null || ev.Thrower == Server.Instance)
            {
                var grenade = ev.Grenade.GetComponent<InventorySystem.Items.ThrowableProjectiles.ExplosionGrenade>();
                throwerTeam = grenade.PreviousOwner.Role.GetTeam();
                if (!grenade.PreviousOwner.IsSet)
                {
                    // RLogger.Log("Anty TeamKill System", "SKIP GRENADE", $"Skip Code: 3.6 | Thrown by null");
                    return; // Skip Code: 3.6
                }

                try
                {
                    throwerUserId = grenade.PreviousOwner.LogUserID;
                }
                catch (Exception ex)
                {
                    Log.Error("Error Code: 3.9");
                    Log.Error(grenade.PreviousOwner.Nickname);
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }

                if (string.IsNullOrEmpty(throwerUserId))
                {
                    // RLogger.Log("Anty TeamKill System", "SKIP GRENADE", $"Skip Code: 3.10 | Thrower userid was null");
                    return; // Skip Code: 3.10
                }

                thrower = Player.Get(throwerUserId);
                if (thrower == null)
                {
                    if (!leftPlayers.TryGetValue(throwerUserId, out thrower))
                    {
                        // RLogger.Log("Anty TeamKill System", "SKIP GRENADE", $"Skip Code: 3.8 | Thrower left server and was not logged");
                        return; // Skip Code: 3.8
                    }

                    Log.Debug("Status Code: 3.7", Plugin.Instance.Config.VerbouseOutput);
                }
            }

            HashSet<Player> friendlies = new HashSet<Player>();
            var targets = ev.TargetsToAffect.ToArray();
            foreach (var target in targets)
            {
                grenadeAttacks[target] = (thrower, throwerUserId, throwerTeam);
                if (IsTeamKill(thrower, target, throwerTeam) || thrower == target)
                    friendlies.Add(target);
            }

            MEC.Timing.CallDelayed(0.1f, () =>
            {
                int tks = 0;
                foreach (var target in targets)
                {
                    if (grenadeAttacks.TryGetValue(target, out var attacker) && attacker.Thrower == thrower)
                        grenadeAttacks.Remove(target);
                }

                foreach (var target in targets)
                {
                    if (!friendlies.Contains(target))
                    {
                        tks = int.MinValue;
                        break;
                    }

                    if (!target.IsAlive)
                        tks++;
                }

                if (tks > 3)
                {
                    // RLogger.Log("Anty TeamKill System", "MASS TK", $"Detected Mass TeamKill ({tks} players), Respawning ...");
                    if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.MassTKGlobalBroadcast))
                        FromApi.Broadcast("Anty TeamKill System", 5, Plugin.Instance.Translation.MassTKGlobalBroadcast.Replace("\\n", "\n").Replace("{TKCount}", tks.ToString()).Replace("{Code}", "5.4"));
                    foreach (var player in friendlies.Where(x => x != thrower))
                    {
                        if (player.IsAlive)
                        {
                            Log.Debug($"Skip Code: 5.2 ({player.Role})", Plugin.Instance.Config.VerbouseOutput);
                            continue;
                        }

                        if (!LastDead.TryGetValue(player, out var playerInfo))
                        {
                            Log.Warning($"Error Code: 5.1 ({player.ToString(true)})");
                            if (!string.IsNullOrWhiteSpace(Plugin.Instance.Translation.Error51ConsoleMessage))
                                player.SendConsoleMessage(Plugin.Instance.Translation.Error51ConsoleMessage.Replace("\\n", "\n"), "red");
                            continue;
                        }

                        player.Role = playerInfo.Role;
                    }
                }
                else
                    Log.Debug($"Skip Code: 5.0 ({tks})", Plugin.Instance.Config.VerbouseOutput);
            });
        }*/

        [PluginEvent(ServerEventType.PlayerDamage)]
        private void Player_Hurting(Player attacker, Player victim, DamageHandlerBase handler)
        {
            /*if (!ev.IsAllowed)
            {
                Log.Debug("Skip Code: 2.4", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.4
            }*/

            /*if (!ev.Target.IsReadyPlayer())
            {
                Log.Debug("Skip Code: 2.0", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.0
            }*/

            if (victim.IsGodModeEnabled)
            {
                Log.Debug("Skip Code: 2.6", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.6
            }

            /*if (ev.Attacker == Server.Host)
            {
                this.Log.Debug("Skip Code: 2.7", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.7
            }*/

            if (attacker is null)
            {
                Log.Debug("Skip Code: 2.8", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 2.8
            }

            if (IsTeamKill(attacker, victim))
            {
                // TeamAttack
                // ExecuteCode: 2.2
                TeamAttack.Create(attacker, victim, handler, "2.2", attacker.Role.GetTeam());
                return;
            }
            else if (LastDead.TryGetValue(attacker, out var attackerInfo) && IsTeamKill(attacker, victim, attackerInfo.Team))
            {
                // TeamAttack but attacker already died
                // ExecuteCode: 2.3
                TeamAttack.Create(attacker, victim, handler, "2.3", attackerInfo.Team);
                return;
            }
            else if (handler is ExplosionDamageHandler && grenadeAttacks.TryGetValue(victim, out var grenadeAttacker))
            {
                if (IsTeamKill(grenadeAttacker.Thrower, victim, grenadeAttacker.ThrowerTeam))
                {
                    // ExecuteCode: 2.5
                    TeamAttack.Create(attacker, victim, handler, "2.5", grenadeAttacker.ThrowerTeam, grenadeAttacker.Thrower);
                }
                else
                {
                    // Not TeamAttack
                    // SkipCode: 2.4
                    // RLogger.Log("Anty TeamKill System", "SKIP TA", $"Grenade Hurting was not detected as TeamAttack. Skip Code: 2.4");
                }

                return;
            }
            /*else if (ev.Handler.Type == Exiled.API.Enums.DamageType.Tesla)
            {
                foreach (var item in teslaTrigger)
                {
                    if (item.Key.PlayerInHurtRange(ev.Target))
                    {
                        if (IsTeamKill(item.Value, ev.Target, item.Value.Role.GetTeam()))
                        {
                            // ExecuteCode: 2.9
                            TeamAttack.Create(ev, "2.5", attackerInfo.Team, item.Value);
                        }
                        else
                        {
                            // Not TeamAttack
                            // SkipCode: 2.10
                            // RLogger.Log("Anty TeamKill System", "SKIP TA", $"Tesla Hurting was not detected as TeamAttack. Skip Code: 2.10");
                        }

                        return;
                    }
                }
            }*/

            // Not TeamAttack
            // SkipCode: 2.1
            // RLogger.Log("Anty TeamKill System", "SKIP TA", $"Hurting was not detected as TeamAttack. Skip Code: 2.1");
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        private void Player_Dying(Player attacker, Player victim, DamageHandlerBase handler)
        {
            /*if (!ev.IsAllowed)
            {
                Log.Debug("Skip Code: 1.4", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.4
            }

            if (!ev.Target.IsReadyPlayer())
            {
                Log.Debug("Skip Code: 1.0", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.0
            }*/

            if (!LastDead.ContainsKey(victim))
            {
                LastDead.Add(victim, (victim.Role.GetTeam(), victim.Role));
                Timing.CallDelayed(10, () => LastDead.Remove(victim));
            }

            /*if (ev.Killer == Server.Host)
            {
                this.Log.Debug("Skip Code: 1.7", PluginHandler.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.7
            }*/

            if (attacker is null)
            {
                Log.Debug("Skip Code: 1.8", Plugin.Instance.Config.VerbouseOutput);
                return; // SkipCode: 1.8
            }

            if (IsTeamKill(attacker, victim))
            {
                // TeamKill -> Punish Player
                // ExecuteCode: 1.2
                TeamKill.Create(attacker, victim, handler, "1.2", attacker.Role.GetTeam());

                return;
            }
            else if (LastDead.TryGetValue(attacker, out var attackerInfo) && IsTeamKill(attacker, victim, attackerInfo.Team))
            {
                // TeamKill but killer already died -> Punish Player
                // ExecuteCode: 1.3
                TeamKill.Create(attacker, victim, handler, "1.3", attackerInfo.Team);

                return;
            }
            else if (handler is ExplosionDamageHandler && grenadeAttacks.TryGetValue(victim, out var grenadeAttacker))
            {
                if (IsTeamKill(grenadeAttacker.Thrower, victim, grenadeAttacker.ThrowerTeam))
                {
                    // ExecuteCode: 1.5
                    TeamKill.Create(attacker, victim, handler, "1.5", grenadeAttacker.ThrowerTeam, grenadeAttacker.Thrower);
                }
                else
                {
                    // Not TeamKill
                    // SkipCode: 1.4
                    // RLogger.Log("Anty TeamKill System", "SKIP TK", $"Grenade Death was not detected as TeamKill. Skip Code: 1.4");
                }

                return;
            }
            /*else if (ev.Handler.Type == Exiled.API.Enums.DamageType.Tesla)
            {
                foreach (var item in teslaTrigger)
                {
                    if (item.Key.PlayerInHurtRange(ev.Target))
                    {
                        if (IsTeamKill(item.Value, ev.Target, item.Value.Role.GetTeam()))
                        {
                            // ExecuteCode: 1.9
                            TeamKill.Create(ev, "1.9", attackerInfo.Team, item.Value);
                        }
                        else
                        {
                            // Not TeamAttack
                            // SkipCode: 1.10
                            // RLogger.Log("Anty TeamKill System", "SKIP TK", $"Tesla kill was not detected as TeamKill. Skip Code: 1.10");
                        }

                        return;
                    }
                }
            }*/

            // Not TeamKill
            // SkipCode: 1.1
            // RLogger.Log("Anty TeamKill System", "SKIP TK", $"Death was not detected as TeamKill. Skip Code: 1.1");
        }

        private void PunishPlayer(Player player, bool grenade)
        {
            /*if (Exiled.Permissions.Extensions.Permissions.CheckPermission(player, "ATKS.PunishBlock"))
            {
                Log.Debug("Skip Code: 4.4", Plugin.Instance.Config.VerbouseOutput);
                return; // Skip Code: 4.4
            }*/

            if (delayedPunishPlayers.Contains(player))
            {
                Log.Debug("Skip Code: 4.0", Plugin.Instance.Config.VerbouseOutput);
                return; // Skip Code: 4.0
            }

            delayedPunishPlayers.Add(player);
            MEC.Timing.CallDelayed(grenade ? 2 : 8, () =>
            {
                var tks = 0;
                foreach (var teamkill in TeamKill.TeamKills[RoundIdCounter])
                {
                    if (teamkill.Attacker.UserId != player.UserId)
                        continue;
                    if (Plugin.Instance.Config.TeamkillPunishmentInvalidateTime != 0 && (DateTime.Now - teamkill.Timestamp).TotalSeconds > Plugin.Instance.Config.TeamkillPunishmentInvalidateTime)
                        continue;
                    tks++;
                }

                // RLogger.Log("Anty TeamKill System", "PUNISH", $"Punishing {player.PlayerToString()} for TeamKilling {tks} players");

                object[] rawBanInfo = null;
                if (!Plugin.Instance.Config.BanLevels.TryGetValue(tks, out rawBanInfo))
                {
                    for (var i = tks; i > 0; i--)
                    {
                        if (Plugin.Instance.Config.BanLevels.TryGetValue(i, out rawBanInfo))
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
                    catch (InvalidCastException ex)
                    {
                        Log.Error("Error Code: 4.3");
                        Log.Error(rawBanInfo[0].GetType().ToString());
                        Log.Error(rawBanInfo[1].GetType().ToString());
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error Code: 4.4");
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                        return;
                    }

                    Log.Info($"Player {player.ToString(true)} has been {(duration == 0 ? "kicked" : "banned")} for teamkilling {tks} players.");
                    Ban(player, duration, message);
                }

                delayedPunishPlayers.Remove(player);
            });
        }

        private void Ban(Player player, long duration, string message)
        {
            duration *= 60; // Change from minutes to seconds

            if (player.Connection?.isAuthenticated ?? false)
            {
                player.Ban(Server.Instance, message, duration);
            }
            else
            {
                var nickname = player.Nickname;
                var maxLength = GameCore.ConfigFile.ServerConfig.GetInt("ban_nickname_maxlength", 30);
                var trimUnicode = GameCore.ConfigFile.ServerConfig.GetBool("ban_nickname_trimunicode", true);
                nickname = string.IsNullOrEmpty(nickname) ? "(no nick)" : nickname;
                if (trimUnicode)
                    nickname = NorthwoodLib.StringUtils.StripUnicodeCharacters(nickname, string.Empty);

                if (nickname.Length > maxLength)
                    nickname = nickname.Substring(0, maxLength);

                Log.Debug("Status Code: 4.2", Plugin.Instance.Config.VerbouseOutput);
                Log.Info($"Player ({nickname}) left before ban, banning offline");
                
                if (!EventManager.ExecuteEvent<bool>(ServerEventType.PlayerBanned, player, Server.Instance, message, duration))
                {
                    Log.Debug("Skip Code: 4.1", Plugin.Instance.Config.VerbouseOutput);
                    return; // Skip Code: 4.1
                }

                // duration = ev.Duration;
                // message = ev.Reason;

                var issuanceTime = TimeBehaviour.CurrentTimestamp();
                var banExpirationTime = TimeBehaviour.GetBanExpirationTime((uint)duration);
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
                Log.Info($"Offline banned {player.UserId}");
                // Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, userIdBan, BanHandler.BanType.UserId));

                if (leftPlayersIPs.TryGetValue(player.UserId, out var ip))
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
                    Log.Info($"Offline banned {ip}");
                    // Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, ipBan, BanHandler.BanType.IP));
                }
            }
        }
    }
}
