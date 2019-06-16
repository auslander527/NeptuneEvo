using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using NeptuneEvo.GUI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class Admin : Script
    {
        private static nLog Log = new nLog("Admin");
        public static bool IsServerStoping = false;

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            ColShape colShape = NAPI.ColShape.CreateCylinderColShape(DemorganPosition, 100, 50, 1337);
            colShape.OnEntityExitColShape += (s, e) =>
            {
                if (!Main.Players.ContainsKey(e)) return;
                if (Main.Players[e].DemorganTime > 0) NAPI.Entity.SetEntityPosition(e, DemorganPosition + new Vector3(0, 0, 1.5));
            };
            Group.LoadCommandsConfigs();
        }

        public static void sendRedbucks(Client player, Client target, int amount)
        {
            if (!Group.CanUseCmd(player, "givereds")) return;

            if (Main.Accounts[target].RedBucks + amount < 0) amount = 0;
            Main.Accounts[target].RedBucks += amount;
            Trigger.ClientEvent(target, "starset", Main.Accounts[target].RedBucks);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы отправили {target.Name} {amount} redbucks", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"+{amount} redbucks", 3000);

            GameLog.Admin(player.Name, $"givereds({amount})", target.Name);
        } 
        public static void stopServer(Client sender, string reason = "Сервер выключен.")
        {
            if (!Group.CanUseCmd(sender, "stop")) return;
            IsServerStoping = true;
            GameLog.Admin($"{sender.Name}", $"stopServer({reason})", "");

            Log.Write("Force saving database...", nLog.Type.Warn);
            BusinessManager.SavingBusiness();
            Fractions.GangsCapture.SavingRegions();
            Houses.HouseManager.SavingHouses();
            Houses.FurnitureManager.Save();
            nInventory.SaveAll();
            Fractions.Stocks.saveStocksDic();
            Weapons.SaveWeaponsDB();
            Log.Write("All data has been saved!", nLog.Type.Success);

            Log.Write("Force kicking players...", nLog.Type.Warn);
            foreach (Client player in NAPI.Pools.GetAllPlayers())
                NAPI.Player.KickPlayer(player, reason);
            Log.Write("All players has kicked!", nLog.Type.Success);

            NAPI.Task.Run(() =>
            {
                Environment.Exit(0);
            }, 60000);
        }
        public static void stopServer(string reason = "Сервер выключен.")
        {
            IsServerStoping = true;
            GameLog.Admin("server", $"stopServer({reason})", "");

            Log.Write("Force saving database...", nLog.Type.Warn);
            BusinessManager.SavingBusiness();
            Fractions.GangsCapture.SavingRegions();
            Houses.HouseManager.SavingHouses();
            Houses.FurnitureManager.Save();
            nInventory.SaveAll();
            Fractions.Stocks.saveStocksDic();
            Weapons.SaveWeaponsDB();
            Log.Write("All data has been saved!", nLog.Type.Success);

            Log.Write("Force kicking players...", nLog.Type.Warn);
            foreach (Client player in NAPI.Pools.GetAllPlayers())
                NAPI.Player.KickPlayer(player, reason);
            Log.Write("All players has kicked!", nLog.Type.Success);

            NAPI.Task.Run(() =>
            {
                Environment.Exit(0);
            }, 60000);
        }
        public static void saveCoords(Client player, string msg)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            pos.Z -= 1.12f;
            //NAPI.Blip.CreateBlip(1, pos, 1, 69);
            Vector3 rot = NAPI.Entity.GetEntityRotation(player);
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Vehicle vehicle = player.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {

                StreamWriter saveCoords = new StreamWriter("coords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                saveCoords.Write($"{msg}   Coords: new Vector3({pos.X}, {pos.Y}, {pos.Z}),    JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(pos)}      \r\n");
                saveCoords.Write($"{msg}   Rotation: new Vector3({rot.X}, {rot.Y}, {rot.Z}),     JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(rot)}    \r\n");
                saveCoords.Close();
            }

            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }

            finally
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords: " + NAPI.Entity.GetEntityPosition(player));
            }
        }
        public static void setPlayerAdminGroup(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "setadmin")) return;
            if (Main.Players[target].AdminLVL >= 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть админ. прав", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 1;
            //Main.AdminSlots.Add(target.GetData("RealSocialClub"), new Main.AdminSlotsData(target.Name, 1, true, false));
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы Выдали админ. права игроку {target.Name}", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} Выдал Вам админ. права", 3000);
            GameLog.Admin($"{player.Name}", $"setAdmin", $"{target.Name}");
        }
        public static void delPlayerAdminGroup(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "deladmin")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете забрать админ. права у себя", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете забрать права у этого администратора", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет админ. прав", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 0;

            //Main.AdminSlots.Remove(target.GetData("RealSocialClub"));

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы забрали права у администратора {target.Name}", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} забрал у Вас админ. права", 3000);
            GameLog.Admin($"{player.Name}", $"delAdmin", $"{target.Name}");
        }
        public static void setPlayerAdminRank(Client player, Client target, int rank)
        {
            if (!Group.CanUseCmd(player, "setadminrank")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете установить себе ранг", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не является администратором!", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете изменить уровень прав у этого администратора", 3000);
                return;
            }
            if (rank < 1 || rank >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно выдать такой ранг", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} {rank} уровень админ. прав", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} выдал Вам {rank} уровень админ. прав", 3000);
            Main.Players[target].AdminLVL = rank;
            //Main.AdminSlots[target.GetData("RealSocialClub")].AdminLVL = rank;
            GameLog.Admin($"{player.Name}", $"setAdminRank({rank})", $"{target.Name}");
        }
        public static void setPlayerVipLvl(Client player, Client target, int rank)
        {
            if (!Group.CanUseCmd(player, "setviplvl")) return;
            if (rank > 4 || rank < 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно выдать такой уровень ВИП аккаунта", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} {Group.GroupNames[rank]}", 3000);
            Main.Accounts[target].VipLvl = rank;
            Main.Accounts[target].VipDate = DateTime.Now.AddDays(30);
            GUI.Dashboard.sendStats(target);
            GameLog.Admin($"{player.Name}", $"setVipLvl({rank})", $"{target.Name}");
        }

        public static void setFracLeader(Client sender, Client target, int fracid)
        {
            if (!Group.CanUseCmd(sender, "setleader")) return;
            if (fracid != 0 && fracid <= 17)
            {
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                int new_fraclvl = Fractions.Configs.FractionRanks[fracid].Count;
                Main.Players[target].FractionLVL = new_fraclvl;
                Main.Players[target].FractionID = fracid;
                Main.Players[target].WorkID = 0;
                if(fracid == 15) {
                    Trigger.ClientEvent(target, "enableadvert", true);
                    Fractions.LSNews.onLSNPlayerLoad(target);
                }
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы стали лидером фракции {Fractions.Manager.getName(fracid)}", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы поставили {target.Name} на лидерство {Fractions.Manager.getName(fracid)}", 3000);
                Fractions.Manager.Load(target, fracid, new_fraclvl);
                Dashboard.sendStats(target);
                GameLog.Admin($"{sender.Name}", $"setFracLeader({fracid})", $"{target.Name}");
                return;
            }
        }
        public static void delFracLeader(Client sender, Client target)
        {
            if (!Group.CanUseCmd(sender, "delleader")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 17)
            {
                if (Main.Players[target].FractionLVL < Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не является лидером", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if(Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{sender.Name.Replace('_', ' ')} снял Вас с поста лидера фракции", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли {target.Name.Replace('_', ' ')} с поста лидера фракции", 3000);
                Dashboard.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                GameLog.Admin($"{sender.Name}", $"delFracLeader", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет фракции", 3000);
        }
        public static void delJob(Client sender, Client target)
        {
            if (!Group.CanUseCmd(sender, "deljob")) return;
            if (Main.Players[target].WorkID != 0)
            {
                if (NAPI.Data.GetEntityData(target, "ON_WORK") == true)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок должен быть не в рабочей форме", 3000);
                    return;
                }
                Main.Players[target].WorkID = 0;
                Dashboard.sendStats(target);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{sender.Name.Replace('_', ' ')} снял трудоустройство с Вашего персонажа", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли {target.Name.Replace('_', ' ')} с трудоустройства", 3000);
                Dashboard.sendStats(target);
                GameLog.Admin($"{sender.Name}", $"delJob", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет работы", 3000);
        }
        public static void delFrac(Client sender, Client target)
        {
            if (!Group.CanUseCmd(sender, "delfrac")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 17)
            {
                if (Main.Players[target].FractionLVL >= Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок - лидер фракции", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if(Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Администратор {sender.Name.Replace('_', ' ')} выгнал Вас из фракции", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выгнали {target.Name.Replace('_', ' ')} из фракции", 3000);
                Dashboard.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                GameLog.Admin($"{sender.Name}", $"delFrac", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет фракции", 3000);
        }

        public static void giveMoney(Client player, Client target, int amount)
        {
            if (!Group.CanUseCmd(player, "givemoney")) return;
            GameLog.Money($"player({Main.Players[player].UUID})",$"player({Main.Players[target].UUID})", amount, "admin");
            MoneySystem.Wallet.Change(target, amount);
            GameLog.Admin($"{player.Name}", $"giveMoney({amount})", $"{target.Name}");
        }
        public static void OffMutePlayer(Client player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "mute")) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    mutePlayer(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок был онлайн, поэтому offmute заменён на mute", 3000);
                    return;
                }
                if(player.Name.Equals(target)) return;
                if (time > 480)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете дать мут больше, чем на 480 минут", 3000);
                    return;
                }
                var split = target.Split('_');
                MySQL.QueryRead($"UPDATE `characters` SET `unmute`={time * 60} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал мут игроку {target} на {time}м ({reason})");
                GameLog.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target}");
            }
            catch { }

        }
        public static void mutePlayer(Client player, Client target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "mute")) return;
            if (player == target) return;
            if (time > 480)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете дать мут больше, чем на 480 минут", 3000);
                return;
            }
            Main.Players[target].Unmute = time * 60;
            Main.Players[target].VoiceMuted = true;
            if (target.HasData("MUTE_TIMER")) Timers.Stop(target.GetData("MUTE_TIMER"));
            NAPI.Data.SetEntityData(target, "MUTE_TIMER", Timers.StartTask(1000, () => timer_mute(target)));
            target.SetSharedData("voice.muted", true);
            Trigger.ClientEvent(target, "voice.mute");
            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал мут игроку {target.Name} на {time}м ({reason})");
            GameLog.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target.Name}");
        }
        public static void unmutePlayer(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "unmute")) return;

            Main.Players[target].Unmute = 2;
            Main.Players[target].VoiceMuted = false;
            target.SetSharedData("voice.muted", false);

            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} снял мут с игрока {target.Name}");
            GameLog.Admin($"{player.Name}", $"unmutePlayer", $"{target.Name}");
        }
        public static void banPlayer(Client player, Client target, int time, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "sban" : "ban";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (player == target) return;
            if(Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL) {
                Commands.SendToAdmins(3, $"!{{#d35400}}[BAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} забанил игрока {target.Name} на {time}{banTimeMsg} ({reason})");
            
            Ban.Online(target, unbanTime, false, reason, player.Name);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Вы заблокированы до {unbanTime.ToString()}", 30000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Причина: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;
            
            GameLog.Ban(AUUID, TUUID, unbanTime, reason, false);

            target.Kick(reason);
        }
        public static void hardbanPlayer(Client player, Client target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            if (player == target) return;
            if(Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL) {
                Commands.SendToAdmins(3, $"!{{#d35400}}[HARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }
            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} ударил банхаммером игрока {target.Name} на {time}{banTimeMsg} ({reason})");

            Ban.Online(target, unbanTime, true, reason, player.Name);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Ты словил банхаммер до {unbanTime.ToString()}", 30000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Причина: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, true);

            target.Kick(reason);
        }
        public static void offBanPlayer(Client player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name == name) return;
            Client target = NAPI.Player.GetPlayerFromName(name);
            if (target != null) {
                if(Main.Players.ContainsKey(target)) {
                    if(Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL) {
                        Commands.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                        return;
                    } else {
                        target.Kick();
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, "Игрок находился в Online, но был кикнут.", 3000);
                    }
                }
            } else {
                string[] split = name.Split('_');
                DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if(targetadminlvl >= Main.Players[player].AdminLVL) {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {name} (offline), который имеет выше уровень администратора.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Ban ban = Ban.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Игрок уже в {hard}бане", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м"; // Можно использовать char
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Ban.Offline(name, unbanTime, false, reason, player.Name);

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, false);

            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} забанил игрока {name} на {time}{banTimeMsg} ({reason})");
        }
        public static void offHardBanPlayer(Client player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if(player.Name.Equals(name)) return;
            Client target = NAPI.Player.GetPlayerFromName(name);
            if (target != null) {
                if(Main.Players.ContainsKey(target)) {
                    if(Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL) {
                        Commands.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                        return;
                    } else {
                        target.Kick();
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, "Игрок находился в Online, но был кикнут.", 3000);
                    }
                }
            } else {
                string[] split = name.Split('_');
                DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if(targetadminlvl >= Main.Players[player].AdminLVL) {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {name} (offline), который имеет выше уровень администратора.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Ban ban = Ban.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Игрок уже в {hard}бане", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Ban.Offline(name, unbanTime, true, reason, player.Name);

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, true);

            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} ударил банхаммером игрока {name} на {time}{banTimeMsg} ({reason})");
        }
        public static void unbanPlayer(Client player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого имени нет!", 3000);
                return;
            }
            if (!Ban.Pardon(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} не находится в бане!", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Игрок разблокирован!", 3000);
            GameLog.Admin($"{player.Name}", $"unban", $"{name}");
        }
        public static void unhardbanPlayer(Client player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого имени нет!", 3000);
                return;
            }
            if (!Ban.PardonHard(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} не находится в бане!", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "С игрока снят хардбан!", 3000);
        }
        public static void kickPlayer(Client player, Client target, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "skick" : "kick";
            if (!Group.CanUseCmd(player, cmd)) return;
            if(Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL) {
                Commands.SendToAdmins(3, $"!{{#d35400}}[KICK-DENIED] {player.Name} ({player.Value}) попытался кикнуть {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} кикнул игрока {target.Name} ({reason})");
            else
            {
                foreach (Client p in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"~r~{player.Name} тихо кикнул игрока {target.Name}");
                    }
                }
            }
            GameLog.Admin($"{player.Name}", $"kickPlayer({reason})", $"{target.Name}");
            NAPI.Player.KickPlayer(target, reason);
        }
        public static void warnPlayer(Client player, Client target, string reason)
        {
            if (!Group.CanUseCmd(player, "warn")) return;
            if(player == target) return;
            if(Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL) {
                Commands.SendToAdmins(3, $"!{{#d35400}}[WARN-DENIED] {player.Name} ({player.Value}) попытался предупредить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            Main.Players[target].Warns++;
            Main.Players[target].Unwarn = DateTime.Now.AddDays(14);

            int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал предупреждение игроку {target.Name} ({reason}) [{Main.Players[target].Warns}/3]");

            if (Main.Players[target].Warns >= 3)
            {
                DateTime unbanTime = DateTime.Now.AddMinutes(43200);
                Main.Players[target].Warns = 0;
                Ban.Online(target, unbanTime, false, "Warns 3/3", "Server_Serverniy");
            }

            GameLog.Admin($"{player.Name}", $"warnPlayer({reason})", $"{target.Name}");
            target.Kick("Предупреждение");
        }
        public static void kickPlayerByName(Client player, string name)
        {
            if (!Group.CanUseCmd(player, "nkick")) return;
            Client target = NAPI.Player.GetPlayerFromName(name);
            if (target == null) return;
            NAPI.Player.KickPlayer(target);
            GameLog.Admin($"{player.Name}", $"kickPlayer", $"{name}");
        }

        public static void killTarget(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "kill")) return;
            NAPI.Player.SetPlayerHealth(target, 0);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы убили игрока {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"killPlayer", $"{target.Name}");
        }
        public static void healTarget(Client player, Client target, int hp)
        {
            if (!Group.CanUseCmd(player, "hp")) return;
            NAPI.Player.SetPlayerHealth(target, hp);
            GameLog.Admin($"{player.Name}", $"healPlayer({hp})", $"{target.Name}");
        }
        public static void armorTarget(Client player, Client target, int ar)
        {
            if (!Group.CanUseCmd(player, "ar")) return;

            nItem aItem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
            if (aItem == null)
                nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, ar.ToString()));
            GameLog.Admin($"{player.Name}", $"armorPlayer({ar})", $"{target.Name}");
        }
        public static void checkGamemode(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "gm")) return;
            int targetHealth = target.Health;
            int targetArmor = target.Armor;
            NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 10));
            NAPI.Task.Run(() => { try { Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"{target.Name} было {targetHealth} HP {targetArmor} Armor | Стало {target.Health} HP {target.Armor} Armor.", 3000); } catch { } }, 3000);
            GameLog.Admin($"{player.Name}", $"checkGm", $"{target.Name}");
        }
        public static void checkMoney(Client player, Client target)
        {
            try
            {
                if (!Group.CanUseCmd(player, "checkmoney")) return;
                MoneySystem.Bank.Data bankAcc = MoneySystem.Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target.Name).Value;
                int bankMoney = 0;
                if (bankAcc != null) bankMoney = (int)bankAcc.Balance;
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"У {target.Name} {Main.Players[target].Money}$ | Bank: {bankMoney}", 3000);
                GameLog.Admin($"{player.Name}", $"checkMoney", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("CheckMoney: " + e.Message, nLog.Type.Error); }
        }
        
        public static void teleportTargetToPlayer(Client player, Client target, bool withveh = false)
        {
            if (!Group.CanUseCmd(player, "metp")) return;
            if(!withveh) {
                GameLog.Admin($"{player.Name}", $"metp", $"{target.Name}");
                NAPI.Entity.SetEntityPosition(target, player.Position);
                NAPI.Entity.SetEntityDimension(target, player.Dimension);
            } else {
                if (!target.IsInVehicle) return;
                NAPI.Entity.SetEntityPosition(target.Vehicle, player.Position + new Vector3(2,2,2));
                NAPI.Entity.SetEntityDimension(target.Vehicle, player.Dimension);
                GameLog.Admin($"{player.Name}", $"gethere", $"{target.Name}");
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы телепортировали {target.Name} к себе", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} телепортировал Вас к себе", 3000);
        }
        
        public static void freezeTarget(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "fz")) return;
            Trigger.ClientEvent(target, "freeze", true);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы заморозили игрока {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"freeze", $"{target.Name}");
        }
        public static void unFreezeTarget(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "ufz")) return;
            Trigger.ClientEvent(target, "freeze", false);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы разморозили игрока {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"unfreeze", $"{target.Name}");
        }
        
        public static void giveTargetGun(Client player, Client target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "guns")) return;
            if (serial.Length != 9)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Серийный номер состоит из 9 символов", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if(wType == ItemType.Mask || wType == ItemType.Gloves || wType == ItemType.Leg || wType == ItemType.Bag || wType == ItemType.Feet ||
                wType == ItemType.Jewelry || wType == ItemType.Undershit || wType == ItemType.BodyArmor || wType == ItemType.Unknown || wType == ItemType.Top || 
                wType == ItemType.Hat || wType == ItemType.Glasses || wType == ItemType.Accessories) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Предметы одежды выдавать запрещено", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У игрока недостаточно места в инвентаре", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} оружие ({weapon.ToString()})", 3000);
            GameLog.Admin($"{player.Name}", $"giveGun({weapon},{serial})", $"{target.Name}");
        }
        public static void giveTargetSkin(Client player, Client target, string pedModel)
        {
            if (!Group.CanUseCmd(player, "setskin")) return;
            if(pedModel.Equals("-1")) {
                if(target.HasData("AdminSkin")) {
                    target.ResetData("AdminSkin");
                    target.SetSkin((Main.Players[target].Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                    Customization.ApplyCharacter(target);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы восстановили игроку внешность", 3000);
                } else {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игроку не меняли внешность", 3000);
                    return;
                }
            } else {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                if (pedHash != 0) {
                    target.SetData("AdminSkin", true);
                    target.SetSkin(pedHash);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сменили игроку {target.Name} внешность на ({pedModel})", 3000);
                } else {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Внешности с таким названием не было найдено", 3000);
                    return;
                }
            }
        }
        public static void giveTargetClothes(Client player, Client target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "giveclothes")) return;
            if (serial.Length < 6 || serial.Length > 12)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Серийный номер состоит из 6-12 символов", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if(wType != ItemType.Mask && wType != ItemType.Gloves && wType != ItemType.Leg && wType != ItemType.Bag && wType != ItemType.Feet &&
                wType != ItemType.Jewelry && wType != ItemType.Undershit && wType != ItemType.BodyArmor && wType != ItemType.Unknown && wType != ItemType.Top && 
                wType != ItemType.Hat && wType != ItemType.Glasses && wType != ItemType.Accessories) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этой командой можно выдавать только предметы одежды", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно места в инвентаре", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} одежду ({weapon.ToString()})", 3000);
        }
        public static void takeTargetGun(Client player, Client target)
        {
            if (!Group.CanUseCmd(player, "oguns")) return;
            Weapons.RemoveAll(target, true);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы забрали у игрока {target.Name} всё оружие", 3000);
            GameLog.Admin($"{player.Name}", $"takeGuns", $"{target.Name}");
        }

        public static void adminSMS(Client player, Client target, string message)
        {
            if (!Group.CanUseCmd(player, "asms")) return;
            target.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
            player.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
        }
        public static void answerReport(Client player, Client target, string message)
        {
            if (!Group.CanUseCmd(player, "ans")) return;
            if (!target.HasData("IS_REPORT")) return;

            player.SendChatMessage($"~r~Вы ответили для {target.Name}: {message}");
            target.SendChatMessage($"~r~Ответ от {player.Name} ({player.Value}): {message}");
            target.ResetData("IS_REPORT");
            foreach (Client p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage($"~y~[ANSWER] {player.Name}({player.Value})->{target.Name}({target.Value}): {message}");
                }
            }
            GameLog.Admin($"{player.Name}", $"answer({message})", $"{target.Name}");
        }
        public static void adminChat(Client player, string message)
        {
            if (!Group.CanUseCmd(player, "a")) return;
            foreach (Client p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage("!{#00ECAA}" + $"[AChat] {player.Name} ({player.Value}): {message}");
                }
            }
        }
        public static void adminGlobal(Client player, string message)
        {
            if (!Group.CanUseCmd(player, "global")) return;
            NAPI.Chat.SendChatMessageToAll("!{#DF5353}" + $"[GLOBAL] {player.Name.Replace('_', ' ')}: {message}");
            GameLog.Admin($"{player.Name}", $"global({message})", $"");
        }
        public static void sendPlayerToDemorgan(Client admin, Client target, int time, string reason)
        {
            if (!Group.CanUseCmd(admin, "demorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;
            if(admin == target) return;
            int firstTime = time * 60;
            string deTimeMsg = "м";
            if (time > 60)
            {
                deTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    deTimeMsg = "д";
                    time /= 24;
                }
            }

            NAPI.Chat.SendChatMessageToAll($"~r~{admin.Name} посадил игрока {target.Name} в спец. тюрьму на {time}{deTimeMsg} ({reason})");
            Main.Players[target].ArrestTime = 0;
            Main.Players[target].DemorganTime = firstTime;
            Fractions.FractionCommands.unCuffPlayer(target);

            NAPI.Entity.SetEntityPosition(target, DemorganPosition + new Vector3(0, 0, 1.5));
            //if (target.HasData("ARREST_TIMER")) Main.StopT(target.GetData("ARREST_TIMER"), "timer_34");
            if (target.HasData("ARREST_TIMER")) Timers.Stop(target.GetData("ARREST_TIMER"));
            //NAPI.Data.SetEntityData(target, "ARREST_TIMER", Main.StartT(1000, 1000, (o) => timer_demorgan(target), "DEMORGAN_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.StartTask(1000, () => timer_demorgan(target)));
            NAPI.Entity.SetEntityDimension(target, 1337);
            Weapons.RemoveAll(target, true);
            GameLog.Admin($"{admin.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target.Name}");
        }
        public static void releasePlayerFromDemorgan(Client admin, Client target)
        {
            if (!Group.CanUseCmd(admin, "udemorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;

            Main.Players[target].DemorganTime = 0;
            Notify.Send(admin, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы освободили {target.Name} из админ. тюрьмы", 3000);
            GameLog.Admin($"{admin.Name}", $"undemorgan", $"{target.Name}");
        }

        #region Demorgan
        public static Vector3 DemorganPosition = new Vector3(1651.217, 2570.393, 44.44485);
        public static void timer_demorgan(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].DemorganTime <= 0)
                {
                    Fractions.FractionCommands.freePlayer(player);
                    return;
                }
                Main.Players[player].DemorganTime--;
            }
            catch (Exception e)
            {
                Log.Write("DEMORGAN_TIMER: " + e.ToString(), nLog.Type.Error);
            }
        }
        public static void timer_mute(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Unmute <= 0)
                {
                    if (!player.HasData("MUTE_TIMER")) return;
                    Timers.Stop(NAPI.Data.GetEntityData(player, "MUTE_TIMER"));
                    NAPI.Data.ResetEntityData(player, "MUTE_TIMER");
                    Main.Players[player].VoiceMuted = false;
                    player.SetSharedData("voice.muted", false);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Mute был снят, не нарушайте больше!", 3000);
                    return;
                }
                Main.Players[player].Unmute--;
            }
            catch (Exception e)
            {
                Log.Write("MUTE_TIMER: " + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
        // need refactor
        public static void respawnAllCars(Client player)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();

            foreach (Vehicle vehicle in all_vehicles)
            {
                List<Client> occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    List<Client> newOccupants = new List<Client>();
                    foreach (Client occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (Vehicle vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                if (vehicle.GetData("ACCESS") == "PERSONAL")
                {
                    Client owner = vehicle.GetData("OWNER");
                    NAPI.Entity.DeleteEntity(vehicle);
                }
                else if (vehicle.GetData("ACCESS") == "WORK")
                    RespawnWorkCar(vehicle);
                else if (vehicle.GetData("ACCESS") == "FRACTION")
                    RespawnFractionCar(vehicle);
                else if (vehicle.GetData("ACCESS") == "GANGDELIVERY" || vehicle.GetData("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);
            }
        }

        public static void RespawnWorkCar(Vehicle vehicle)
        {
            if (vehicle.GetData("ON_WORK") && Main.Players.ContainsKey(vehicle.GetData("DRIVER"))) return;
            var type = vehicle.GetData("TYPE");
            switch (type)
            {
                case "MOWER":
                    Jobs.Lawnmower.respawnCar(vehicle);
                    break;
                case "BUS":
                    Jobs.Bus.respawnBusCar(vehicle);
                    break;
                case "TAXI":
                    Jobs.Taxi.respawnCar(vehicle);
                    break;
                case "TRUCKER":
                    Jobs.Truckers.respawnCar(vehicle);
                    break;
                case "COLLECTOR":
                    Jobs.Collector.respawnCar(vehicle);
                    break;
                case "MECHANIC":
                    Jobs.AutoMechanic.respawnCar(vehicle);
                    break;
            }
        }

        public static void RespawnFractionCar(Vehicle vehicle)
        {
            if (NAPI.Data.HasEntityData(vehicle, "loaderMats"))
            {
                Client loader = NAPI.Data.GetEntityData(vehicle, "loaderMats");
                Trigger.ClientEvent(loader, "hideLoader");
                Notify.Send(loader, NotifyType.Warning, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                if (loader.HasData("loadMatsTimer"))
                {
                    //Main.StopT(loader.GetData("loadMatsTimer"), "timer_35");
                    Timers.Stop(loader.GetData("loadMatsTimer"));
                    loader.ResetData("loadMatsTime");
                }
                NAPI.Data.ResetEntityData(vehicle, "loaderMats");
            }
            Fractions.Configs.RespawnFractionCar(vehicle);
        }
    }

    public class Group
    {
        private static List<GroupCommand> GroupCommands = new List<GroupCommand>();
        public static void LoadCommandsConfigs()
        {
            DataTable result = MySQL.QueryRead($"SELECT * FROM adminaccess");
            if (result == null || result.Rows.Count == 0) return;
            List<GroupCommand> groupCmds = new List<GroupCommand>();
            foreach (DataRow Row in result.Rows)
            {
                string cmd = Convert.ToString(Row["command"]);
                bool isadmin = Convert.ToBoolean(Row["isadmin"]);
                int minrank = Convert.ToInt32(Row["minrank"]);

                groupCmds.Add(new GroupCommand(cmd, isadmin, minrank));
            }
            GroupCommands = groupCmds;
        }

        public static List<string> GroupNames = new List<string>()
        {
            "Игрок",
            "Bronze VIP",
            "Silver VIP",
            "Gold VIP",
            "Platinum VIP",
        };
        public static List<float> GroupPayAdd = new List<float>()
        {
            1.0f,
            1.0f,
            1.15f,
            1.25f,
            1.35f,
        };
        public static List<int> GroupAddPayment = new List<int>()
        {
            0,
            200,
            400,
            550,
            700
        };
        public static List<int> GroupMaxContacts = new List<int>()
        {
            50,
            60,
            70,
            80,
            100,
        };
        public static List<int> GroupMaxBusinesses = new List<int>()
        {
            1,
            1,
            1,
            1,
            1,
        };
        public static List<int> GroupEXP = new List<int>()
        {
            1,
            2,
            2,
            2,
            3,
        };

        public static bool CanUseCmd(Client player, string cmd, string args = "")
        {
            if (!Main.Players.ContainsKey(player)) return false;
            GroupCommand command = GroupCommands.FirstOrDefault(c => c.Command == cmd);
        check:
            if (command != null)
            {
                if (command.IsAdmin)
                {
                    if (Main.Players[player].AdminLVL >= command.MinLVL) return true;
                }
                else
                {
                    if (Main.Accounts[player].VipLvl >= command.MinLVL) return true;
                }
            }
            else
            {
                MySQL.Query($"INSERT INTO `adminaccess`(`command`, `isadmin`, `minrank`) VALUES ('{cmd}',1,7)");
                GroupCommand newGcmd = new GroupCommand(cmd, true, 7);
                command = newGcmd;
                GroupCommands.Add(newGcmd);
                goto check;
            }
           
            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно прав", 3000);
            return false;
        }

        internal class GroupCommand
        {
            public GroupCommand(string command, bool isAdmin, int minlvl)
            {
                Command = command;
                IsAdmin = isAdmin;
                MinLVL = minlvl;
            }

            public string Command { get; }
            public bool IsAdmin { get; }
            public int MinLVL { get; }
        }
    }
}
