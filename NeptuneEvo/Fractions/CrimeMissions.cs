using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.Fractions
{
    class CarDelivery : Script
    {
        private static nLog Log = new nLog("Crime:CarDelivery");

        private static List<VehicleHash> GangsVehiclesHashes = new List<VehicleHash>()
        {
            { VehicleHash.Emperor },
            { VehicleHash.Ingot },
            { VehicleHash.Asterope },
            { VehicleHash.Blista2 },
            { VehicleHash.Seminole },
            { VehicleHash.Fugitive },
            { VehicleHash.Tailgater },
            { VehicleHash.Cog55 },
            { VehicleHash.Serrano },
            { VehicleHash.Dominator },
            { VehicleHash.Baller2 },
            { VehicleHash.Cavalcade2 },
            { VehicleHash.Dubsta2 },
            { VehicleHash.Schafter2 },
            { VehicleHash.Buffalo2 },
        };
        private static List<Color> GangsVehiclesColors = new List<Color>()
        {
            { new Color(0, 0, 0) },
            { new Color(0, 0, 0) },
            { new Color(250, 250, 250) },
            { new Color(200, 0, 0) },
            { new Color(250, 250, 250) },
            { new Color(250, 250, 250) },
            { new Color(0, 0, 200) },
            { new Color(80, 80, 80) },
            { new Color(0, 0, 0) },
            { new Color(230, 90, 0) },
            { new Color(250, 250, 250) },
            { new Color(0, 0, 250) },
            { new Color(80, 80, 80) },
            { new Color(250, 250, 250) },
            { new Color(0, 200, 0) },
        };

        private static List<VehicleHash> MafiaVehiclesHashes = new List<VehicleHash>()
        {
            { VehicleHash.Pounder },
            { VehicleHash.Boxville3 },
            { VehicleHash.Mule },
        };
        private static List<Color> MafiaVehiclesColors = new List<Color>()
        {
            { new Color(0, 0, 0) },
            { new Color(0, 0, 0) },
            { new Color(0, 0, 0) },
        };

        private static Dictionary<int, DateTime> NextDelivery = new Dictionary<int, DateTime>()
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
            { 10, DateTime.Now },
            { 11, DateTime.Now },
            { 12, DateTime.Now },
            { 13, DateTime.Now },
        };

        private static Vector3 GangStartDelivery = new Vector3(480.9385, -1302.576, 28.12353);
        private static List<Vector3> GangSpawnAutos = new List<Vector3>()
        {
            new Vector3(814.4807, -747.8201, 26.8163),
            new Vector3(711.6686, -895.5933, 23.6463),
            new Vector3(731.1492, -1291.567, 26.3728),
            new Vector3(-451.0398, -1691.584, 19.04522),
            new Vector3(-559.7632, -1802.013, 22.71108),
            new Vector3(-702.673, -1138.601, 10.7008),
            new Vector3(331.2978, -996.9423, 29.30805),
            new Vector3(295.5413, -1203.21, 29.24488),
            new Vector3(198.8274, -1244.367, 29.34067),
            new Vector3(-478.4023, -757.7425, 35.46944),
            new Vector3(-313.0926, -771.1686, 34.05299),
            new Vector3(-151.8391, -1030.047, 27.3617),
            new Vector3(1000.476, -1532.942, 29.95521),
            new Vector3(969.8792, -1647.465, 29.93223),
            new Vector3(987.3099, -1832.049, 31.16065),
            new Vector3(1265.665, -2563.195, 42.91005),
        };
        private static List<Vector3> GangSpawnAutosRot = new List<Vector3>()
        {
            new Vector3(-0.1028762, -6.507742E-05, 185.9321),
            new Vector3(-0.03887018, 4.864606, 267.9159),
            new Vector3(0.001559988, 0.06757163, 88.61029),
            new Vector3(-0.4639261, 0.1743369, 164.3215),
            new Vector3(0.1674757, -0.3905835, 331.478),
            new Vector3(0.03815498, 0.0097493, 43.03497),
            new Vector3(-2.474918, 0.1015807, 181.4881),
            new Vector3(0.6146746, 1.624031, 177.8676),
            new Vector3(0.197663, 0.6434172, 85.07928),
            new Vector3(0.02163176, 0.1697985, 88.46164),
            new Vector3(-0.1381722, 0.07952184, 160.3685),
            new Vector3(-0.01273972, 0.0547189, 165.6741),
            new Vector3(0.002720123, -6.338181, 86.91882),
            new Vector3(-5.582367, -0.02341981, 179.5117),
            new Vector3(-0.353688, -1.634855, 354.2626),
            new Vector3(1.573582, 3.525663, 284.0478),
        };
        private static Dictionary<int, Vector3> MafiaStartDelivery = new Dictionary<int, Vector3>()
        {
            { 10, new Vector3(1463.797, 1128.923, 114.3969) },
            { 11, new Vector3(-128.5574, 994.9902, 235.8243) },
            { 12, new Vector3(-1538.677, -76.76743, 54.22959) },
            { 13, new Vector3(-1795.539, 399.2474, 112.8691) },
        };
        private static Dictionary<int, Vector3> MafiaStartDeliveryRot = new Dictionary<int, Vector3>()
        {
            { 10, new Vector3(1.19556, 0.2941337, 91.12183) },
            { 11, new Vector3(-0.02024388, 0.4382547, 198.8489) },
            { 12, new Vector3(-0.4612706, 0.8252076, 180.706) },
            { 13, new Vector3(-0.3686997, -0.2600957, 286.0435) },
        };

        private static List<Vector3> GangEndDelivery = new List<Vector3>()
        {
            new Vector3(-2165.107, 4285.171, 49.03421),
            new Vector3(142.6606, 6652.569, 31.59082),
            new Vector3(1427.403, 6327.671, 24.14281),
            new Vector3(1421.056, 6355.05, 24.06137),
            new Vector3(2547.709, 4645.736, 34.15304),
            new Vector3(2529.517, 4109.557, 38.75628),
            new Vector3(1404.127, 3656.653, 34.19704),
            new Vector3(1709.216, 3313.655, 41.2984),
            new Vector3(1041.522, 2650.708, 39.62605),
            new Vector3(260.4555, 2580.766, 45.11154),
        };
        private static List<Vector3> MafiaEndDelivery = new List<Vector3>()
        {
            new Vector3(-1834.286, 4692.507, 0.6031599),
            new Vector3(-2658.842, 2533.021, 1.898514),
            new Vector3(3304.526, 5401.26, 13.62714),
            new Vector3(2834.238, -724.9515, 0.6343195),
            new Vector3(1413.24, 3647.831, 33.24832),
            new Vector3(1698.02, 4873.916, 40.91091),
            new Vector3(828.0319, -3206.071, 4.78082),
            new Vector3(-337.131, -2462.448, 4.880634),
            new Vector3(1642.278, 4837.864, 40.90576),
            new Vector3(616.3151, 2799.022, 40.80243),
            new Vector3(-1151.819, -1565.433, 3.273624),
            new Vector3(1378.693, -2077.05, 50.87856),
        };
        private static Vector3 PoliceEndDelivery = new Vector3(479.2215, -1024.153, 26.91038);

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var colShape = NAPI.ColShape.CreateCylinderColShape(GangStartDelivery, 2, 3, NAPI.GlobalDimension);
                colShape.OnEntityEnterColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 52);
                    }
                    catch (Exception ex) { Log.Write("start_colShape.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShape.OnEntityExitColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Log.Write("start_colShape.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                };

                NAPI.TextLabel.CreateTextLabel("~g~Jimmy Lishman", GangStartDelivery + new Vector3(0, 0, 2.5), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                #region MafiaStartDelivery
                var i = 0;
                foreach (var pos in MafiaStartDelivery)
                {
                    colShape = NAPI.ColShape.CreateCylinderColShape(pos.Value, 3, 3, NAPI.GlobalDimension);
                    colShape.SetData("ID", pos.Key);
                    colShape.OnEntityEnterColShape += (s, e) => {
                        try
                        {
                            e.SetData("ONMAFIAID", s.GetData("ID")); e.SetData("INTERACTIONCHECK", 53);
                        }
                        catch (Exception ex) { Log.Write("Delivery_colShape.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    colShape.OnEntityExitColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("Delivery_colShape.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                    };

                    NAPI.Marker.CreateMarker(1, pos.Value - new Vector3(0, 0, 2.7), new Vector3(), new Vector3(), 3, new Color(255, 0, 0, 220), false, NAPI.GlobalDimension);
                    NAPI.TextLabel.CreateTextLabel("~g~Get delivery mission", pos.Value, 5f, 0.4f, 0, new Color(255, 255, 255));
                }
                #endregion

                #region PoliceDropDelivery
                colShape = NAPI.ColShape.CreateCylinderColShape(PoliceEndDelivery, 3, 4, NAPI.GlobalDimension);
                colShape.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!Main.Players.ContainsKey(e) || (Main.Players[e].FractionID != 7 && Main.Players[e].FractionID != 9) || !e.IsInVehicle || !e.Vehicle.HasData("ACCESS")
                            || (e.Vehicle.GetData("ACCESS") != "GANGDELIVERY" && e.Vehicle.GetData("ACCESS") != "MAFIADELIVERY")) return;

                        NAPI.Data.ResetEntityData(e.Vehicle.GetData("WHOS_VEH"), "DELIVERY_CAR");
                        e.Vehicle.Delete();
                        MoneySystem.Wallet.Change(e, 250);
                        GameLog.Money($"server", $"player({Main.Players[e].UUID})", 250, $"arrestCar");
                        Notify.Send(e, NotifyType.Success, NotifyPosition.BottomCenter, "Вы арестовали машину", 3000);
                    }
                    catch (Exception ex) { Log.Write("OnEntityEnterDropDelivery: " + ex.Message); }
                };
                NAPI.Marker.CreateMarker(1, PoliceEndDelivery - new Vector3(0, 0, 2.5), new Vector3(), new Vector3(), 3, new Color(255, 0, 0, 220), false, NAPI.GlobalDimension);
                #endregion

                #region GangsDropDelivery
                i = 0;
                foreach (var pos in GangEndDelivery)
                {
                    colShape = NAPI.ColShape.CreateCylinderColShape(pos, 5, 3, NAPI.GlobalDimension);
                    colShape.SetData("ID", i);
                    colShape.OnEntityEnterColShape += (s, e) =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(e) || !e.IsInVehicle || !e.Vehicle.HasData("ACCESS")
                                || e.Vehicle.GetData("ACCESS") != "GANGDELIVERY" || s.GetData("ID") != e.Vehicle.GetData("ENDPOINT")) return;

                            if (DateTime.Now < e.Vehicle.GetData("ENDDATA"))
                            {
                                Notify.Send(e, NotifyType.Success, NotifyPosition.BottomCenter, "Попробуйте чуть позже", 3000);
                                return;
                            }

                            NAPI.Data.ResetEntityData(e.Vehicle.GetData("WHOS_VEH"), "DELIVERY_CAR");
                            e.Vehicle.Delete();
                            Stocks.fracStocks[Main.Players[e].FractionID].Money += 500;
                            GameLog.Money($"server", $"frac({Main.Players[e].FractionID})", 500, "dropCar");
                            e.SendChatMessage("Сдача машины: !{#00FF00}500$ ~w~были отправлены в общак банды. (" + $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}" + ")");
                        }
                        catch (Exception ex) { Log.Write("GangDropDelivery: " + ex.Message); }
                    };

                    NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 3.5), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 220), false, NAPI.GlobalDimension);
                    i++;
                }
                #endregion

                #region MafiaDropDelivery
                i = 0;
                foreach (var pos in MafiaEndDelivery)
                {
                    colShape = NAPI.ColShape.CreateCylinderColShape(pos, 5, 3, NAPI.GlobalDimension);
                    colShape.SetData("ID", i);
                    colShape.OnEntityEnterColShape += (s, e) =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(e) || !e.IsInVehicle || !e.Vehicle.HasData("ACCESS")
                                || e.Vehicle.GetData("ACCESS") != "MAFIADELIVERY" || s.GetData("ID") != e.Vehicle.GetData("ENDPOINT")) return;

                            if (DateTime.Now < e.Vehicle.GetData("ENDDATA"))
                            {
                                Notify.Send(e, NotifyType.Success, NotifyPosition.BottomCenter, "Попробуйте чуть позже", 3000);
                                return;
                            }

                            NAPI.Data.ResetEntityData(e.Vehicle.GetData("WHOS_VEH"), "DELIVERY_CAR");
                            e.Vehicle.Delete();
                            Stocks.fracStocks[Main.Players[e].FractionID].Money += 500;
                            GameLog.Money($"server", $"frac({Main.Players[e].FractionID})", 500, "dropCar");
                            e.SendChatMessage("Сдача фургона: !{#00FF00}500$ ~w~были отправлены в общак мафии. (" + $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()})");
                        }
                        catch (Exception ex) { Log.Write("MafiaDropDelivery: " + ex.Message); }
                    };

                    NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 3.5), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 220), false, NAPI.GlobalDimension);
                    i++;
                }
                #endregion
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void Event_PlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seat)
        {
            try
            {
                if (!vehicle.HasData("ACCESS") || !Main.Players.ContainsKey(player)) return;
                var fraction = Main.Players[player].FractionID;
                switch ((string)vehicle.GetData("ACCESS"))
                {
                    case "GANGDELIVERY":
                        if (fraction == 7 || fraction == 9)
                        {
                            Trigger.ClientEvent(player, "createWaypoint", PoliceEndDelivery.X, PoliceEndDelivery.Y);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Отвезите машину в полицейский участок", 3000);
                        }
                        else if (fraction != vehicle.GetData("GANG"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет доступа", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        else
                        {
                            var end = (int)vehicle.GetData("ENDPOINT");
                            Trigger.ClientEvent(player, "createWaypoint", GangEndDelivery[end].X, GangEndDelivery[end].Y);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Доставьте машину в точку, отмеченную на карте", 3000);
                        }
                        return;
                    case "MAFIADELIVERY":
                        if (fraction == 7 || fraction == 9)
                        {
                            Trigger.ClientEvent(player, "createWaypoint", PoliceEndDelivery.X, PoliceEndDelivery.Y);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Отвезите машину в полицейский участок", 3000);
                        }
                        else if (fraction != vehicle.GetData("MAFIA"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет доступа", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        else
                        {
                            var end = (int)vehicle.GetData("ENDPOINT");
                            Trigger.ClientEvent(player, "createWaypoint", MafiaEndDelivery[end].X, MafiaEndDelivery[end].Y);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Доставьте машину в точку, отмеченную на карте", 3000);
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("EnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        
        public static void Event_PlayerDisconnected(Client player)
        {
            if (player.HasData("DELIVERY_CAR"))
            {
                Vehicle veh = player.GetData("DELIVERY_CAR");
                NAPI.Task.Run(() => { try { veh.Delete(); } catch { } });
            }
        }

        public static void Event_InteractPressed(Client player, int id)
        {
            if (!Main.Players.ContainsKey(player) || player.IsInVehicle) return;
            var fraction = Main.Players[player].FractionID;

            switch (id)
            {
                case 52:
                    if (Manager.FractionTypes[fraction] != 1) return;
                    Trigger.ClientEvent(player, "gangmis");
                    return;
                case 53:
                    if (fraction != player.GetData("ONMAFIAID")) return;
                    Trigger.ClientEvent(player, "mafiamis");
                    return;
            }
        }

        private static int LastGangSpawn = 0;
        [RemoteEvent("gangmis")]
        public void Event_gangMission(Client player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || player.IsInVehicle) return;
                if (player.Position.DistanceTo(GangStartDelivery) > 5) return;

                var fraction = Main.Players[player].FractionID;
                switch (id)
                {
                    case 0:
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недоступно на данный момент", 3000);
                        return;
                    case 1:
                        if (DateTime.Now < NextDelivery[fraction])
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Попробуйте позже", 3000);
                            return;
                        }

                        if (player.HasData("DELIVERY_CAR"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны сначала доставить предыдущую машину", 3000);
                            return;
                        }

                        Random rnd = new Random();
                        var vehicleNum = rnd.Next(0, GangsVehiclesHashes.Count);
                        var vehicle = NAPI.Vehicle.CreateVehicle(GangsVehiclesHashes[vehicleNum], GangSpawnAutos[LastGangSpawn], GangSpawnAutosRot[LastGangSpawn], 0, 0);
                        Trigger.ClientEvent(player, "createWaypoint", GangSpawnAutos[LastGangSpawn].X, GangSpawnAutos[LastGangSpawn].Y);
                        LastGangSpawn = (LastGangSpawn + 1 == GangSpawnAutos.Count) ? 0 : LastGangSpawn + 1;
                        vehicle.CustomPrimaryColor = GangsVehiclesColors[vehicleNum];
                        vehicle.CustomSecondaryColor = GangsVehiclesColors[vehicleNum];
                        NAPI.Vehicle.SetVehicleNumberPlate(vehicle, "xxxxx");

                        var end = rnd.Next(0, GangEndDelivery.Count);
                        vehicle.SetData("ACCESS", "GANGDELIVERY");
                        vehicle.SetData("ENDPOINT", end);
                        vehicle.SetData("GANG", fraction);
                        vehicle.SetData("ENDDATA", DateTime.Now.AddSeconds(vehicle.Position.DistanceTo(GangEndDelivery[end]) / 100 * 2));
                        vehicle.SetData("WHOS_VEH", player);
                        VehicleStreaming.SetEngineState(vehicle, true);
                        player.SetData("DELIVERY_CAR", vehicle);

                        NextDelivery[fraction] = DateTime.Now.AddMinutes(5);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы получили координаты машины. Сядьте в неё и отвезите в место, которое отмечено в GPS автомобиля.", 3000);
                        return;
                }
            }
            catch (Exception e) { Log.Write("GangMission: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("mafiamis")]
        public void Event_mafiaMission(Client player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || player.IsInVehicle) return;
                int i = player.GetData("ONMAFIAID");
                if (player.Position.DistanceTo(MafiaStartDelivery[i]) > 6) return;

                var fraction = Main.Players[player].FractionID;

                if (DateTime.Now < NextDelivery[fraction])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Попробуйте позже", 3000);
                    return;
                }

                if (player.HasData("DELIVERY_CAR"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны сначала доставить предыдущую машину", 3000);
                    return;
                }

                Random rnd = new Random();
                var vehicleHash = MafiaVehiclesHashes[0];
                var text = "";

                switch (id)
                {
                    case 0:
                        text = "оружием";
                        break;
                    case 1:
                        text = "деньгами";
                        vehicleHash = MafiaVehiclesHashes[1];
                        break;
                    case 2:
                        text = "трупами";
                        vehicleHash = MafiaVehiclesHashes[2];
                        break;
                }

                
                var vehicle = NAPI.Vehicle.CreateVehicle(vehicleHash, MafiaStartDelivery[i] + new Vector3(0, 0, 1), MafiaStartDeliveryRot[i], 0, 0);
                vehicle.CustomPrimaryColor = new Color(0, 0, 0);
                vehicle.CustomSecondaryColor = new Color(0, 0, 0);
                NAPI.Vehicle.SetVehicleNumberPlate(vehicle, "xxxxx");

                var end = rnd.Next(0, MafiaEndDelivery.Count);
                vehicle.SetData("ACCESS", "MAFIADELIVERY");
                vehicle.SetData("ENDPOINT", end);
                vehicle.SetData("MAFIA", fraction);
                vehicle.SetData("ENDDATA", DateTime.Now.AddSeconds(vehicle.Position.DistanceTo(MafiaEndDelivery[end]) / 100 * 2));
                vehicle.SetData("WHOS_VEH", player);
                VehicleStreaming.SetEngineState(vehicle, true);
                player.SetData("DELIVERY_CAR", vehicle);

                NextDelivery[fraction] = DateTime.Now.AddMinutes(5);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили фургон с {text} для транспортировки. Отвезите его в указанное на карте место", 3000);
                Trigger.ClientEvent(player, "createWaypoint", MafiaEndDelivery[end].X, MafiaEndDelivery[end].Y);
                player.SetIntoVehicle(vehicle, -1);
                return;
            }
            catch (Exception e) { Log.Write("GangMission: " + e.Message, nLog.Type.Error); }
        }
    }
}
