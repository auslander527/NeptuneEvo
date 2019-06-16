using GTANetworkAPI;
using System.Collections.Generic;
using NeptuneEvo.GUI;
using System;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.Jobs
{
    class Lawnmower : Script
    {
        static int checkpointPayment = 2;
        private static nLog Log = new nLog("Lawnmower");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStartHandler()
        {
            try
            {
                MowerWaysCols.Add(new Dictionary<int, ColShape>());
                MowerWaysCols.Add(new Dictionary<int, ColShape>());
                MowerWaysCols.Add(new Dictionary<int, ColShape>());

                for (int i = 0; i < MowerWays.Count; i++)
                {
                    for (int d = 0; d < MowerWays[i].Count; d++)
                    {
                        MowerWaysCols[i].Add(d, NAPI.ColShape.CreateCylinderColShape(MowerWays[i][d], 2.5F, 2, 0));
                        MowerWaysCols[i][d].OnEntityEnterColShape += mowerCheckpointEnterWay;
                        MowerWaysCols[i][d].SetData("WAY", i);
                        MowerWaysCols[i][d].SetData("NUMBER", d);
                    }
                }
            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void mowerCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 5);
                NAPI.Data.SetEntityData(veh, "TYPE", "MOWER");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Core.VehicleStreaming.SetEngineState(veh, false);
            }
        }

        private static List<Dictionary<int, ColShape>> MowerWaysCols = new List<Dictionary<int, ColShape>>();
        private static List<List<Vector3>> MowerWays = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(-1311.801, 47.19352, 53.15438),      // 2
                new Vector3(-1297.115, 44.82426, 51.8745),      // 3
                new Vector3(-1289.672, 41.50377, 51.41702),      // 4
                new Vector3(-1281.616, 33.57293, 50.01316),      // 5
                new Vector3(-1271.753, 25.13823, 48.37608),      // 6
                new Vector3(-1264.546, 20.33339, 48.22725),      // 7
                new Vector3(-1251.953, 16.60849, 48.29678),      // 8
                new Vector3(-1238.87, 6.834239, 47.69267),      // 9
                new Vector3(-1220.066, -0.2359556, 47.76429),      // 10
                new Vector3(-1205.803, -10.85785, 47.54021),      // 11
                new Vector3(-1191.213, -26.15599, 46.0427),      // 12
                new Vector3(-1169.42, -32.93403, 45.1136),      // 13
                new Vector3(-1151.52, -47.59819, 44.79146),      // 14
                new Vector3(-1134.367, -57.45755, 44.24867),      // 15
                new Vector3(-1113.337, -65.23543, 44.08297),      // 16
                new Vector3(-1089.529, -65.04275, 43.71874),      // 17
                new Vector3(-1068.829, -71.01239, 44.14802),      // 18
                new Vector3(-1042.585, -66.32011, 44.12957),      // 19
                new Vector3(-1028.766, -62.31656, 44.31059),      // 20
                new Vector3(-1010.945, -54.24666, 42.86016),      // 21
                new Vector3(-1004.091, -37.87244, 45.26571),      // 22
                new Vector3(-990.7962, -28.25723, 45.11967),      // 23
                new Vector3(-987.4324, -11.62305, 46.72776),      // 24
                new Vector3(-996.9695, 6.893913, 48.91844),      // 25
                new Vector3(-1007.902, 24.14594, 50.23178),      // 26
                new Vector3(-1020.931, 45.43934, 50.92136),      // 27
                new Vector3(-1029.439, 66.24312, 51.80055),      // 28
                new Vector3(-1051.099, 96.99185, 53.38039),      // 29
                new Vector3(-1058.959, 108.3395, 54.89647),      // 30
                new Vector3(-1067.165, 125.1816, 57.015),      // 31
                new Vector3(-1076.025, 142.120, 59.05041),      // 32
                new Vector3(-1090.573, 163.7347, 61.53791),      // 33
                new Vector3(-1110.753, 173.7621, 62.98595),      // 34
                new Vector3(-1125.826, 181.9235, 63.85329),      // 35
                new Vector3(-1168.176, 178.2816, 64.00827),      // 36
                new Vector3(-1199.407, 170.2452, 63.66505),      // 37
                new Vector3(-1223.774, 171.792, 62.43406),      // 38
                new Vector3(-1258.112, 168.5394, 59.59033),      // 39
                new Vector3(-1269.38, 151.660, 58.65258),      // 40
                new Vector3(-1291.592, 144.490, 58.38726),      // 41
                new Vector3(-1293.719, 115.4129, 56.68567),      // 42
                new Vector3(-1266.897, 102.3202, 56.23266),      // 43
                new Vector3(-1247.905, 92.40211, 55.77993),      // 44
                new Vector3(-1220.986, 94.42358, 57.08755),      // 45
                new Vector3(-1198.35, 100.4512, 58.12225),      // 46
                new Vector3(-1171.731, 106.132, 58.92729),      // 47
                new Vector3(-1154.295, 113.0125, 59.21473),      // 48
                new Vector3(-1134.693, 103.8594, 58.01085),      // 49
                new Vector3(-1115.926, 84.72909, 55.9537),      // 50
                new Vector3(-1105.542, 59.41261, 53.21173),      // 51
                new Vector3(-1100.384, 37.19199, 51.35212),      // 52
                new Vector3(-1094.208, 24.42362, 50.99221),      // 53
                new Vector3(-1080.965, 13.15148, 50.65717),      // 54
                new Vector3(-1092.702, 1.099258, 50.87796),      // 55
                new Vector3(-1109.086, -11.22914, 50.53533),      // 56
                new Vector3(-1120.472, -24.12328, 48.84153),      // 57
                new Vector3(-1129.953, -46.12931, 45.39178),      // 58
                new Vector3(-1141.217, -66.69366, 43.90619),      // 59
                new Vector3(-1156.251, -66.09172, 44.83364),      // 60
                new Vector3(-1164.265, -75.3207, 45.51969),      // 61
                new Vector3(-1173.412, -81.57317, 44.9781),      // 62
                new Vector3(-1185.708, -73.71046, 44.91101),      // 63
                new Vector3(-1200.024, -62.89313, 44.89631),      // 64
                new Vector3(-1222.646, -47.62072, 45.85396),      // 65
                new Vector3(-1245.577, -36.77324, 46.27745),      // 66
                new Vector3(-1261.259, -23.22089, 47.47988),      // 67
                new Vector3(-1266.621, -8.019106, 48.37564),      // 68
                new Vector3(-1275.466, -2.052856, 48.95642),      // 69
                new Vector3(-1287.256, -10.6012, 49.94118),      // 70
                new Vector3(-1298.156, -14.02906, 49.41903),      
                new Vector3(-1309.017, 4.108258, 50.99983),
                new Vector3(-1318.991, 25.81007, 53.54743),
                new Vector3(-1297.523, 51.1332, 51.67038),      // 74
                new Vector3(-1269.485, 46.31668, 49.99769),      // 75
                new Vector3(-1248.031, 37.50771, 48.87788),      // 76
                new Vector3(-1223.725, 26.63491, 47.60126),      // 77
                new Vector3(-1200.899, 8.332254, 47.56612),      // 78
                new Vector3(-1184.26, -7.549067, 47.28683),      // 79
                new Vector3(-1156.366, -4.323437, 47.77936),      // 80
                new Vector3(-1156.365, 11.39268, 49.60625),      // 81
                new Vector3(-1153.883, 31.17125, 51.20083),      // 82
                new Vector3(-1169.151, 45.62376, 52.44979),      // 83
                new Vector3(-1196.515, 61.6429, 54.02454),      // 84
                new Vector3(-1220.711, 72.92409, 53.81203),      // 85
                new Vector3(-1244.826, 73.44586, 52.69176),      // 86
                new Vector3(-1270.854, 73.22437, 53.18766),      // 87
                new Vector3(-1285.552, 74.05328, 54.72931),      // 88
                new Vector3(-1298.148, 72.72589, 54.62475),      // 89
                new Vector3(-1321.219, 69.28333, 53.55971),      // 90
            },
        };

        public static void respawnCar(Vehicle veh)
        {
            try
            {
                int i = NAPI.Data.GetEntityData(veh, "NUMBER");
                NAPI.Entity.SetEntityPosition(veh, CarInfos[i].Position);
                NAPI.Entity.SetEntityRotation(veh, CarInfos[i].Rotation);
                VehicleManager.RepairCar(veh);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 5);
                NAPI.Data.SetEntityData(veh, "TYPE", "MOWER");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("respawnCar: " + e.Message, nLog.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                try { if (!player.GetData("ON_WORK")) return; }
                catch { return; }
                if (Main.Players[player].WorkID == 5 &&
                    NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    respawnCar(vehicle);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Client player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "MOWER" &&
                Main.Players[player].WorkID == 5 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 60 секунд, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_4");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "LAWNM_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            } catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Client player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_5");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Log.Debug("Player exit work vehicle timer was stoped");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 60)
                    {
                        respawnCar(vehicle);
                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        Trigger.ClientEvent(player, "deleteCheckpoint", 4, 0);
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_6");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Customization.ApplyCharacter(player);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);
                }
                catch (Exception e) { Log.Write("Timer_PlayerExitWorkVehicle_Lawnmower: " + e.Message, nLog.Type.Error); }
            });
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Client player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "MOWER" || player.VehicleSeat != -1) return;
                
                if (Main.Players[player].WorkID == 5)
                {
                    if (!NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            Trigger.ClientEvent(player, "openDialog", "MOWER_RENT", $"Начать работу газонокосильщиком?");
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                    else
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У газонокосилки есть газонокосильщик", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        else NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете газонокосильщиком. Устроиться можно в мэрии", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                }
            } catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void mowerRent(Client player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != -1 || player.Vehicle.GetData("TYPE") != "MOWER")
            {
                var way = 0;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали работу газонокосильщика, следуйте по чекпоинтам", 3000);
                var vehicle = player.Vehicle;
                NAPI.Data.SetEntityData(player, "WORK", vehicle);
                Core.VehicleStreaming.SetEngineState(vehicle, true);
                NAPI.Data.SetEntityData(player, "ON_WORK", true);
                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                NAPI.Data.SetEntityData(vehicle, "ON_WORK", true);
                NAPI.Data.SetEntityData(player, "WORKWAY", way);
                NAPI.Data.SetEntityData(player, "WORKCHECK", 0);
                NAPI.Data.SetEntityData(vehicle, "DRIVER", player);

                var gender = Main.Players[player].Gender;
                Core.Customization.ClearClothes(player, gender);
                if (gender)
                {
                    Customization.SetHat(player, 94, 9);
                    player.SetClothes(11, 82, 4);
                    player.SetClothes(4, 27, 10);
                    player.SetClothes(6, 1, 11);
                    player.SetClothes(11, Core.Customization.CorrectTorso[gender][82], 0);
                }
                else
                {
                    Customization.SetHat(player, 93, 9);
                    player.SetClothes(11, 14, 9);
                    player.SetClothes(4, 16, 2);
                    player.SetClothes(6, 1, 3);
                    player.SetClothes(11, Core.Customization.CorrectTorso[gender][14], 0);
                }

                Trigger.ClientEvent(player, "createCheckpoint", 4, 1, MowerWays[way][0] - new Vector3(0, 0, 1.12), 2, 0, 255, 0, 0, MowerWays[way][1] - new Vector3(0, 0, 1.12));
                Trigger.ClientEvent(player, "createWaypoint", MowerWays[way][0].X, MowerWays[way][0].Y);
            }
            else
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспорте", 3000);
            }
        }

        private static void mowerCheckpointEnterWay(ColShape shape, Client player)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(player)) return;
                var vehicle = player.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "MOWER" || Main.Players[player].WorkID != 5 || !player.GetData("ON_WORK") || player.GetData("WORKWAY") != shape.GetData("WAY") || player.GetData("WORKCHECK") != shape.GetData("NUMBER"))
                    return;

                int way = player.GetData("WORKWAY");
                int check = NAPI.Data.GetEntityData(player, "WORKCHECK");
                if (MowerWays[way][check].DistanceTo(player.Position) > 3) return;

                var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                MoneySystem.Wallet.Change(player, payment);
                GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"lawnCheck");

                if (check + 1 != MowerWays[way].Count)
                {
                    var direction = (check + 2 != MowerWays[way].Count) ? MowerWays[way][check + 2] : MowerWays[way][0] - new Vector3(0, 0, 1.12);
                    Trigger.ClientEvent(player, "createCheckpoint", 4, 1, MowerWays[way][check + 1] - new Vector3(0, 0, 1.12), 2, 0, 255, 0, 0, direction);
                    Trigger.ClientEvent(player, "createWaypoint", MowerWays[way][check + 1].X, MowerWays[way][check + 1].Y);
                    NAPI.Data.SetEntityData(player, "WORKCHECK", check + 1);
                    //NAPI.Data.SetEntityData(player, "PAYMENT", NAPI.Data.GetEntityData(player, "PAYMENT") + payment);
                }
                else
                {
                    var next_way = 0;
                    Trigger.ClientEvent(player, "createCheckpoint", 4, 1, MowerWays[next_way][0] - new Vector3(0, 0, 1.12), 2, 0, 255, 0, 0, MowerWays[next_way][1] - new Vector3(0, 0, 1.12));
                    Trigger.ClientEvent(player, "createWaypoint", MowerWays[next_way][0].X, MowerWays[next_way][0].Y);
                    NAPI.Data.SetEntityData(player, "WORKCHECK", 0);
                    NAPI.Data.SetEntityData(player, "WORKWAY", next_way);
                    //NAPI.Data.SetEntityData(player, "PAYMENT", NAPI.Data.GetEntityData(player, "PAYMENT") + payment);
                }
            }
            catch (Exception ex) { Log.Write("mowerCheckpointEnterWay: " + ex.Message, nLog.Type.Error); }
        }
    }
}
