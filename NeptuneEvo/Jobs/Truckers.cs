using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using NeptuneEvo.Core;
using Redage.SDK;
using Newtonsoft.Json;
using NeptuneEvo.GUI;

namespace NeptuneEvo.Jobs
{
    class Truckers : Script
    {
        private static nLog Log = new nLog("Truckers");

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                for (int i = 0; i < getProduct.Count; i++)
                {
                    cols.Add(NAPI.ColShape.CreateCylinderColShape(getProduct[i], 1F, 2F, 0));
                    cols[i].OnEntityEnterColShape += onEntityEnterGetProduct;
                    cols[i].OnEntityExitColShape += onEntityExitGetProduct;
                    cols[i].SetData("PROD", i);
                    NAPI.Marker.CreateMarker(1, getProduct[i] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to take products"), new Vector3(getProduct[i].X, getProduct[i].Y, getProduct[i].Z + 0.7), 30f, 0.5f, 0, new Color(255, 255, 255));
                }
            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }   
        }

        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void truckerCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                Core.VehicleStreaming.SetEngineState(veh, false);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 6);
                NAPI.Data.SetEntityData(veh, "TYPE", "TRUCKER");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
        }

        private static List<ColShape> cols = new List<ColShape>();
        public static List<Vector3> getProduct = new List<Vector3>()
        {
            new Vector3(95.82169, 6363.628, 30.37586), // 24/7 products && burger-shot && clothesshop && tattoo-salon && barber-shop && masks shop && ls customs
            new Vector3(2786.021, 1575.39, 23.50065), // petrol products
            new Vector3(148.6672, 6362.376, 30.52923), // autos
            new Vector3(2710.076, 3454.989, 55.31736), // gun products
        };
        private static List<List<Vector3>> SpawnTrailers = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(69.55959, 6347.407, 31.31177),
                new Vector3(63.11602, 6344.027, 31.31204),
                new Vector3(80.00861, 6325.999, 31.23615),
            }, // 24/7 products && burger-shot && clothesshop && tattoo-salon && barber-shop && masks shop
            new List<Vector3>()
            {
                new Vector3(2788.678, 1616.642, 24.58699),
                new Vector3(2784.913, 1616.672, 24.58648),
                new Vector3(2768.964, 1601.715, 24.58705),
            }, // petrol products
            new List<Vector3>()
            {
                new Vector3(143.4239, 6355.267, 31.46451),
                new Vector3(138.6919, 6352.848, 31.4094),
                new Vector3(133.8103, 6350.519, 31.34178),
            }, // autos
            new List<Vector3>()
            {
                new Vector3(2718.307, 3427.96, 56.25636),
                new Vector3(2696.434, 3436.59, 55.8614),
                new Vector3(2657.382, 3452.611, 55.75822),
            }, // guns
        };
        private static List<List<Vector3>> SpawnTrailersRot = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(0.04818683, 0.02195024, 26.42471),
                new Vector3(0.03737624, 0.01959893, 26.66589),
                new Vector3(0.01805799, 0.1463955, 26.55667),
            }, // 24/7 products && burger-shot && clothesshop && tattoo-salon && barber-shop
            new List<Vector3>()
            {
                new Vector3(-0.04697039, -0.003354567, 179.4011),
                new Vector3(-0.06740852, 0.0153397, 179.3384),
                new Vector3(0.0002473542, -0.05684812, 269.6186),
            }, // petrol products
            new List<Vector3>()
            {
                new Vector3(0.01672701, 0.06130609, 29.15912),
                new Vector3(-0.2675416, 0.6732094, 28.63733),
                new Vector3(-0.2370726, 0.6611202, 24.13281),
            }, // autos
            new List<Vector3>()
            {
                new Vector3(-0.2371912, 2.819255, 248.2221),
                new Vector3(-2.565463, 0.7743745, 248.5302),
                new Vector3(-1.767069, 1.140572, 245.2113),
            }, // guns
        };
        private static List<int> LastTrailerSpawn = new List<int>()
        {
            0, 0, 0, 0
        };

        public static void cancelOrder(Client player)
        {
            try
            {
                if (player.HasData("ORDER"))
                {
                    int uid = player.GetData("ORDER");
                    Business biz = BusinessManager.BizList[BusinessManager.Orders[uid]];
                    var order = biz.Orders.FirstOrDefault(o => o.UID == uid);
                    if (order != null) order.Taked = false;
                    player.ResetData("ORDER");
                }

                if (player.HasData("TRAILER"))
                {
                    var trailer = player.GetData("TRAILER");
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(trailer);
                        }
                        catch { }
                    });
                    player.ResetData("TRAILER");
                }
                
                player.ResetData("ORDERDATE");
            }
            catch (Exception e) { Log.Write("CancelOrder: " + e.Message, nLog.Type.Error); }
        }
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
                NAPI.Data.SetEntityData(veh, "WORK", 6);
                NAPI.Data.SetEntityData(veh, "TYPE", "TRUCKER");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("respawnCar: " + e.Message, nLog.Type.Error); }
        }
        public static void truckerRent(Client player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) && player.VehicleSeat == -1 && player.Vehicle.GetData("TYPE") == "TRUCKER")
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали работу дальнобойщиком. Открыть заказы /orders", 3000);
                var vehicle = player.Vehicle;
                NAPI.Data.SetEntityData(player, "WORK", vehicle);
                player.ResetData("WayPointBiz");
                Core.VehicleStreaming.SetEngineState(vehicle, true);
                NAPI.Data.SetEntityData(player, "ON_WORK", true);
                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                NAPI.Data.SetEntityData(vehicle, "ON_WORK", true);
                NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
            }
            else
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспорте", 3000);
            }
        }
        public static void getOrderTrailer(Client player)
        {
            if (Main.Players[player].WorkID != 6)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете дальнобойщиком", 3000);
                return;
            }
            if (player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны выйти из машины", 3000);
                return;
            }
            if (!player.HasData("ORDER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет заказа", 3000);
                return;
            }
            int prod = player.GetData("PROD");
            if (player.HasData("TRAILER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже получили продукты", 3000);
                return;
            }
            int uid = player.GetData("ORDER");
            if (!BusinessManager.Orders.ContainsKey(uid))
            {
                cancelOrder(player);
                return;
            }
            Business biz = BusinessManager.BizList[BusinessManager.Orders[uid]];
            var order = biz.Orders.FirstOrDefault(o => o.UID == uid);
            if (order == null)
            {
                cancelOrder(player);
                return;
            }
            if (!player.HasData("ORDERDATE"))
            {
                cancelOrder(player);
                return;
            }
            DateTime date = player.GetData("ORDERDATE");
            if (DateTime.Now < date)
            {
                DateTime g = new DateTime((date - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Товар всё ещё подготавливается. Ждите {min}:{sec}", 3000);
                return;
            }

            var spawnI = LastTrailerSpawn[prod];
            if (LastTrailerSpawn[prod] == 2) LastTrailerSpawn[prod] = 0;
            else LastTrailerSpawn[prod]++;

            switch (prod)
            {
                case 0:
                    if (biz.Type != 0 && biz.Type != 8 && biz.Type != 7 && biz.Type != 9 && biz.Type != 10 && biz.Type != 11 && biz.Type != 12 && biz.Type != 13 && biz.Type != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не брали заказ на этот тип товара", 3000);
                        return;
                    }
                    var veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Trailers2, SpawnTrailers[prod][spawnI], SpawnTrailersRot[prod][spawnI].Z, 0, 0);
                    player.SetData("TRAILER", veh);
                    Trigger.ClientEvent(player, "SetOrderTruck", veh);
                    player.SendChatMessage("Если вдруг Вы потеряете свой трейлер, то напишите в чат /findtrailer");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили трейлер с товаром, подойдите к нему и увидите 'Ваш Заказ'", 3000);
                    return;
                case 1:
                    if (biz.Type != 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не брали заказ на бензин", 3000);
                        return;
                    }
                    veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Tanker, SpawnTrailers[prod][spawnI], SpawnTrailersRot[prod][spawnI].Z, 0, 0);
                    player.SetData("TRAILER", veh);
                    Trigger.ClientEvent(player, "SetOrderTruck", veh);
                    player.SendChatMessage("Если вдруг Вы потеряете свой трейлер, то напишите в чат /findtrailer");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили трейлер с товаром, подойдите к нему и увидите 'Ваш Заказ'", 3000);
                    return;
                case 2:
                    if (biz.Type < 2 || biz.Type > 5)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не брали заказ на автомобили", 3000);
                        return;
                    }
                    veh = NAPI.Vehicle.CreateVehicle(VehicleHash.TR4, SpawnTrailers[prod][spawnI], SpawnTrailersRot[prod][spawnI].Z, 0, 0);
                    player.SetData("TRAILER", veh);
                    Trigger.ClientEvent(player, "SetOrderTruck", veh);
                    player.SendChatMessage("Если вдруг Вы потеряете свой трейлер, то напишите в чат /findtrailer");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили трейлер с товаром, подойдите к нему и увидите 'Ваш Заказ'", 3000);
                    return;
                case 3:
                    if (biz.Type != 6)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не брали заказ на оружие", 3000);
                        return;
                    }
                    veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Trailers, SpawnTrailers[prod][spawnI], SpawnTrailersRot[prod][spawnI].Z, 0, 0);
                    player.SetData("TRAILER", veh);
                    Trigger.ClientEvent(player, "SetOrderTruck", veh);
                    player.SendChatMessage("Если вдруг Вы потеряете свой трейлер, то напишите в чат /findtrailer");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили трейлер с товаром, подойдите к нему и увидите 'Ваш Заказ'", 3000);
                    return;
            }
        }
        public static void onPlayerDissconnectedHandler(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 6 &&
                    NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    respawnCar(vehicle);
                    cancelOrder(player);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        private void onEntityEnterGetProduct(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 31);
                NAPI.Data.SetEntityData(entity, "PROD", shape.GetData("PROD"));
            }
            catch (Exception ex) { Log.Write("onEntityEnterGetProduct: " + ex.Message, nLog.Type.Error); }
        }
        private void onEntityExitGetProduct(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("onEntityExitGetProduct: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Client player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TRUCKER") return;
                if (player.VehicleSeat == -1)
                {
                    if (!Main.Players[player].Licenses[2])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].WorkID == 6)
                    {
                        if (Main.Players[player].BizIDs.Count > 0)
                        {
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы не можете начать работу", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        }
                        if (!NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                        {
                            if (NAPI.Data.GetEntityData(player, "WORK") == null)
                            {
                                Trigger.ClientEvent(player, "openDialog", "TRUCKER_RENT", $"Вы действительно хотите начать работу?");
                            }
                            else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                            {
                                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                            }

                        }
                        else
                        {
                            if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине есть водитель", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                            else
                            {
                                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                                if (player.HasData("ORDER") && player.HasData("TRAILER") && !player.HasData("GOTPRODUCT"))
                                {
                                    playerGotProducts(player);
                                }
                            }
                        }
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете дальнобойщиком. Устроиться можно в мэрии", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
            } catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }   
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Client player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "TRUCKER" &&
                Main.Players[player].WorkID == 6 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 5 минут, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_truckers_1");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "TRUCK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.Start(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
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
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_truckers_2");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        if(player.HasData("WayPointBiz")) {
                            Business biz = player.GetData("WayPointBiz");
                            Trigger.ClientEvent(player, "createWaypoint", biz.UnloadPoint.X, biz.UnloadPoint.Y);
                        }
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 300)
                    {
                        respawnCar(vehicle);
                        Trigger.ClientEvent(player, "SetOrderTruck", null);
                        player.ResetData("WayPointBiz");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        cancelOrder(player);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                } catch(Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle_Truckers: \n" + e.ToString(), nLog.Type.Error);
                }
            });
        }

        public static void playerGotProducts(Client player)
        {
            try
            {
                if (!player.HasData("ORDER") || !player.IsInVehicle) return;

                Vehicle veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(player.Vehicle);
                if (!veh.HasData("ACCESS") || veh.GetData("ACCESS") != "WORK" || !player.HasData("WORK") || player.GetData("WORK") != veh) return;

                int uid = player.GetData("ORDER");
                if (!BusinessManager.Orders.ContainsKey(uid))
                {
                    cancelOrder(player);
                    return;
                }
                Business biz = BusinessManager.BizList[BusinessManager.Orders[uid]];
                Order order = biz.Orders.FirstOrDefault(o => o.UID == uid);
                if (order == null)
                {
                    cancelOrder(player);
                    return;
                }
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Довезите товар до {BusinessManager.BusinessTypeNames[biz.Type]}", 3000);

                player.SetData("GOTPRODUCT", true);
                player.SetData("WayPointBiz", biz);
                Trigger.ClientEvent(player, "createWaypoint", biz.UnloadPoint.X, biz.UnloadPoint.Y);
                Trigger.ClientEvent(player, "createCheckpoint", 10, 1, biz.UnloadPoint, 7, 0, 255, 0, 0);
            }
            catch (Exception ex)
            {
                Log.Write($"Error: {ex.Message} ", nLog.Type.Error);
            }
        }

        public static void onEntityEnterDropTrailer(ColShape shape, Client player)
        {
            try
            {
                if (!player.HasData("ORDER") || BusinessManager.Orders[(int)player.GetData("ORDER")] != shape.GetData("BIZID") || !player.HasData("GOTPRODUCT")) return;

                int uid = player.GetData("ORDER");
                if (!BusinessManager.Orders.ContainsKey(uid))
                {
                    cancelOrder(player);
                    return;
                }
                Business biz = BusinessManager.BizList[BusinessManager.Orders[uid]];
                Order order = biz.Orders.FirstOrDefault(o => o.UID == uid);
                if (order == null)
                {
                    cancelOrder(player);
                    return;
                }
                Vehicle trailer = player.GetData("TRAILER");
                if (player.Position.DistanceTo(trailer.Position) > 20)
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Трейлер должен находиться по-близости", 3000);
                    return;
                }
                var payment = Convert.ToInt32(order.Amount * BusinessManager.ProductsOrderPrice[order.Name] * 0.1 * Main.oldconfig.PaydayMultiplier);
                var max = Convert.ToInt32(2000 * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                var min = Convert.ToInt32(500 * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                if (payment > max) payment = max;
                else if (payment < min) payment = min;
                MoneySystem.Wallet.Change(player, payment);
                GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"truckerCheck");
                var ow = NAPI.Player.GetPlayerFromName(biz.Owner);
                Trigger.ClientEvent(player, "SetOrderTruck", null);
                player.ResetData("WayPointBiz");
                if (ow != null)
                    Notify.Send(ow, NotifyType.Warning, NotifyPosition.BottomCenter, $"Ваш заказ на {order.Name} был доставлен", 3000);
                foreach (var p in biz.Products)
                {
                    if (p.Name != order.Name) continue;
                    p.Ordered = false;
                    p.Lefts += order.Amount;
                    break;
                }
                biz.Orders.Remove(order);
                BusinessManager.Orders.Remove(uid);
                
                Trigger.ClientEvent(player, "deleteCheckpoint", 10);
                player.ResetData("GOTPRODUCT");
                player.ResetData("ORDER");
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player.HasData("TRAILER"))
                        {
                            NAPI.Entity.DeleteEntity(player.GetData("TRAILER"));
                            player.ResetData("TRAILER");
                        }
                    } catch { }
                });
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы доставили трейлер", 3000);
                return;
            }
            catch (Exception e) { Log.Write("onEntityDropTrailer_ATTENTION: " + e.Message, nLog.Type.Error); }
        }

        public static void truckerOrders(Client player)
        {
            if (Main.Players[player].WorkID != 6)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете дальнобойщиком", 3000);
                return;
            }
            if (!player.GetData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете в данный момент", 3000);
                return;
            }
            WorkManager.OpenTruckersOrders(player);
        }

        [ServerEvent(Event.VehicleTrailerChange)]
        public static void onVehicleTrailerChange(Vehicle vehicle, Vehicle trailer)
        {
            
        }
    }
}
