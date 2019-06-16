using GTANetworkAPI;
using System.Collections.Generic;
using NeptuneEvo.GUI;
using System;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.Jobs
{
    class AutoMechanic : Script
    {
        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void mechanicCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 8);
                NAPI.Data.SetEntityData(veh, "TYPE", "MECHANIC");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntitySharedData(veh, "FUELTANK", 0);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
            }
        }
        private static nLog Log = new nLog("Mechanic");

        private static int mechanicRentCost = 100;
        private static Dictionary<Client, ColShape> orderCols = new Dictionary<Client, ColShape>();

        public static void mechanicRepair(Client player, Client target, int price)
        {
            if (Main.Players[player].WorkID != 8 || !player.GetData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работает автомехаником", 3000);
                return;
            }
            if (!player.IsInVehicle || !player.Vehicle.HasData("TYPE") || player.Vehicle.GetData("TYPE") != "MECHANIC")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в рабочем транспорте", 3000);
                return;
            }
            if (!target.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок должен находиться в транспортном средстве", 3000);
                return;
            }
            if (player.Vehicle.Position.DistanceTo(target.Vehicle.Position) > 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
                return;
            }
            if (price < 50 || price > 300)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы можете установить цену от 50$ до 300$", 3000);
                return;
            }
            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно денег", 3000);
                return;
            }
            
            target.SetData("MECHANIC", player);
            target.SetData("MECHANIC_PRICE", price);
            Trigger.ClientEvent(target, "openDialog", "REPAIR_CAR", $"Игрок ({player.Value}) предложил отремонтировать Ваш транспорт за ${price}");
            
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили игроку ({target.Value}) отремонтировать транспорт за {price}$", 3000);
        }

        public static void mechanicRent(Client player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != -1 || player.Vehicle.GetData("TYPE") != "MECHANIC") return;

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы арендовали рабочий транспорт. Ожидайте заказ", 3000);
            MoneySystem.Wallet.Change(player, -mechanicRentCost);
            GameLog.Money($"player({Main.Players[player].UUID})", $"server", mechanicRentCost, $"mechanicRent");
            var vehicle = player.Vehicle;
            NAPI.Data.SetEntityData(player, "WORK", vehicle);
            Core.VehicleStreaming.SetEngineState(vehicle, false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(player, "ON_WORK", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
        }

        public static void mechanicPay(Client player)
        {
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспортном средстве", 3000);
                return;
            }

            var price = NAPI.Data.GetEntityData(player, "MECHANIC_PRICE");
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств", 3000);
                return;
            }

            VehicleManager.RepairCar(player.Vehicle);
            var driver = NAPI.Data.GetEntityData(player, "MECHANIC");
            MoneySystem.Wallet.Change(player, -price);
            MoneySystem.Wallet.Change(driver, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[driver].UUID})", price, $"mechanicRepair");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили ремонт Вашего транспортного средства", 3000);
            Notify.Send(driver, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) оплатил ремонт", 3000);
            Commands.RPChat("me", driver, $"починил автомобиль");

            player.ResetData("MECHANIC_DRIVER");
            driver.ResetData("MECHANIC_CLIENT");
            try
            {
                NAPI.ColShape.DeleteColShape(orderCols[player]);
                orderCols.Remove(player);
            }
            catch { }
        }

        private static void order_onEntityExit(ColShape shape, Client player)
        {
            if (shape.GetData("MECHANIC_CLIENT") != player) return;

            if (player.HasData("MECHANIC_DRIVER"))
            {
                Client driver = player.GetData("MECHANIC_DRIVER");
                driver.ResetData("MECHANIC_CLIENT");
                player.ResetData("MECHANIC_DRIVER");
                player.SetData("IS_CALL_MECHANIC", false);
                Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"Клиент отменил заказ", 3000);
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы покинули место вызова автомеханика", 3000);
                try
                {
                    NAPI.ColShape.DeleteColShape(orderCols[player]);
                    orderCols.Remove(player);
                }
                catch { }
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void Event_onPlayerEnterVehicleHandler(Client player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "MECHANIC") return;
                if (seatid == -1)
                {
                    if (!Main.Players[player].Licenses[1])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории B", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].WorkID == 8)
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (vehicle.GetData("DRIVER") != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Этот рабочий транспорт уже занят", 3000);
                                return;
                            }
                            if (Main.Players[player].Money >= mechanicRentCost)
                            {
                                Trigger.ClientEvent(player, "openDialog", "MECHANIC_RENT", $"Арендовать рабочий транспорт за ${mechanicRentCost}?");
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает " + (mechanicRentCost - Main.Players[player].Money) + "$ на аренду рабочего транспорта", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle) NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                        else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже работаете", 3000);
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете автомехаником. Устроиться можно в мэрии", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void respawnCar(Vehicle veh)
        {
            try
            {
                int i = NAPI.Data.GetEntityData(veh, "NUMBER");
                NAPI.Entity.SetEntityPosition(veh, CarInfos[i].Position);
                NAPI.Entity.SetEntityRotation(veh, CarInfos[i].Rotation);
                VehicleManager.RepairCar(veh);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 8);
                NAPI.Data.SetEntityData(veh, "TYPE", "MECHANIC");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntitySharedData(veh, "FUELTANK", 0);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("RespawnCar: " + e.Message, nLog.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("MECHANIC_DRIVER"))
                {
                    Client driver = player.GetData("MECHANIC_DRIVER");
                    driver.ResetData("MECHANIC_CLIENT");
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"Клиент отменил заказ", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
                if ((Main.Players[player].WorkID == 8 && NAPI.Data.GetEntityData(player, "ON_WORK") && NAPI.Data.GetEntityData(player, "WORK") != null))
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    respawnCar(vehicle);
                    if (player.HasData("MECHANIC_CLIENT"))
                    {
                        Client client = player.GetData("MECHANIC_CLIENT");
                        client.ResetData("MECHANIC_DRIVER");
                        client.SetData("IS_CALL_MECHANIC", false);
                        Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, $"Автомеханик покинул рабочее место, сделайте новый заказ", 3000);
                        try
                        {
                            NAPI.ColShape.DeleteColShape(orderCols[client]);
                            orderCols.Remove(client);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_onPlayerExitVehicleHandler(Client player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" &&
                Main.Players[player].WorkID == 8 &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 5 минут, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_1");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "AUM_EXIT_CAR_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.Start(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, nLog.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Client player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_2");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 300)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        respawnCar(vehicle);
                        player.SetData("ON_WORK", false);
                        player.SetData("WORK", null);
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_3");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        if (player.HasData("MECHANIC_CLIENT"))
                        {
                            Client client = player.GetData("MECHANIC_CLIENT");
                            client.ResetData("MECHANIC_DRIVER");
                            client.SetData("IS_CALL_MECHANIC", false);
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Автомеханик покинул рабочее место, сделайте новый заказ", 3000);
                            player.ResetData("MECHANIC_CLIENT");
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[client]);
                                orderCols.Remove(client);
                            }
                            catch { }
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                } catch(Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle:\n" + e.ToString(), nLog.Type.Error);
                }
            });
        }

        public static void acceptMechanic(Client player, Client target)
        {
            if (Main.Players[player].WorkID == 8 && player.GetData("ON_WORK"))
            {
                if (player.HasData("MECHANIC_CLIENT"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже взяли заказ", 3000);
                    return;
                }
                if (NAPI.Data.GetEntityData(target, "IS_CALL_MECHANIC") && !target.HasData("MECHANIC_DRIVER"))
                {
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) принял Ваш вызов. Оставайтесь на мест", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы приняли вызов игрока ({target.Value})", 3000);
                    Trigger.ClientEvent(player, "createWaypoint", NAPI.Entity.GetEntityPosition(target).X, NAPI.Entity.GetEntityPosition(target).Y);

                    target.SetData("MECHANIC_DRIVER", player);
                    player.SetData("MECHANIC_CLIENT", target);

                    orderCols.Add(target, NAPI.ColShape.CreateCylinderColShape(target.Position, 10F, 10F, 0));
                    orderCols[target].SetData("MECHANIC_CLIENT", target);
                    orderCols[target].OnEntityExitColShape += order_onEntityExit;
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не вызывал автомеханика", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете автомехаником в данный момент", 3000);
        }

        public static void cancelMechanic(Client player)
        {
            if (player.HasData("MECHANIC_CLIENT"))
            {
                Client client = player.GetData("MECHANIC_CLIENT");
                client.ResetData("MECHANIC_DRIVER");
                client.SetData("IS_CALL_MECHANIC", false);
                player.ResetData("MECHANIC_CLIENT");
                Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, $"Автомеханик покинул рабочее место, сделайте новый заказ", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отменили выезд к клиенту", 3000);
                try
                {
                    NAPI.ColShape.DeleteColShape(orderCols[client]);
                    orderCols.Remove(client);
                }
                catch { }
                return;
            }
            if (NAPI.Data.GetEntityData(player, "IS_CALL_MECHANIC"))
            {
                NAPI.Data.SetEntityData(player, "IS_CALL_MECHANIC", false);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отменили вызов автомеханика", 3000);
                if (player.HasData("MECHANIC_DRIVER"))
                {
                    Client driver = player.GetData("MECHANIC_DRIVER");
                    driver.ResetData("MECHANIC_CLIENT");
                    player.ResetData("MECHANIC_DRIVER");
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"Клиент отменил заказ", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не вызывали автомеханика.", 3000);
        }

        public static void callMechanic(Client player)
        {
            if (!NAPI.Data.GetEntityData(player, "IS_CALL_MECHANIC"))
            {
                List<Client> players = NAPI.Pools.GetAllPlayers();
                var i = 0;
                foreach (var p in players)
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].WorkID == 8 && NAPI.Data.GetEntityData(p, "ON_WORK"))
                    {
                        i++;
                        NAPI.Chat.SendChatMessageToPlayer(p, $"~g~[ДИСПЕТЧЕР]: ~w~Игрок ({player.Value}) вызвал автомеханика ~y~({player.Position.DistanceTo(p.Position)}м)~w~. Напишите ~y~/ma ~b~[ID]~w~, чтобы принять вызов");
                    }
                }
                if (i > 0)
                {
                    NAPI.Data.SetEntityData(player, "IS_CALL_MECHANIC", true);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ожидайте принятия вызова. В Вашем районе сейчас {i} автомехаников. Для отмены вызова используйте /cmechanic", 3000);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В Вашем районе сейчас нет автомехаников. Попробуйте в другой раз", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже вызвали автомеханика. Для отмены напишите /cmechanic", 3000);
        }

        public static void buyFuel(Client player, int fuel)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].WorkID != 8 || !player.GetData("ON_WORK") || !player.IsInVehicle || player.GetData("WORK") != player.Vehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны работать автомехаником и находиться в рабочей машине", 3000);
                return;
            }
            if (player.GetData("BIZ_ID") == -1 || BusinessManager.BizList[player.GetData("BIZ_ID")].Type != 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на заправке", 3000);
                return;
            }
            if (fuel <= 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData("BIZ_ID")];
            if (Main.Players[player].Money < biz.Products[0].Price * fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }
            if (player.Vehicle.GetSharedData("FUELTANK") + fuel > 1000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Бак с бензином полон", 3000);
                return;
            }
            if (!BusinessManager.takeProd(biz.ID, fuel, biz.Products[0].Name, biz.Products[0].Price * fuel))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно топлива на заправке", 3000);
                return;
            }
            MoneySystem.Wallet.Change(player, -biz.Products[0].Price * fuel);
            GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", biz.Products[0].Price * fuel, $"mechanicBuyFuel");
            player.Vehicle.SetSharedData("FUELTANK", player.Vehicle.GetSharedData("FUELTANK") + fuel);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы пополнили бак в вашей рабочей машине до {player.Vehicle.GetSharedData("FUELTANK")}л", 3000);
        }

        public static void mechanicFuel(Client player, Client target, int fuel, int pricePerLitr)
        {
            if (Main.Players[player].WorkID != 8 || !player.GetData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работает автомехаником", 3000);
                return;
            }
            if (!player.IsInVehicle || !player.Vehicle.HasData("TYPE") || player.Vehicle.GetData("TYPE") != "MECHANIC")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в рабочем транспорте", 3000);
                return;
            }
            if (!target.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок должен находиться в транспортном средстве", 3000);
                return;
            }
            if (player.Vehicle.Position.DistanceTo(target.Vehicle.Position) > 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
                return;
            }
            if (fuel < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете продать меньше литра", 3000);
                return;
            }
            if (pricePerLitr < 2 || pricePerLitr > 10)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы можете установить цену от 2$ до 10$ за литр", 3000);
                return;
            }
            if (Main.Players[target].Money < pricePerLitr * fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно денег", 3000);
                return;
            }
            
            target.SetData("MECHANIC", player);
            target.SetData("MECHANIC_PRICE", pricePerLitr);
            target.SetData("MECHANIC_FEUL", fuel);
            Trigger.ClientEvent(target, "openDialog", "FUEL_CAR", $"Игрок ({player.Value}) предложил заправить Ваш транспорт на {fuel}л за ${fuel * pricePerLitr}");
            
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили игроку ({target.Value}) заправить транспорт на {fuel}л за {fuel * pricePerLitr}$.", 3000);
        }

        public static void mechanicPayFuel(Client player)
        {
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспортном средстве", 3000);
                return;
            }

            var price = NAPI.Data.GetEntityData(player, "MECHANIC_PRICE");
            var fuel = NAPI.Data.GetEntityData(player, "MECHANIC_FEUL");
            if (Main.Players[player].Money < price * fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств", 3000);
                return;
            }

            Client driver = NAPI.Data.GetEntityData(player, "MECHANIC");

            if (!driver.IsInVehicle || !driver.Vehicle.HasData("TYPE") || driver.Vehicle.GetData("TYPE") != "MECHANIC")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Механик должен находиться в транспортном средстве", 3000);
                return;
            }

            if (driver.Vehicle.GetSharedData("FUELTANK") < fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У механика недостаточно топлива, чтобы заправить Вас", 3000);
                return;
            }

            MoneySystem.Wallet.Change(player, -price * fuel);
            MoneySystem.Wallet.Change(driver, price * fuel);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[driver].UUID})", price * fuel, $"mechanicFuel");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили ремонт заправку транспортного средства", 3000);
            Notify.Send(driver, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) оплатил заправку транспорта", 3000);
            Commands.RPChat("me", driver, $"заправил транспортное средство");

            var carFuel = (player.Vehicle.GetSharedData("PETROL") + fuel > player.Vehicle.GetSharedData("MAXPETROL")) ? player.Vehicle.GetSharedData("MAXPETROL") : player.Vehicle.GetSharedData("PETROL") + fuel;
            player.Vehicle.SetSharedData("PETROL", carFuel);
            driver.Vehicle.SetSharedData("FUELTANK", driver.Vehicle.GetSharedData("FUELTANK") - fuel);
            player.ResetData("MECHANIC_DRIVER");
            driver.ResetData("MECHANIC_CLIENT");
            try
            {
                NAPI.ColShape.DeleteColShape(orderCols[player]);
                orderCols.Remove(player);
            }
            catch { }
        }
    }
}
