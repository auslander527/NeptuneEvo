using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEvo.GUI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.Houses;

namespace NeptuneEvo.Jobs
{
    class Gopostal : Script
    {
        private static nLog Log = new nLog("GoPostal");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Coords[0], 1, 2, 0)); // start work
                Cols[0].OnEntityEnterColShape += gp_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += gp_onEntityExitColShape;
                Cols[0].SetData("INTERACT", 28);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("Postal stock"), Coords[0] + new Vector3(0, 0, 0.3), 10F, 0.6F, 0, new Color(0, 180, 0));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(Coords[1], 1, 2, 0)); // get car
                Cols[1].OnEntityEnterColShape += gp_onEntityEnterColShape;
                Cols[1].OnEntityExitColShape += gp_onEntityExitColShape;
                Cols[1].SetData("INTERACT", 29);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("Take work car"), Coords[1] + new Vector3(0, 0, 0.3), 10F, 0.6F, 0, new Color(0, 180, 0));

            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static int checkpointPayment = 3;

        public static List<Vector3> Coords = new List<Vector3>()
        {
            new Vector3(105.4633, -1568.843, 28.60269), // start work
            new Vector3(106.2007, -1563.748, 28.60272), // get car
        };
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        private static Dictionary<int, ColShape> gCols = new Dictionary<int, ColShape>();
        // Postal items (objects) //
        public static List<uint> GoPostalObjects = new List<uint>
        {
            NAPI.Util.GetHashKey("prop_drug_package_02"),
        };

        public static void onPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != 2) return;
                if (player.GetData("WORK") != null) NAPI.Entity.DeleteEntity(player.GetData("WORK"));
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != 2) return;
                if (NAPI.Data.GetEntityData(player, "ON_WORK") && NAPI.Data.GetEntityData(player, "PACKAGES") != 0)
                {
                    int x = WorkManager.rnd.Next(0, GoPostalObjects.Count);
                    BasicSync.AttachObjectToPlayer(player, GoPostalObjects[x], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                }
            } catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_PlayerDeath(Client player, Client entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 2 && NAPI.Data.GetEntityData(player, "ON_WORK"))
                {
                    NAPI.Data.SetEntityData(player, "ON_WORK", false);
                    Customization.ApplyCharacter(player);
                }
            } catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void GoPostal_onEntityEnterColShape(ColShape shape, Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (HouseManager.Houses.Count == 0) return;
                if (Main.Players[player].WorkID != 2 || !NAPI.Data.GetEntityData(player, "ON_WORK")) return;
                if (player.HasData("NEXTHOUSE") && player.HasData("HOUSEID") && NAPI.Data.GetEntityData(player, "NEXTHOUSE") == player.GetData("HOUSEID"))
                {
                    if (NAPI.Player.IsPlayerInAnyVehicle(player))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Покиньте транспортное средство", 3000);
                        return;
                    }
                    if (player.GetData("PACKAGES") == 0) return;
                    else if (player.GetData("PACKAGES") > 1)
                    {
                        player.SetData("PACKAGES", player.GetData("PACKAGES") - 1);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"У Вас осталось {player.GetData("PACKAGES")} посылок", 3000);

                        var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData("W_LASTPOS")) / 100);
                        var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                        DateTime lastTime = player.GetData("W_LASTTIME");
                        if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Хозяина нет дома, попробуйте позже", 3000);
                            return;
                        }

                        //player.SetData("PAYMENT", player.GetData("PAYMENT") + payment);
                        MoneySystem.Wallet.Change(player, payment);
                        GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"postalCheck");

                        BasicSync.DetachObject(player);

                        var nextHouse = player.GetData("NEXTHOUSE");
                        var next = -1;
                        do
                        {
                            next = WorkManager.rnd.Next(0, HouseManager.Houses.Count - 1);
                        }
                        while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(player.Position) < 200);
                        player.SetData("W_LASTPOS", player.Position);
                        player.SetData("W_LASTTIME", DateTime.Now);
                        player.SetData("NEXTHOUSE", HouseManager.Houses[next].ID);
                        
                        Trigger.ClientEvent(player, "createCheckpoint", 1, 1, HouseManager.Houses[next].Position, 1, 0, 255, 0, 0);
                        Trigger.ClientEvent(player, "createWaypoint", HouseManager.Houses[next].Position.X, HouseManager.Houses[next].Position.Y);
                        Trigger.ClientEvent(player, "createWorkBlip", HouseManager.Houses[next].Position);
                        NAPI.Player.PlayPlayerAnimation(player, -1, "anim@heists@narcotics@trash", "drop_side");
                    }
                    else
                    {
                        var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData("W_LASTPOS")) / 100);
                        var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl]);

                        DateTime lastTime = player.GetData("W_LASTTIME");
                        if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Хозяина нет дома, попробуйте позже", 3000);
                            return;
                        }

                        //player.SetData("PAYMENT", player.GetData("PAYMENT") + payment);
                        MoneySystem.Wallet.Change(player, payment);
                        GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"postalCheck");

                        Trigger.ClientEvent(player, "deleteWorkBlip");
                        Trigger.ClientEvent(player, "createWaypoint", 105.4633f, -1568.843f);

                        BasicSync.DetachObject(player);

                        Trigger.ClientEvent(player, "deleteCheckpoint", 1, 0);
                        NAPI.Player.PlayPlayerAnimation(player, -1, "anim@heists@narcotics@trash", "drop_side");
                        player.SetData("PACKAGES", 0);
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"У Вас не осталось посылок, возьмите новые", 3000);
                    }
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"GoPostal\":\n" + e.ToString(), nLog.Type.Error); }
        }
        private void gp_onEntityEnterColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData("INTERACT"));
            }
            catch (Exception ex) { Log.Write("gp_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }
        private void gp_onEntityExitColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("gp_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                BasicSync.DetachObject(player);
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void getGoPostalCar(Client player)
        {
            if (Main.Players[player].WorkID != 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете курьером", 3000);
                return;
            }
            if (!player.GetData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player.GetData("WORK") != null)
            {
                NAPI.Entity.DeleteEntity(player.GetData("WORK"));
                player.SetData("WORK", null);
                return;
            }
            var veh = API.Shared.CreateVehicle(VehicleHash.Faggio, player.Position + new Vector3(0, 0, 1), player.Rotation.Z, 10, 10);
            player.SetData("WORK", veh);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили рабочий транспорт", 3000);
            veh.SetData("ACCESS", "WORK");
            Core.VehicleStreaming.SetEngineState(veh, true);
        }
    }
}
