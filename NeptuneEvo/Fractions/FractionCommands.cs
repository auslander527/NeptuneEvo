using NeptuneEvo.Core;
using Redage.SDK;
using System;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.GUI;
using NeptuneEvo.Core.Character;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

namespace NeptuneEvo.Fractions
{
    class FractionCommands : Script
    {
        private static nLog Log = new nLog("FractionCommangs");

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Client player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "CUFFED") && player.VehicleSeat == -1)
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Отпустите человека", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        private static Dictionary<int, DateTime> NextCarRespawn = new Dictionary<int, DateTime>()
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
            { 6, DateTime.Now },
            { 7, DateTime.Now },
            { 8, DateTime.Now },
            { 9, DateTime.Now },
            { 10, DateTime.Now },
            { 11, DateTime.Now },
            { 12, DateTime.Now },
            { 13, DateTime.Now },
            { 14, DateTime.Now },
            { 15, DateTime.Now },
            { 16, DateTime.Now },
            { 17, DateTime.Now },
        };
        public static void respawnFractionCars(Client player)
        {
            if (Main.Players[player].FractionID == 0 || Main.Players[player].FractionLVL < (Configs.FractionRanks[Main.Players[player].FractionID].Count - 1)) return;
            if (DateTime.Now < NextCarRespawn[Main.Players[player].FractionID])
            {
                DateTime g = new DateTime((NextCarRespawn[Main.Players[player].FractionID] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сможете сделать это только через {min}:{sec}", 3000);
                return;
            }

            var all_vehicles = NAPI.Pools.GetAllVehicles();
            foreach (var vehicle in all_vehicles)
            {
                var occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    var newOccupants = new List<Client>();
                    foreach (var occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (var vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                var color1 = vehicle.PrimaryColor;
                var color2 = vehicle.SecondaryColor;
                if (!vehicle.HasData("ACCESS")) continue;

                if (vehicle.GetData("ACCESS") == "FRACTION" && vehicle.GetData("FRACTION") == Main.Players[player].FractionID)
                    Admin.RespawnFractionCar(vehicle);
            }

            NextCarRespawn[Main.Players[player].FractionID] = DateTime.Now.AddHours(2);
            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы зареспавнили все фракционные машины", 3000);
        }
        public static void playerPressCuffBut(Client player)
        {
            var fracid = Main.Players[player].FractionID;
            if (!Manager.canUseCommand(player, "cuff")) return;
            if (NAPI.Data.GetEntityData(player, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы в наручниках или связаны", 3000);
                return;
            }
            var target = Main.GetNearestPlayer(player, 2);
            if (target == null) return;
            var cuffmesp = ""; // message for Player after cuff
            var cuffmest = ""; // message for Target after cuff
            var uncuffmesp = ""; // message for Player after uncuff
            var uncuffmest = ""; // message for Target after uncuff
            var cuffme = ""; // message /me after cuff
            var uncuffme = ""; // message /me after uncuff

            if (player.IsInVehicle) return;
            if (target.IsInVehicle) return;

            if (Manager.FractionTypes[fracid] == 2) // for gov factions
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны сначала начать рабочий день", 3000);
                    return;
                }
                if (target.GetData("CUFFED_BY_MAFIA"))
                {
                    uncuffmesp = $"Вы развязали игрока {target.Name}";
                    uncuffmest = $"Игрок {player.Name} развязал Вас";
                    uncuffme = "развязал(а) игрока {name}";
                }
                else
                {
                    cuffmesp = $"Вы надели наручники на игрока {target.Name}";
                    cuffmest = $"Игрок {player.Name} надел на Вас наручники";
                    cuffme = "надел(а) наручники на игрока {name}";
                    uncuffmesp = $"Вы сняли наручники с игрока {target.Name}";
                    uncuffmest = $"Игрок {player.Name} снял с Вас наручники";
                    uncuffme = "снял(а) наручники с игрока {name}";
                }
            }
            else // for mafia
            {
                if (target.GetData("CUFFED_BY_COP"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от наручников", 3000);
                    return;
                }
                var cuffs = nInventory.Find(Main.Players[player].UUID, ItemType.Cuffs);
                var count = (cuffs == null) ? 0 : cuffs.Count;

                if (!target.GetData("CUFFED") && count == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет стяжек для рук", 3000);
                    return;
                }
                else if (!target.GetData("CUFFED"))
                    nInventory.Remove(player, ItemType.Cuffs, 1);

                cuffmesp = $"Вы связали игрока {target.Name}";
                cuffmest = $"Игрок {player.Name} связал Вас";
                cuffme = "связал(а) игрока {name}";
                uncuffmesp = $"Вы развязали игрока {target.Name}";
                uncuffmest = $"Игрок {player.Name} развязал Вас";
                uncuffme = "развязал(а) игрока {name}";
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы в машине", 3000);
                return;
            }
            if (NAPI.Player.IsPlayerInAnyVehicle(target))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок в машине", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING") || NAPI.Data.HasEntityData(target, "FOLLOWER") || Main.Players[target].ArrestTime != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно применить на данном игроке", 3000);
                return;
            }
            if (!target.GetData("CUFFED"))
            {
                // cuff target
                if (NAPI.Data.HasEntityData(target, "HAND_MONEY")) SafeMain.dropMoneyBag(target);
                if (NAPI.Data.HasEntityData(target, "HEIST_DRILL")) SafeMain.dropDrillBag(target);

                NAPI.Data.SetEntityData(target, "CUFFED", true);
                Voice.Voice.PhoneHCommand(target);

                Main.OnAntiAnim(player);
                NAPI.Player.PlayPlayerAnimation(target, 49, "mp_arresting", "idle");
                // -0.02 0.063 0 75 0 76
                BasicSync.AttachObjectToPlayer(target, NAPI.Util.GetHashKey("p_cs_cuffs_02_s"), 6286, new Vector3(-0.02f, 0.063f, 0.0f), new Vector3(75.0f, 0.0f, 76.0f));

                Trigger.ClientEvent(target, "CUFFED", true);
                if (fracid == 6 || fracid == 7 || fracid == 9) target.SetData("CUFFED_BY_COP", true);
                else target.SetData("CUFFED_BY_MAFIA", true);

                GUI.Dashboard.Close(target);
                Trigger.ClientEvent(target, "blockMove", true);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, cuffmesp, 3000);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, cuffmest, 3000);
                Commands.RPChat("me", player, cuffme, target);
                return;
            }
            // uncuff target
            unCuffPlayer(target);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, uncuffmesp, 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, uncuffmest, 3000);
            NAPI.Data.SetEntityData(target, "CUFFED_BY_COP", false);
            NAPI.Data.SetEntityData(target, "CUFFED_BY_MAFIA", false);
            Commands.RPChat("me", player, uncuffme, target);
            return;
        }
        
        public static void onPlayerDeathHandler(Client player, Client entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "CUFFED"))
                {
                    unCuffPlayer(player);
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Client target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    unFollow(player, target);
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Client cop = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    unFollow(cop, player);
                }
                if (player.HasData("HEAD_POCKET"))
                {
                    player.ClearAccessory(1);
                    player.SetClothes(1, 0, 0);

                    Trigger.ClientEvent(player, "setPocketEnabled", false);
                    player.ResetData("HEAD_POCKET");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        #region every fraction commands

        [Command("delad", GreedyArg = true)]
        public static void CMD_deleteAdvert(Client player, int AdID, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if(Main.Players[player].FractionID == 15) {
                    if (!Manager.canUseCommand(player, "delad")) return;
                    LSNews.AddAnswer(player, AdID, reason, true);
                }
                else if (Group.CanUseCmd(player, "delad")) LSNews.AddAnswer(player, AdID, reason, true);
            }
            catch (Exception e) { Log.Write("delad: " + e.Message, nLog.Type.Error); }
        }

        [Command("openstock")]
        public static void CMD_OpenFractionStock(Client player)
        {
            if (!Manager.canUseCommand(player, "openstock")) return;

            if (!Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Склад уже открыт", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].IsOpen = true;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы открыли склад", 3000);
        }

        [Command("closestock")]
        public static void CMD_CloseFractionStock(Client player)
        {
            if (!Manager.canUseCommand(player, "openstock")) return;

            if (!Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (!Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Склад уже закрыт", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].IsOpen = false;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы закрыли склад", 3000);
        }

        public static void GetMembers(Client sender)
        {
            if (Manager.canUseCommand(sender, "members"))
            {
                sender.SendChatMessage("Члены организации онлайн:");
                int fracid = Main.Players[sender].FractionID;
                foreach (var m in Manager.Members)
                    if (m.Value.FractionID == fracid) sender.SendChatMessage($"[{m.Value.inFracName}] {m.Value.Name}");
            }
        }

        public static void GetAllMembers(Client sender)
        {
            if (Manager.canUseCommand(sender, "offmembers"))
            {
                string message = "Все члены организации: ";
                NAPI.Chat.SendChatMessageToPlayer(sender, message);
                int fracid = Main.Players[sender].FractionID;
                var result = MySQL.QueryRead($"SELECT * FROM `characters` WHERE `fraction`='{fracid}'");
                foreach (DataRow Row in result.Rows)
                {
                    var fraclvl = Convert.ToInt32(Row["fractionlvl"]);
                    NAPI.Chat.SendChatMessageToPlayer(sender, $"~g~[{Manager.getNickname(fracid, fraclvl)}]: ~w~" + Row["name"].ToString().Replace('_', ' '));
                }
                return;
            }
        }

        public static void SetFracRank(Client sender, Client target, int newrank)
        {
            if (Manager.canUseCommand(sender, "setrank"))
            {
                if(newrank <= 0) {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя установить отрицательный или нулевой ранг", 3000);
                    return;
                }
                int senderlvl = Main.Players[sender].FractionLVL;
                int playerlvl = Main.Players[target].FractionLVL;
                int senderfrac = Main.Players[sender].FractionID;
                if (!Manager.inFraction(target, senderfrac)) return;

                if (newrank >= senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете повысить до этого ранга", 3000);
                    return;
                }
                if (playerlvl > senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете повысить этого игрока", 3000);
                    return;
                };
                Manager.UNLoad(target);

                Main.Players[target].FractionLVL = newrank;
                Manager.Load(target, Main.Players[target].FractionID, Main.Players[target].FractionLVL);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1)
                {
                    Manager.AllMembers[index].FractionLVL = newrank;
                    Manager.AllMembers[index].inFracName = Manager.getNickname(senderfrac, newrank);
                }
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Теперь ты {Manager.Members[target].inFracName} во фракции", 3000);
                Notify.Send(sender, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы повысили игрока {target.Name} до {Manager.Members[target].inFracName}", 3000);
                Dashboard.sendStats(target);
                return;
            }
        }

        public static void InviteToFraction(Client sender, Client target)
        {
            if (Manager.canUseCommand(sender, "invite"))
            {
                if (sender.Position.DistanceTo(target.Position) > 3)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
                    return;
                }
                if (Manager.isHaveFraction(target))
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок уже состоит организации", 3000);
                    return;
                }
                if (Main.Players[target].LVL < 1)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Необходим как минимум 1 lvl для приглашения игрока во фракцию", 3000);
                    return;
                }
                if (Main.Players[target].Warns > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно принять этого игрока", 3000);
                    return;
                }
                if (Manager.FractionTypes[Main.Players[sender].FractionID] == 2 && !Main.Players[target].Licenses[7])
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет мед.карты", 3000);
                    return;
                }

                target.SetData("INVITEFRACTION", Main.Players[sender].FractionID);
                target.SetData("SENDERFRAC", sender);
                Trigger.ClientEvent(target, "openDialog", "INVITED", $"{sender.Name} пригласил Вас в {Manager.FractionNames[Main.Players[sender].FractionID]}");
                
                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы пригласили во фракцию {target.Name}", 3000);
                Dashboard.sendStats(target);
            }
        }

        public static void UnInviteFromFraction(Client sender, Client target, bool mayor = false)
        {
            if (!Manager.canUseCommand(sender, "uninvite")) return;
            if (sender == target)
            {
                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете уволить сами себя", 3000);
                return;
            }

            int senderlvl = Main.Players[sender].FractionLVL;
            int playerlvl = Main.Players[target].FractionLVL;
            int senderfrac = Main.Players[sender].FractionID;

            if (senderlvl <= playerlvl)
            {
                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете выгнать этого игрока", 3000);
                return;
            }

            if (mayor)
            {
                if (Manager.FractionTypes[Main.Players[target].FractionID] != 2)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете выгнать этого игрока", 3000);
                    return;
                }
            }
            else
            {
                if (senderfrac != Main.Players[target].FractionID)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок состоит в другой организации", 3000);
                    return;
                }
            }

            Manager.UNLoad(target);

            int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Manager.AllMembers.RemoveAt(index);

            if(Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            Customization.ApplyCharacter(target);
            if (target.HasData("HAND_MONEY")) target.SetClothes(5, 45, 0);
            else if (target.HasData("HEIST_DRILL")) target.SetClothes(5, 41, 0);
            target.SetData("ON_DUTY", false);
            GUI.MenuManager.Close(sender);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вас выгнали из фракции {Manager.FractionNames[Main.Players[sender].FractionID]}", 3000);
            Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгнали из фракции {target.Name}", 3000);
            Dashboard.sendStats(target);
            return;
        }

        #endregion

        #region cops and cityhall commands
        public static void ticketToTarget(Client player, Client target, int sum, string reason)
        {
            if (!Manager.canUseCommand(player, "ticket")) return;
            if (sum > 7000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ограничение по штрафу 7000$", 3000);
                return;
            }
            if (reason.Length > 100)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком большая причина", 3000);
                return;
            }
            if (Main.Players[target].Money < sum && MoneySystem.Bank.Accounts[Main.Players[target].Bank].Balance < sum)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно средств", 3000);
                return;
            }

            target.SetData("TICKETER", player);
            target.SetData("TICKETSUM", sum);
            target.SetData("TICKETREASON", reason);
            Trigger.ClientEvent(target, "openDialog", "TICKET", $"{player.Name} выписал Вам штраф в размере {sum}$ за {reason}. Оплатить?");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выписали штраф для {target.Name} в размере {sum}$ за {reason}", 3000);
        }
        public static void ticketConfirm(Client target, bool confirm)
        {
            Client player = target.GetData("TICKETER");
            if (player == null || !Main.Players.ContainsKey(player)) return;
            int sum = target.GetData("TICKETSUM");
            string reason = target.GetData("TICKETREASON");

            if (confirm)
            {
                if (!MoneySystem.Wallet.Change(target, -sum) && !MoneySystem.Bank.Change(Main.Players[target].Bank, -sum, false))
                {
                    Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно средств", 3000);
                }

                Stocks.fracStocks[6].Money += Convert.ToInt32(sum * 0.9);
                MoneySystem.Wallet.Change(player, Convert.ToInt32(sum * 0.1));
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы оплатили штраф в размере {sum}$ за {reason}", 3000);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{target.Name} оплатил штраф в размере {sum}$ за {reason}", 3000);
                Commands.RPChat("me", player, " выписал штраф для {name}", target);
                Manager.sendFractionMessage(7, $"{player.Name} оштрафовал {target.Name} на {sum}$ ({reason})", true);
                GameLog.Ticket(Main.Players[player].UUID, Main.Players[target].UUID, sum, reason, player.Name, target.Name);
            }
            else
            {
                Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы отказались платить штраф в размере {sum}$ за {reason}", 3000);
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{target.Name} отказался платить штраф в размере {sum}$ за {reason}", 3000);
            }
        }
        public static void arrestTarget(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "arrest")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно применить на себе", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть возле камеры", 3000);
                return;
            }
            if (Main.Players[target].ArrestTime != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок уже в тюрьме", 3000);
                return;
            }
            if (Main.Players[target].WantedLVL == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не в розыске", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не в наручниках", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                unFollow(target.GetData("FOLLOWING"), target);
            }
            unCuffPlayer(target);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы посадили игрока ({target.Value}) на {Main.Players[target].WantedLVL.Level * 20} минут", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) посадил Вас на {Main.Players[target].WantedLVL.Level * 20} минут", 3000);
            Commands.RPChat("me", player, " поместил {name} в КПЗ", target);
            Manager.sendFractionMessage(7, $"{player.Name} посадил в КПЗ {target.Name} ({Main.Players[target].WantedLVL.Reason})", true);
            Manager.sendFractionMessage(9, $"{player.Name} посадил в КПЗ {target.Name} ({Main.Players[target].WantedLVL.Reason})", true);
            Main.Players[target].ArrestTime = Main.Players[target].WantedLVL.Level * 20 * 60;
            GameLog.Arrest(Main.Players[player].UUID, Main.Players[target].UUID, Main.Players[target].WantedLVL.Reason, Main.Players[target].WantedLVL.Level, player.Name, target.Name);
            arrestPlayer(target);
        }

        public static void releasePlayerFromPrison(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "rfp")) return;
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно применить на себе", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть возле камеры", 3000);
                return;
            }
            if (Main.Players[target].ArrestTime == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не в тюрьме", 3000);
                return;
            }
            freePlayer(target);
            Main.Players[target].ArrestTime = 0;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы освободили игрока ({target.Value}) из тюрьмы", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) освободил Вас из тюрьмы", 3000);
            Commands.RPChat("me", player, " освободил {name} из КПЗ", target);
        }

        public static void arrestTimer(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].ArrestTime == 0)
                {
                    freePlayer(player);
                    return;
                }
                Main.Players[player].ArrestTime--;
            } catch(Exception e)
            {
                Log.Write("ARRESTTIMER: " + e.ToString(), nLog.Type.Error);
            }
            
        }

        public static void freePlayer(Client player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("ARREST_TIMER")) return;
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER")); // still not fixed
                    NAPI.Data.ResetEntityData(player, "ARREST_TIMER");
                    Police.setPlayerWantedLevel(player, null);
                    NAPI.Entity.SetEntityPosition(player, Police.policeCheckpoints[5]);
                    NAPI.Entity.SetEntityDimension(player, 0);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы были освобождены из тюрьмы", 3000);
                }
                catch { }
            });
        }

        public static void arrestPlayer(Client target)
        {
            NAPI.Entity.SetEntityPosition(target, Police.policeCheckpoints[4]);
            Police.setPlayerWantedLevel(target, null);
            //NAPI.Data.SetEntityData(target, "ARREST_TIMER", Main.StartT(1000, 1000, (o) => arrestTimer(target), "ARREST_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.Start(1000, () => arrestTimer(target)));
            Weapons.RemoveAll(target, true);
        }

        public static void unCuffPlayer(Client player)
        {
            Trigger.ClientEvent(player, "CUFFED", false);
            NAPI.Data.SetEntityData(player, "CUFFED", false);
            NAPI.Player.StopPlayerAnimation(player);
            BasicSync.DetachObject(player);
            Trigger.ClientEvent(player, "blockMove", false);
            Main.OffAntiAnim(player);
        }

        [RemoteEvent("playerPressFollowBut")]
        public void ClientEvent_playerPressFollow(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Manager.canUseCommand(player, "follow", false)) return;
                if (player.HasData("FOLLOWER"))
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отпустили игрока ({player.GetData("FOLLOWER").Value})", 3000);
                    Notify.Send(player.GetData("FOLLOWER"), NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) отпустил Вас", 3000);
                    unFollow(player, player.GetData("FOLLOWER"));
                }
                else
                {
                    var target = Main.GetNearestPlayer(player, 2);
                    if (target == null || !Main.Players.ContainsKey(target)) return;
                    targetFollowPlayer(player, target);
                }
            }
            catch (Exception e) { Log.Write($"PlayerPressFollow: {e.ToString()} // {e.TargetSite} // ", nLog.Type.Error); }
        }

        public static void unFollow(Client cop, Client suspect)
        {
            NAPI.Data.ResetEntityData(cop, "FOLLOWER");
            NAPI.Data.ResetEntityData(suspect, "FOLLOWING");
            Trigger.ClientEvent(suspect, "setFollow", false);
        }

        public static void targetFollowPlayer(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "follow")) return;
            var fracid = Main.Players[player].FractionID;
            if (Manager.FractionTypes[fracid] == 2) // for gov factions
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны сначала начать рабочий день", 3000);
                    return;
                }
            }
            if (player.IsInVehicle || target.IsInVehicle) return;

            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно применить на себе", 3000);
                return;
            }

            if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже тащите за собой игрока", 3000);
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }

            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не в наручниках", 3000);
                return;
            }

            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока уже тащат", 3000);
                return;
            }

            NAPI.Data.SetEntityData(player, "FOLLOWER", target);
            NAPI.Data.SetEntityData(target, "FOLLOWING", player);
            Trigger.ClientEvent(target, "setFollow", true, player);
            Commands.RPChat("me", player, "потащил(а) {name} за собой", target);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы потащили за собой игрока ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) потащил Вас за собой", 3000);
        }
        public static void targetUnFollowPlayer(Client player)
        {
            if (!Manager.canUseCommand(player, "follow")) return;
            var fracid = Main.Players[player].FractionID;
            if (!NAPI.Data.HasEntityData(player, "FOLLOWER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы никого не тащите за собой", 3000);
                return;
            }
            Client target = NAPI.Data.GetEntityData(player, "FOLLOWER");
            unFollow(player, target);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отпустили игрока ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) отпустил Вас", 3000);
        }

        public static void suPlayer(Client player, int pasport, int stars, string reason)
        {
            if (!Manager.canUseCommand(player, "su")) return;
            if (!Main.PlayerNames.ContainsKey(pasport))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Паспорта с таким номером не существует", 3000);
                return;
            }
            Client target = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Владелец паспорта должен быть в сети", 3000);
                return;
            }
            if (player != target)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок в тюрьме", 3000);
                    return;
                }

                if (stars > 5)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете выдать такое кол-во звёзд", 3000);
                    return;
                }

                if (Main.Players[target].WantedLVL == null || Main.Players[target].WantedLVL.Level + stars <= 5)
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы объявили игрока " + target.Name.Replace('_', ' ') + " в розыск", 3000);
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"{player.Name.Replace('_', ' ')} объявил Вас в розыск ({reason})", 3000);
                    var oldStars = (Main.Players[target].WantedLVL == null) ? 0 : Main.Players[target].WantedLVL.Level;
                    var wantedLevel = new WantedLevel(oldStars + stars, player.Name, DateTime.Now, reason);
                    Police.setPlayerWantedLevel(target, wantedLevel);
                    return;
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете выдать такое кол-во звёзд", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете объявить в розыск самого себя", 3000);
        }

        // Садит игрока в машину
        public static void playerInCar(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "incar")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно использовать на себе", 3000);
                return;
            }
            var vehicle = VehicleManager.getNearestVehicle(player, 3);
            if (vehicle == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Рядом нет машин", 3000);
                return;
            }
            if (player.VehicleSeat != -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть на водительском месте", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок должен быть в наручниках", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                var cop = NAPI.Data.GetEntityData(target, "FOLLOWING");
                unFollow(cop, target);
            }

            var emptySlots = new List<int>
            {
                2,
                1,
                0
            };

            var players = NAPI.Pools.GetAllPlayers();
            foreach (var p in players)
            {
                if (p == null || !p.IsInVehicle || p.Vehicle != vehicle) continue;
                if (emptySlots.Contains(p.VehicleSeat)) emptySlots.Remove(p.VehicleSeat);
            }

            if (emptySlots.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине нет места", 3000);
                return;
            }

            NAPI.Player.SetPlayerIntoVehicle(target, vehicle, emptySlots[0]);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы запихали игрока ({target.Value}) в машину", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) запихал Вас в машину", 3000);
            Commands.RPChat("me", player, " открыл дверь и усадил {name} в машину", target);
        }

        public static void playerOutCar(Client player, Client target)
        {
            if (player != target)
            {
                if (!Manager.canUseCommand(player, "pull")) return;
                Vector3 posPlayer = NAPI.Entity.GetEntityPosition(player);
                Vector3 posTarget = NAPI.Entity.GetEntityPosition(target);
                if (player.Position.DistanceTo(target.Position) < 5)
                {
                    if (NAPI.Player.IsPlayerInAnyVehicle(target))
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выкинули игрока ({target.Value}) из машины", 3000);
                        Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) Выкинул Вас из машины", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(target);
                        Commands.RPChat("me", player, " открыл дверь и вытащил {name} из машины", target);
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не в машине", 3000);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете Выкинуть сами себя из машины", 3000);
        }

        public static void setWargPoliceMode(Client player)
        {
            if (!Manager.canUseCommand(player, "warg"))
            {
                return;
            }
            if (Main.Players[player].FractionID == 7)
            {
                var message = "";
                Police.is_warg = !Police.is_warg;
                if (Police.is_warg) message = $"{NAPI.Player.GetPlayerName(player)} объявил режим ЧП!!!";
                else message = $"{NAPI.Player.GetPlayerName(player)} отключил режим ЧП.";
                Manager.sendFractionMessage(7, message);
            }
            else if (Main.Players[player].FractionID == 9)
            {
                var message = "";
                Fbi.warg_mode = !Fbi.warg_mode;
                if (Fbi.warg_mode) message = $"{NAPI.Player.GetPlayerName(player)} объявил режим ЧП!!!";
                else message = $"{NAPI.Player.GetPlayerName(player)} отключил режим ЧП.";
                Manager.sendFractionMessage(9, message);
            }

        }

        public static void takeGunLic(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "takegunlic")) return;
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!Main.Players[target].Licenses[6])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет лицензии на оружие", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы отобрали лицензию на оружие у игрока ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) отобрал у Вас лицензию на оружие", 3000);
            Main.Players[target].Licenses[6] = false;
            Dashboard.sendStats(target);
        }

        public static void giveGunLic(Client player, Client target, int price)
        {
            if (!Manager.canUseCommand(player, "givegunlic")) return;
            if (player == target) return;
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (price < 5000 || price > 6000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Цена некорректна", 3000);
                return;
            }
            if (Main.Players[target].Licenses[6])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть лицензия на оружие", 3000);
                return;
            }
            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно средств", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили купить лицензию на оружие игроку ({target.Value}) за ${price}", 3000);

            Trigger.ClientEvent(target, "openDialog", "GUN_LIC", $"Игрок ({player.Value}) предложил Вам купить лицензию на оружие за ${price}");
            target.SetData("SELLER", player);
            target.SetData("GUN_PRICE", price);
        }

        public static void acceptGunLic(Client player)
        {
            if (!Main.Players.ContainsKey(player)) return;

            Client seller = player.GetData("SELLER");
            if (!Main.Players.ContainsKey(seller)) return;
            int price = player.GetData("GUN_PRICE");
            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Продавец слишком далеко", 3000);
                return;
            }

            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }

            MoneySystem.Wallet.Change(seller, price / 20);
            Fractions.Stocks.fracStocks[6].Money += Convert.ToInt32(price * 0.95);
            GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", price, $"buyGunlic({Main.Players[seller].UUID})");
            GameLog.Money($"frac(6)", $"player({Main.Players[seller].UUID})", price / 20, $"sellGunlic({Main.Players[player].UUID})");

            Main.Players[player].Licenses[6] = true;
            Dashboard.sendStats(player);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили лицензию на оружие у игрока ({seller.Value}) за {price}$", 3000);
            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) купил у Вас лицензию на оружие", 3000);
        }

        public static void playerTakeoffMask(Client player, Client target)
        {
            if (player.IsInVehicle || target.IsInVehicle) return;

            if (!target.HasData("IS_MASK") || !target.HasData("IS_MASK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет маски", 3000);
                return;
            }

            var maskItem = nInventory.Items[Main.Players[target].UUID].FirstOrDefault(i => i.Type == ItemType.Mask && i.IsActive);
            nInventory.Remove(target, maskItem);
            Customization.CustomPlayerData[Main.Players[target].UUID].Clothes.Mask = new ComponentItem(0, 0);
            if (maskItem != null) Items.onDrop(player, maskItem, null);

            Customization.SetMask(target, 0, 0);;

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сорвали маску с игрока ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) сорвал с Вас маску", 3000);
            Commands.RPChat("me", player, " сорвал маску с {name}", target);
        }
        #endregion

        #region crimeCommands
        public static void robberyTarget(Client player, Client target)
        {
            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

            if (!target.GetData("CUFFED") && !target.HasData("HANDS_UP"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок должен быть связан или с поднятыми руками", 3000);
                return;
            }

            if (!player.HasData("IS_MASK") || !player.GetData("IS_MASK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ограбление возможно только в маске", 3000);
                return;
            }

            if (Main.Players[target].LVL < 2 || Main.Players[target].Money <= 1000 || (target.HasData("NEXT_ROB") && DateTime.Now < target.GetData("NEXT_ROB")))
            {
                Commands.RPChat("me", player, "хорошенько обшарив {name}, ничего не нашёл", target);
                return;
            }

            var max = (Main.Players[target].Money >= 3000) ? 3000 : Convert.ToInt32(Main.Players[target].Money) - 200;
            var min = (max - 200 < 0) ? max : max - 200;

            var found = Main.rnd.Next(min, max + 1);
            MoneySystem.Wallet.Change(target, -found);
            MoneySystem.Wallet.Change(player, found);
            GameLog.Money($"player({Main.Players[target].UUID})", $"player({Main.Players[player].UUID})", found, $"robbery");
            target.SetData("NEXT_ROB", DateTime.Now.AddMinutes(60));

            Commands.RPChat("me", player, "хорошенько обшарив {name}" + $", нашёл ${found}", target);
        }
        public static void playerChangePocket(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "pocket")) return;
            if (player.IsInVehicle) return;
            if (target.IsInVehicle) return;

            if (target.HasData("HEAD_POCKET"))
            {
                target.ClearAccessory(1);
                target.SetClothes(1, 0, 0);

                Trigger.ClientEvent(target, "setPocketEnabled", false);
                target.ResetData("HEAD_POCKET");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли мешок с игрока ({target.Value})", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) снял с Вас мешок", 3000);
                Commands.RPChat("me", player, "снял(а) мешок с {name}", target);
            }
            else
            {
                if (nInventory.Find(Main.Players[player].UUID, ItemType.Pocket) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет мешков", 3000);
                    return;
                }

                target.SetAccessories(1, 24, 2);
                target.SetClothes(1, 56, 1);

                Trigger.ClientEvent(target, "setPocketEnabled", true);
                target.SetData("HEAD_POCKET", true);

                nInventory.Remove(player, ItemType.Pocket, 1);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы надели мешок на игрока ({target.Value})", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) надел на Вас мешок", 3000);
                Commands.RPChat("me", player, "надел(а) мешок на {name}", target);
            }
        }
        #endregion
        
        #region EMS commands
        public static void giveMedicalLic(Client player, Client target)
        {
            if (!Manager.canUseCommand(player, "givemedlic")) return;

            if (Main.Players[target].Licenses[7])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть мед. карта", 3000);
                return;
            }

            Main.Players[target].Licenses[7] = true;
            GUI.Dashboard.sendStats(target);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} медицинскую карту", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} выдал Вам медицинскую карту", 3000);
        }
        public static void sellMedKitToTarget(Client player, Client target, int price)
        {
            if (Manager.canUseCommand(player, "medkit"))
            {
                if (!player.GetData("ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                    return;
                }
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                if (item == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны взять аптечки со склада", 3000);
                    return;
                }
                if (price < 500 || price > 1500)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны установить цену от 500$ до 1500$", 3000);
                    return;
                }
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                if (Main.Players[target].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет столько денег", 3000);
                    return;
                }
                Trigger.ClientEvent(target, "openDialog", "PAY_MEDKIT", $"Медик ({player.Value}) предложил купить Вам аптечку за ${price}.");
                target.SetData("SELLER", player);
                target.SetData("PRICE", price);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили купить игроку ({target.Value}) аптечку за {price}$", 3000);
            }
        }

        public static void acceptEMScall(Client player, Client target)
        {
            if (Manager.canUseCommand(player, "accept"))
            {
                if (!player.GetData("ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не начали рабочий день", 3000);
                    return;
                }
                if (!target.HasData("IS_CALL_EMS"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не вызывал скорую", 3000);
                    return;
                }
                Trigger.ClientEvent(player, "createWaypoint", target.Position.X, target.Position.Y);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Медик ({player.Value}) принял Ваш вызов", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы приняли вызов игрока ({target.Value})", 3000);
                target.ResetData("IS_CALL_EMS");
                return;
            }
        }

        public static void healTarget(Client player, Client target, int price)
        {
            if (Manager.canUseCommand(player, "heal"))
            {
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                if (price < 50 || price > 400)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны установить цену от 50$ до 400$", 3000);
                    return;
                }
                if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Player.IsPlayerInAnyVehicle(target))
                {
                    var pveh = player.Vehicle;
                    var tveh = target.Vehicle;
                    Vehicle veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(pveh);
                    if (veh.GetData("ACCESS") != "FRACTION" || veh.GetData("TYPE") != "EMS" || !veh.HasData("CANMEDKITS"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сидите не в карете EMS", 3000);

                        return;
                    }
                    if (pveh != tveh)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок сидит в другой машине", 3000);
                        return;
                    }
                    target.SetData("SELLER", player);
                    target.SetData("PRICE", price);
                    Trigger.ClientEvent(target, "openDialog", "PAY_HEAL", $"Медик ({player.Value}) предложил лечение за ${price}");

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили лечение игроку ({target.Value}) за {price}$", 3000);
                    return;
                }
                else if (player.GetData("IN_HOSPITAL") && target.GetData("IN_HOSPITAL"))
                {
                    target.SetData("SELLER", player);
                    target.SetData("PRICE", price);
                    Trigger.ClientEvent(target, "openDialog", "PAY_HEAL", $"Медик ({player.Value}) предложил лечение за ${price}");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили лечение игроку ({target.Value}) за {price}$", 3000);
                    return;
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть в больнице или корете скорой помощи", 3000);;
                    return;
                }
            }
        }

        #endregion

    }
}