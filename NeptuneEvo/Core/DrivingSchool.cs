using System.Collections.Generic;
using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEvo.GUI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class DrivingSchool : Script
    {
        // мотоциклы, легковые машины, грузовые, водный, вертолёты, самолёты
        private static List<int> LicPrices = new List<int>() { 600, 1000, 3000, 6000, 10000, 10000 };
        private static Vector3 enterSchool = new Vector3(-712.2147, -1298.926, 4.101922);
        private static List<Vector3> startCourseCoord = new List<Vector3>()
        {
            new Vector3(-718.1355, -1291.299, 3.57617),
            new Vector3(-718.1355, -1291.299, 3.57617),
            new Vector3(-718.1355, -1291.299, 3.57617),
        };
        private static List<Vector3> startCourseRot = new List<Vector3>()
        {
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
        };
        private static List<Vector3> drivingCoords = new List<Vector3>()
        {
            new Vector3(-704.6472, -1247.343, 10.67442),     //as1
            new Vector3(-686.6135, -1252.129, 11.03791),     //as2
            new Vector3(-668.1112, -1277.741, 11.11878),     //as3
            new Vector3(-647.9838, -1297.883, 11.11266),     //as4
            new Vector3(-631.2891, -1299.402, 11.10439),     //as5
            new Vector3(-612.2523, -1283.693, 11.10192),     //as6
            new Vector3(-582.577, -1247.959, 13.36889),     //as7
            new Vector3(-551.6786, -1204.123, 18.13077),     //as8
            new Vector3(-540.2383, -1181.224, 19.18878),     //as9
            new Vector3(-510.3582, -1179.808, 20.25262),     //as10
            new Vector3(-489.511, -1218.502, 21.78694),     //as11
            new Vector3(-515.7674, -1296.293, 28.07829),     //as12
            new Vector3(-534.27, -1349.218, 29.60077),     //as13
            new Vector3(-519.2154, -1400.633, 29.65829),     //as14
            new Vector3(-474.7718, -1426.947, 29.6368),     //as15
            new Vector3(-438.2497, -1434.056, 29.6976),     //as16
            new Vector3(-392.2361, -1440.421, 29.69909),     //as17
            new Vector3(-355.8693, -1440.931, 29.81322),     //as18
            new Vector3(-302.8323, -1441.081, 31.73903),     //as19
            new Vector3(-286.3747, -1453.478, 31.72387),     //as20
            new Vector3(-292.7627, -1507.852, 29.64017),     //as21
            new Vector3(-329.7639, -1587.84, 22.43151),     //as22
            new Vector3(-383.8394, -1681.795, 19.32527),     //as23
            new Vector3(-411.5873, -1749.223, 20.53662),     //as24
            new Vector3(-428.1123, -1765.016, 20.96091),     //as25
            new Vector3(-484.4346, -1776.815, 21.43759),     //as26
            new Vector3(-550.9955, -1747.087, 22.35854),     //as27
            new Vector3(-609.9813, -1705.323, 24.43946),     //as28
            new Vector3(-654.9661, -1664.971, 25.63946),     //as29
            new Vector3(-701.2487, -1616.159, 23.39617),     //as30
            new Vector3(-715.5563, -1603.546, 22.83745),     //as31
            new Vector3(-736.1861, -1612.059, 24.43959),     //as32
            new Vector3(-763.5643, -1642.022, 27.34238),     //as33
            new Vector3(-771.7167, -1694.855, 29.66684),     //as34
            new Vector3(-770.2577, -1722.702, 29.6922),     //as35
            new Vector3(-762.8403, -1745.841, 29.74304),     //as36
            new Vector3(-754.4462, -1759.328, 29.74207),     //as37
            new Vector3(-728.3046, -1759.528, 30.03871),     //as38
            new Vector3(-661.0937, -1753.61, 37.98396),     //as39
            new Vector3(-559.1539, -1723.255, 37.78191),     //as40
            new Vector3(-510.9143, -1702.245, 37.25971),     //as41
            new Vector3(-444.6659, -1643.36, 32.99823),     //as42
            new Vector3(-392.0374, -1524.149, 25.89943),     //as43
            new Vector3(-384.0456, -1396.677, 22.64989),     //as44
            new Vector3(-389.9767, -1305.895, 22.36889),     //as45
            new Vector3(-390.8521, -1190.269, 21.19579),     //as46
            new Vector3(-390.8799, -1070.524, 22.41175),     //as47
            new Vector3(-382.655, -914.3037, 35.94333),     //as48
            new Vector3(-384.1105, -832.9343, 39.32611),     //as49
            new Vector3(-392.9055, -739.7156, 37.61785),     //as50
            new Vector3(-393.7488, -666.5231, 37.62029),     //as51
            new Vector3(-393.5527, -604.5363, 33.92249),     //as52
            new Vector3(-404.7963, -508.064, 34.13367),     //as53
            new Vector3(-441.523, -476.5334, 33.31015),     //as54
            new Vector3(-523.574, -468.2849, 32.58505),     //as55
            new Vector3(-609.9857, -475.7358, 35.1473),     //as56
            new Vector3(-632.9848, -477.7071, 35.26155),     //as57
            new Vector3(-644.2718, -491.5341, 35.19373),     //as58
            new Vector3(-644.6234, -541.7328, 35.1782),     //as59
            new Vector3(-645.2361, -570.4226, 35.4185),     //as60
            new Vector3(-640.3406, -635.2014, 32.45972),     //as61
            new Vector3(-640.1005, -683.8199, 31.69821),     //as62
            new Vector3(-640.896, -779.8826, 25.85054),     //as63
            new Vector3(-640.9213, -809.8345, 25.52384),     //as64
            new Vector3(-645.9667, -859.3205, 25.0564),     //as65
            new Vector3(-646.494, -939.6482, 22.5421),     //as66
            new Vector3(-649.717, -1000.834, 20.64941),     //as67
            new Vector3(-752.1108, -1101.364, 11.19446),     //as68
            new Vector3(-769.0291, -1141.633, 11.00855),     //as69
            new Vector3(-731.3914, -1191.951, 11.05298),     //as70
            new Vector3(-703.2067, -1227.076, 11.04342),     //as71
            new Vector3(-709.2208, -1240.626, 10.70718),     //as72
        };

        private static nLog Log = new nLog("DrivingSc");
        
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(enterSchool, 1, 2, 0);
                shape.OnEntityEnterColShape += onPlayerEnterSchool;
                shape.OnEntityExitColShape += onPlayerExitSchool;

                NAPI.Marker.CreateMarker(1, enterSchool - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Driving School"), new Vector3(enterSchool.X, enterSchool.Y, enterSchool.Z + 1), 5f, 0.3f, 0, new Color(255, 255, 255));
                var blip = NAPI.Blip.CreateBlip(enterSchool, 0);
                blip.ShortRange = true;
                blip.Name = Main.StringToU16("Driving School");
                blip.Sprite = 545;
                blip.Color = 29;
                for (int i = 0; i < drivingCoords.Count; i++)
                {
                    var colshape = NAPI.ColShape.CreateCylinderColShape(drivingCoords[i], 4, 5, 0);
                    colshape.OnEntityEnterColShape += onPlayerEnterDrive;
                    colshape.SetData("NUMBER", i);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("SCHOOLVEH") && player.GetData("SCHOOLVEH") == vehicle)
                {
                    //player.SetData("SCHOOL_TIMER", Main.StartT(60000, 99999999, (o) => timer_exitVehicle(player), "SCHOOL_TIMER"));
                    player.SetData("SCHOOL_TIMER", Timers.StartOnce(60000, () => timer_exitVehicle(player)));

                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если вы не сядете в машину в течение 60 секунд, то провалите экзамен", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private void timer_exitVehicle(Client player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    if (!player.HasData("SCHOOLVEH")) return;
                    if (player.IsInVehicle && player.Vehicle == player.GetData("SCHOOLVEH")) return;
                    NAPI.Entity.DeleteEntity(player.GetData("SCHOOLVEH"));
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    player.ResetData("IS_DRIVING");
                    player.ResetData("SCHOOLVEH");
                    //Main.StopT(player.GetData("SCHOOL_TIMER"), "timer_36");
                    Timers.Stop(player.GetData("SCHOOL_TIMER"));
                    player.ResetData("SCHOOL_TIMER");
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы провалили экзмен", 3000);
                }
                catch (Exception e) { Log.Write("TimerDrivingSchool: " + e.Message, nLog.Type.Error); }
            });
        }

        public static void onPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player.HasData("SCHOOLVEH")) NAPI.Entity.DeleteEntity(player.GetData("SCHOOLVEH"));
                }
                catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
            }, 0);
        }
        public static void startDrivingCourse(Client player, int index)
        {
            if (player.HasData("IS_DRIVING") || player.GetData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете сделать это сейчас", 3000);
                return;
            }
            if (Main.Players[player].Licenses[index])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть эта лицензия", 3000);
                return;
            }
            switch (index)
            {
                case 0:
                    if (Main.Players[player].Money < LicPrices[0])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    var vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Bagger, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, -1);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 0);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[0]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[0];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[0], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите B", 3000);
                    return;
                case 1:
                    if (Main.Players[player].Money < LicPrices[1])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Dilettante, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, -1);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 1);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[1]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[1];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[1], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите B", 3000);
                    return;
                case 2:
                    if (Main.Players[player].Money < LicPrices[2])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Flatbed, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, -1);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 2);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    MoneySystem.Wallet.Change(player, -LicPrices[2]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[2];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[2], $"buyLic");
                    Core.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите B", 3000);
                    return;
                case 3:
                    if (Main.Players[player].Money < LicPrices[3])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[3] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[3]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[3];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[3], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию на водный транспорт", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 4:
                    if (Main.Players[player].Money < LicPrices[4])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"", 3000);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[4] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[4]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[4];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[4], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию управление вертолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 5:
                    if (Main.Players[player].Money < LicPrices[5])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[5] = true;
                    MoneySystem.Wallet.Change(player, -LicPrices[5]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[5];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[5], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию управление самолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
            }
        }
        private void onPlayerEnterSchool(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 39);
            }
            catch (Exception e) { Log.Write("onPlayerEnterSchool: " + e.ToString(), nLog.Type.Error); }
        }
        private void onPlayerExitSchool(ColShape shape, Client player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        private void onPlayerEnterDrive(ColShape shape, Client player)
        {
            try
            {
                if (!player.IsInVehicle || player.VehicleSeat != -1) return;
                if (!player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData("ACCESS") != "SCHOOL") return;
                if (!player.HasData("IS_DRIVING")) return;
                if (player.Vehicle != player.GetData("SCHOOLVEH")) return;
                if (shape.GetData("NUMBER") != player.GetData("CHECK")) return;
                //Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                var check = player.GetData("CHECK");
                if (check == drivingCoords.Count - 1)
                {
                    player.ResetData("IS_DRIVING");
                    var vehHP = player.Vehicle.Health;
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(player.Vehicle);
                        } catch { }
                    });
                    player.ResetData("SCHOOLVEH");
                    if (vehHP < 500)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы провалили экзамен", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[player.GetData("LICENSE")] = true;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно сдали экзамен", 3000);
                    Dashboard.sendStats(player);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    return;
                }

                player.SetData("CHECK", check + 1);
                if (check + 2 < drivingCoords.Count)
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0, drivingCoords[check + 2] - new Vector3(0, 0, 1.12));
                else
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                Trigger.ClientEvent(player, "createWaypoint", drivingCoords[check + 1].X, drivingCoords[check + 1].Y);
            }
            catch (Exception e)
            {
                Log.Write("ENTERDRIVE:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        #region menu
        public static void OpenDriveSchoolMenu(Client player)
        {
            Menu menu = new Menu("driveschool", false, false);
            menu.Callback = callback_driveschool;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Лицензии";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_0", Menu.MenuItem.Button);
            menuItem.Text = $"(A) Мотоциклы - {LicPrices[0]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_1", Menu.MenuItem.Button);
            menuItem.Text = $"(B) Легковые машины - {LicPrices[1]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_2", Menu.MenuItem.Button);
            menuItem.Text = $"(C) Грузовые машины - {LicPrices[2]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_3", Menu.MenuItem.Button);
            menuItem.Text = $"(V) Водный транспорт - {LicPrices[3]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_4", Menu.MenuItem.Button);
            menuItem.Text = $"(LV) Вертолёты - {LicPrices[4]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_5", Menu.MenuItem.Button);
            menuItem.Text = $"(LS) Самолёты - {LicPrices[5]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_driveschool(Client client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(client);
            if (item.ID == "close") return;
            var id = item.ID.Split('_')[1];
            startDrivingCourse(client, Convert.ToInt32(id));
            return;
        }
        #endregion
    }
}