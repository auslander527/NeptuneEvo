using System;
using System.Collections.Generic;
using System.Data;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using NeptuneEvo.Core.Character;
using Newtonsoft.Json;

namespace NeptuneEvo.Fractions
{
    class Police : Script
    {
        private static nLog Log = new nLog("Police");

        
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> policeCheckpoints = new List<Vector3>()
        {
            new Vector3(463.2361, -998.3675, 23.91487), // shape, where player can arrest suspect       0
            new Vector3(452.4479, -980.1406, 29.68958), // guns     1
            new Vector3(457.2408, -988.3513, 29.68959), // dressing room     2
            new Vector3(452.2765, -993.3061, 29.68959), // special checkpoint     3
            new Vector3(459.188, -997.7607, 23.91485), // prison room      4 
            new Vector3(428.478, -979.8452, 30.71022), // place of release from prison     5
            new Vector3(441.9336, -981.5965, 29.6896), // buy gun licence     6
            new Vector3(439.8187, -981.6356, 29.5696), // surrender bags with drill and money     7
            new Vector3(461.0604, -981.0191, 29.56959),  // open stock     8
            new Vector3(452.1674, -1024.298, 27.40882),  // vehicle boost       9
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_arm_secdoor"), new Vector3(453.0793, -983.1894, 30.83926), 30f);

                NAPI.TextLabel.CreateTextLabel("~g~Alonzo Harris", new Vector3(452.2527, -993.119, 31.6896), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                NAPI.TextLabel.CreateTextLabel("~g~Nancy Spungen", new Vector3(441.169, -978.3074, 31.6896), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                NAPI.TextLabel.CreateTextLabel("~g~Bones Bulldog", new Vector3(454.121, -980.0575, 31.6896), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[0], 6, 3, 0));
                Cols[0].OnEntityEnterColShape += arrestShape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += arrestShape_onEntityExitColShape;

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 10);
                Cols[1].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[1].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to open gun menu"), new Vector3(policeCheckpoints[1].X, policeCheckpoints[1].Y, policeCheckpoints[1].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[2], 1, 2, 0));
                Cols[2].SetData("INTERACT", 11);
                Cols[2].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[2].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to change clothes"), new Vector3(policeCheckpoints[2].X, policeCheckpoints[2].Y, policeCheckpoints[2].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[3], 1, 2, 0));
                Cols[3].SetData("INTERACT", 12);
                Cols[3].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[3].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to open ES menu"), new Vector3(policeCheckpoints[3].X, policeCheckpoints[3].Y, policeCheckpoints[3].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[7], 1, 2, 0));
                Cols[5].SetData("INTERACT", 42);
                Cols[5].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[5].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Hand over bags and get a reward"), new Vector3(policeCheckpoints[7].X, policeCheckpoints[7].Y, policeCheckpoints[7].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[8], 1, 2, 0));
                Cols[6].SetData("INTERACT", 59);
                Cols[6].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[6].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Open gun stock"), new Vector3(policeCheckpoints[8].X, policeCheckpoints[8].Y, policeCheckpoints[8].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[9], 4, 5, 0));
                Cols[7].SetData("INTERACT", 66);
                Cols[7].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[7].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Boost"), new Vector3(policeCheckpoints[9].X, policeCheckpoints[9].Y, policeCheckpoints[9].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                NAPI.Marker.CreateMarker(1, policeCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[7] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[8] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[9] - new Vector3(0, 0, 3.7), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 220));
            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.VehicleSeat != -1 || player.VehicleSeat != 0) return;
                if (Main.Players[player].FractionID != 7 || Main.Players[player].FractionID != 9) return;
                Trigger.ClientEvent(player, "closePc");
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void callPolice(Client player, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try {
                    if (Manager.countOfFractionMembers(7) == 0 && Manager.countOfFractionMembers(9) == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет полицейских в Вашем районе. Попробуйте позже", 3000);
                        return;
                    }
                    if (player.HasData("NEXTCALL_POLICE") && DateTime.Now < player.GetData("NEXTCALL_POLICE"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы уже вызвали полицию, попробуйте позже", 3000);
                        return;
                    }
                    player.SetData("NEXTCALL_POLICE", DateTime.Now.AddMinutes(7));

                    if (player.HasData("CALLPOLICE_BLIP"))
                        NAPI.Entity.DeleteEntity(player.GetData("CALLPOLICE_BLIP"));

                    var Blip = NAPI.Blip.CreateBlip(0, player.Position, 1, 70, "Call from " + player.Name.Replace('_', ' ') + $" ({player.Value})", 0, 0, true, 0, 0);
                    Blip.Transparency = 0;
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                        {
                            if (!Main.Players.ContainsKey(p)) continue;
                            if (Main.Players[p].FractionID != 7 && Main.Players[p].FractionID != 9) continue;
                            p.TriggerEvent("changeBlipAlpha", Blip, 255);
                        }
                    player.SetData("CALLPOLICE_BLIP", Blip);

                    var colshape = NAPI.ColShape.CreateCylinderColShape(player.Position, 70, 4, 0);
                    colshape.OnEntityExitColShape += (s, e) =>
                    {
                        if (e == player)
                        {
                            try
                            {
                                Blip.Delete();
                                e.ResetData("CALLPOLICE_BLIP");

                                Manager.sendFractionMessage(7, $"{e.Name.Replace('_', ' ')} отменил вызов");
                                Manager.sendFractionMessage(9, $"{e.Name.Replace('_', ' ')} отменил вызов");

                                colshape.Delete();

                                e.ResetData("CALLPOLICE_COL");
                                e.ResetData("IS_CALLPOLICE");
                            }
                            catch (Exception ex) { Log.Write("EnterPoliceCall: " + ex.Message); }
                        }
                    };
                    player.SetData("CALLPOLICE_COL", colshape);

                    player.SetData("IS_CALLPOLICE", true);
                    Manager.sendFractionMessage(7, $"Поступил вызов от игрока ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(7, $"~b~Поступил вызов от игрока ({player.Value}) - {reason}", true);
                    Manager.sendFractionMessage(9, $"Поступил вызов от игрока ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(9, $"~b~Поступил вызов от игрока ({player.Value}) - {reason}", true);
                }
                catch { }
            });
        }

        public static void acceptCall(Client player, Client target)
        {
            try
            {
                if (!Manager.canUseCommand(player, "pd")) return;
                if(target == null || !NAPI.Entity.DoesEntityExist(target)) return;
                if (!target.HasData("IS_CALLPOLICE"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не вызывал полицию или этот вызов уже кто-то принял", 3000);
                    return;
                }
                Blip blip = target.GetData("CALLPOLICE_BLIP");

                Trigger.ClientEvent(player, "changeBlipColor", blip, 38);
                Trigger.ClientEvent(player, "createWaypoint", blip.Position.X, blip.Position.Y);

                ColShape colshape = target.GetData("CALLPOLICE_COL");
                colshape.OnEntityEnterColShape += (s, e) =>
                {
                    if (e == player)
                    {
                        try
                        {
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    NAPI.Entity.DeleteEntity(target.GetData("CALLPOLICE_BLIP"));
                                    target.ResetData("CALLPOLICE_BLIP");
                                    colshape.Delete();
                                }
                                catch { }
                            });
                        }
                        catch (Exception ex) { Log.Write("EnterPoliceCall: " + ex.Message); }
                    }
                };

                Manager.sendFractionMessage(7, $"{player.Name.Replace('_', ' ')} принял вызов от игрока ({target.Value})");
                Manager.sendFractionMessage(7, $"~b~{player.Name.Replace('_', ' ')} принял вызов от игрока ({target.Value})", true);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) принял Ваш вызов", 3000);
            }
            catch {
            }
        }
            
        [RemoteEvent("clearWantedLvl")]
        public static void clearWantedLvl(Client sender, params object[] arguments)
        {
            try
            {
                var target = (string)arguments[0];
                Client player = null;
                try
                {
                    var pasport = Convert.ToInt32(target);
                    if (!Main.PlayerNames.ContainsKey(pasport))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Паспорта с таким номером не существует", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
                    target = Main.PlayerNames[pasport];
                }
                catch
                {
                    target.Replace(' ', '_');
                    if (!Main.PlayerNames.ContainsValue(target))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Игрок не найден", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(target);
                }

                var split = target.Split('_');
                MySQL.Query($"UPDATE characters SET wanted=null WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                try
                {
                    setPlayerWantedLevel(player, null);
                }
                catch { }
                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomRight, $"Вы сняли розыск с владельца паспорта {target}", 3000);
            } catch (Exception e) { Log.Write("ClearWantedLvl: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("checkNumber")]
        public static void checkNumber(Client sender, params object[] arguments)
        {
            try
            {
                var number = (string)arguments[0];
                VehicleManager.VehicleData vehicle;
                try
                {
                    vehicle = VehicleManager.Vehicles[number];
                }
                catch
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Машины с таким номером не найдено", 3000);
                    return;
                }
                Trigger.ClientEvent(sender, "executeCarInfo", Convert.ToString(vehicle.Model), vehicle.Holder.Replace('_', ' '));
            }
            catch (Exception e) { Log.Write("checkNumber: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("checkPerson")]
        public static void checkPerson(Client sender, params object[] arguments)
        {
            try
            {
                var target = (string)arguments[0];
                Client player = null;
                try
                {
                    var pasport = Convert.ToInt32(target);
                    if (!Main.PlayerNames.ContainsKey(pasport))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Паспорта с таким номером не существует", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
                    target = Main.PlayerNames[pasport];
                }
                catch
                {
                    target.Replace(' ', '_');
                    if (!Main.PlayerNames.ContainsValue(target))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Игрок не найден", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(target);
                }
                
                try
                {
                    var acc = Main.Players[player];
                    var wantedLvl = (acc.WantedLVL == null) ? 0 : acc.WantedLVL.Level;
                    var gender = (acc.Gender) ? "Мужской" : "Женский";
                    var lic = "";
                    for (int i = 0; i < acc.Licenses.Count; i++)
                        if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                    if (lic == "") lic = "Отсутствуют";

                    Trigger.ClientEvent(sender, "executePersonInfo", $"{acc.FirstName}", $"{acc.LastName}", $"{acc.UUID}", $"{gender}", $"{wantedLvl}", $"{lic}");
                }
                catch
                {
                    var split = target.Split('_');
                    var result = MySQL.QueryRead($"SELECT * FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    foreach (DataRow Row in result.Rows)
                    {
                        var firstName = Convert.ToString(Row["firstname"]);
                        var lastName = Convert.ToString(Row["lastname"]);
                        var genderBool = Convert.ToBoolean(Row["gender"]);
                        var uuid = Convert.ToInt32(Row["uuid"].ToString());
                        var gender = (genderBool) ? "Мужской" : "Женский";
                        var wanted = Newtonsoft.Json.JsonConvert.DeserializeObject<WantedLevel>(Row["wanted"].ToString());
                        var wantedLvl = (wanted == null) ? 0 : wanted.Level;
                        var licenses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<bool>>(Convert.ToString(Row["licenses"]));
                        var lic = "";
                        for (int i = 0; i < licenses.Count; i++)
                            if (licenses[i]) lic += $"{Main.LicWords[i]} / ";
                        if (lic == "") lic = "Отсутствуют";

                        Trigger.ClientEvent(sender, "executePersonInfo", $"{firstName}", $"{lastName}", $"{uuid}",$"{gender}", $"{wantedLvl}", $"{lic}", "Лицензия на оружие", "Водительские права");
                    }
                }
            }
            catch (Exception e) { Log.Write("checkPerson: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("checkWantedList")]
        public static void checkWantedList(Client sender, params object[] arguments)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    var acc = Main.Players[p];
                    var wantedLvl = (acc.WantedLVL == null) ? 0 : acc.WantedLVL.Level;
                    if (wantedLvl != 0) list.Add($"{acc.FirstName} {acc.LastName} - {wantedLvl}*");
                }
                var json = JsonConvert.SerializeObject(list);
                Log.Debug(json);
                Trigger.ClientEvent(sender, "executeWantedList", json);
            }
            catch (Exception e) { Log.Write("checkWantedList: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("openCopCarMenu")]
        public static void openCopcarmenu(Client sender, params object[] arguments)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                var vehicle = sender.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "FRACTION" &&
                    (NAPI.Data.GetEntityData(vehicle, "FRACTION") == 7 || NAPI.Data.GetEntityData(vehicle, "FRACTION") == 9) &&
                    (sender.VehicleSeat == -1 || sender.VehicleSeat == 0))
                {
                    MenuManager.Close(sender);
                    if (Main.Players[sender].FractionID == 7 || Main.Players[sender].FractionID == 9)
                    {
                        Trigger.ClientEvent(sender, "openPc");
                        Commands.RPChat("me", sender, "включил(а) бортовой компьютер");
                    }
                }
                return;
            }
            catch (Exception e) { Log.Write("openCopCarMenu: " + e.Message, nLog.Type.Error); }
        }
        
        public static void Event_PlayerDeath(Client player, Client killer, uint reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    if (NAPI.Data.GetEntityData(player, "IN_CP_MODE"))
                    {
                        Manager.setSkin(player, Main.Players[player].FractionID, Main.Players[player].FractionLVL);
                        NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Client player, int interact)
        {
            if (!Main.Players.ContainsKey(player)) return;
            switch (interact)
            {
                case 10:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[7].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    OpenPoliceGunMenu(player);
                    return;
                case 11:
                    if (Main.Players[player].FractionID == 7)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы начали рабочий день", 3000);
                            Manager.setSkin(player, 7, Main.Players[player].FractionLVL);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                            break;
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);;
                            Customization.ApplyCharacter(player);
                            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                            break;
                        }
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                    return;
                case 12:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!is_warg)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не включен режим ЧП", 3000);
                        return;
                    }
                    OpenSpecialPoliceMenu(player);
                    return;
                case 42:
                    if (!player.HasData("HAND_MONEY") && !player.HasData("HEIST_DRILL"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ни сумки с деньгами, ни сумки с дрелью", 3000);
                        return;
                    }
                    if (player.HasData("HAND_MONEY"))
                    {
                        nInventory.Remove(player, ItemType.BagWithMoney, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HAND_MONEY");
                    }
                    if (player.HasData("HEIST_DRILL"))
                    {
                        nInventory.Remove(player, ItemType.BagWithDrill, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HEIST_DRILL");
                    }
                    MoneySystem.Wallet.Change(player, 200);
                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", 200, $"policeAward");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили вознаграждение в 200$", 3000);
                    return;
                case 44:
                    if (Main.Players[player].Licenses[6])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть лицензия на оружие", 3000);
                        return;
                    }
                    if (!MoneySystem.Wallet.Change(player, -30000))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств.", 3000);
                        return;
                    }
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили лицензию на оружие", 3000);
                    Main.Players[player].Licenses[6] = true;
                    Dashboard.sendStats(player);
                    return;
                case 59:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[7].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 7);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[7].Weapons, "Склад оружия", 6);
                    return;
                case 66:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!player.IsInVehicle || (player.Vehicle.Model != NAPI.Util.GetHashKey("police") && 
                        player.Vehicle.Model != NAPI.Util.GetHashKey("police2") && player.Vehicle.Model != NAPI.Util.GetHashKey("police3") && player.Vehicle.Model != NAPI.Util.GetHashKey("police4")))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в рабочей машине", 3000);
                        return;
                    }
                    Trigger.ClientEvent(player, "svem", 20, 20);
                    player.Vehicle.SetSharedData("BOOST", 20);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Пробущено", 3000);
                    return;
            }
        }

        #region shapes
        private void arrestShape_onEntityEnterColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "IS_IN_ARREST_AREA", true);
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void arrestShape_onEntityExitColShape(ColShape shape, Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                NAPI.Data.SetEntityData(player, "IS_IN_ARREST_AREA", false);
                if (Main.Players[player].ArrestTime != 0)
                {
                    NAPI.Entity.SetEntityPosition(player, Police.policeCheckpoints[4]);
                }
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityEnterColshape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData("INTERACT"));
            }
            catch (Exception ex) { Log.Write("onEntityEnterColshape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityExitColshape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("onEntityExitColshape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion

        public static void onPlayerDisconnectedhandler(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (NAPI.Data.HasEntityData(player, "ARREST_TIMER"))
                {
                    //Main.StopT(NAPI.Data.GetEntityData(player, "ARREST_TIMER"), "onPlayerDisconnectedhandler_arrest");
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER"));
                }

                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Client target = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    NAPI.Data.ResetEntityData(target, "FOLLOWER");
                }
                else if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Client target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    NAPI.Data.ResetEntityData(target, "FOLLOWING");
                    target.FreezePosition = false;
                    Trigger.ClientEvent(target, "follow", false);
                }
                
                if (player.HasData("CALLPOLICE_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(player.GetData("CALLPOLICE_BLIP"));

                    Manager.sendFractionMessage(7, $"{player.Name.Replace('_', ' ')} отменил вызов");
                    Manager.sendFractionMessage(9, $"{player.Name.Replace('_', ' ')} отменил вызов");
                }
                if (player.HasData("CALLPOLICE_COL"))
                {
                    NAPI.ColShape.DeleteColShape(player.GetData("CALLPOLICE_COL"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void setPlayerWantedLevel(Client player, WantedLevel wantedlevel)
        {
            Main.Players[player].WantedLVL = wantedlevel;
            if (wantedlevel != null) Trigger.ClientEvent(player, "setWanted", wantedlevel.Level);
            else Trigger.ClientEvent(player, "setWanted", 0);
        }

        public static bool is_warg = false;

        #region menus
        public static void OpenPoliceGunMenu(Client player)
        {
            Trigger.ClientEvent(player, "policeg");
        }
        [RemoteEvent("lspdgun")]
        public static void callback_policeGuns(Client client, int index)
        {
            try
            {
                switch (index)
                {
                    case 0: //nightstick
                        Fractions.Manager.giveGun(client, Weapons.Hash.Nightstick, "Nightstick");
                        return;
                    case 1: //pistol
                        Fractions.Manager.giveGun(client, Weapons.Hash.Pistol, "Pistol");
                        return;
                    case 2: //smg
                        Fractions.Manager.giveGun(client, Weapons.Hash.SMG, "SMG");
                        return;
                    case 3: //pumpshotgun
                        Fractions.Manager.giveGun(client, Weapons.Hash.PumpShotgun, "PumpShotgun");
                        return;
                    case 4: //stungun
                        Fractions.Manager.giveGun(client, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(client, "armor")) return;
                        if (Fractions.Stocks.fracStocks[7].Materials < Fractions.Manager.matsForArmor)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "На складе недостаточно материала", 3000);
                            return;
                        }
                        var aItem = nInventory.Find(Main.Players[client].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[7].Materials -= Fractions.Manager.matsForArmor;
                        Fractions.Stocks.fracStocks[7].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "armor", 1, false);
                        return;
                    case 6: // medkit
                        if (!Manager.canGetWeapon(client, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[7].Medkits == 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "На складе нет аптечек", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[client].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть аптечка", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[7].Medkits--;
                        Fractions.Stocks.fracStocks[7].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 7: // pistol ammo
                        if (!Manager.canGetWeapon(client, "PistolAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.PistolAmmo, 12);
                        return;
                    case 8: // smg ammo
                        if (!Manager.canGetWeapon(client, "SMGAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.SMGAmmo, 30);
                        return;
                    case 9: // shotgun ammo
                        if (!Manager.canGetWeapon(client, "ShotgunsAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.ShotgunsAmmo, 6);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Write($"Lspdgun: " + e.Message, nLog.Type.Error);
            }
        }

        public static void OpenSpecialPoliceMenu(Client player)
        {
            Menu menu = new Menu("policeSpecial", false, false);
            menu.Callback += callback_policeSpecial;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Оружейная";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changeclothes", Menu.MenuItem.Button);
            menuItem.Text = "Переодеться";
            menu.Add(menuItem);

            menuItem = new Menu.Item("pistol50", Menu.MenuItem.Button);
            menuItem.Text = "Desert Eagle";
            menu.Add(menuItem);

            menuItem = new Menu.Item("carbineRifle", Menu.MenuItem.Button);
            menuItem.Text = "Штурмовая винтовка";
            menu.Add(menuItem);

            menuItem = new Menu.Item("riflesammo", Menu.MenuItem.Button);
            menuItem.Text = "Автоматный калибр x30";
            menu.Add(menuItem);

            menuItem = new Menu.Item("heavyshotgun", Menu.MenuItem.Button);
            menuItem.Text = "Тяжелый дробовик";
            menu.Add(menuItem);
            
            menuItem = new Menu.Item("stungun", Menu.MenuItem.Button);
            menuItem.Text = "Tazer";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_policeSpecial(Client client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "changeclothes":
                    if (!NAPI.Data.GetEntityData(client, "IN_CP_MODE"))
                    {
                        bool gender = Main.Players[client].Gender;
                        Customization.ApplyCharacter(client);
                        Customization.ClearClothes(client, gender);
                        if (gender)
                        {
                            Customization.SetHat(client, 39, 0);
                            //client.SetClothes(1, 52, 0);
                            client.SetClothes(11, 53, 0);
                            client.SetClothes(4, 31, 0);
                            client.SetClothes(6, 25, 0);
                            client.SetClothes(9, 15, 2);
                            client.SetClothes(3, 49, 0);
                        }
                        else
                        {
                            Customization.SetHat(client, 38, 0);
                            //client.SetClothes(1, 57, 0);
                            client.SetClothes(11, 46, 0);
                            client.SetClothes(4, 30, 0);
                            client.SetClothes(6, 25, 0);
                            client.SetClothes(9, 17, 2);
                            client.SetClothes(3, 53, 0);
                        }
                        if (client.HasData("HAND_MONEY")) client.SetClothes(5, 45, 0);
                        else if (client.HasData("HEIST_DRILL")) client.SetClothes(5, 41, 0);
                        NAPI.Data.SetEntityData(client, "IN_CP_MODE", true);
                        return;
                    }
                    Fractions.Manager.setSkin(client, 7, Main.Players[client].FractionLVL);
                    client.SetData("IN_CP_MODE", false);
                    return;
                case "pistol50":
                    Fractions.Manager.giveGun(client, Weapons.Hash.Pistol50, "pistol50");
                    return;
                case "carbineRifle":
                    Fractions.Manager.giveGun(client, Weapons.Hash.CarbineRifle, "carbineRifle");
                    return;
                case "riflesammo":
                    if (!Manager.canGetWeapon(client, "RiflesAmmo")) return;
                    Fractions.Manager.giveAmmo(client, ItemType.RiflesAmmo, 30);
                    return;
                case "heavyshotgun":
                    Fractions.Manager.giveGun(client, Weapons.Hash.HeavyShotgun, "heavyshotgun");
                    return;
                case "stungun":
                    Fractions.Manager.giveGun(client, Weapons.Hash.StunGun, "stungun");
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }
        #endregion
    }
}
