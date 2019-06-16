using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEvo.Houses
{
    class Hotel : Script
    {
        private static nLog Log = new nLog("Hotel");

        public static int HotelRent = 100;
        public static List<Vector3> HotelEnters = new List<Vector3>()
        {
            new Vector3(435.7797, 215.2411, 102.0459),
            new Vector3(-1274.113, 315.5634, 64.39182),
            new Vector3(-877.9172, -2178.256, 8.689036),
            new Vector3(416.5617, -1108.615, 28.93262),
        };
        private static List<Vector3> CarsGet = new List<Vector3>()
        {
            new Vector3(463.2156, 222.7217, 101.9851),
            new Vector3(-1297.033, 251.5936, 61.63777),
            new Vector3(-889.4531, -2180.477, 7.47327),
            new Vector3(407.5778, -1100.066, 28.28551),
        };
        private static Vector3 InteriorDoor = new Vector3(151.2052, -1008.007, -100.12);

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var HotelID = 0;
                foreach (var pos in HotelEnters)
                {
                    var blip = NAPI.Blip.CreateBlip(pos);
                    blip.ShortRange = true;
                    blip.Sprite = 475;
                    blip.Color = 59;
                    blip.Name = "Hotel";

                    var colshape = NAPI.ColShape.CreateCylinderColShape(pos, 1.5f, 5f, 0);
                    colshape.SetData("ID", HotelID);
                    colshape.OnEntityEnterColShape += (s, e) =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(e)) return;
                            e.SetData("HOTEL_ID", s.GetData("ID"));
                            e.SetData("INTERACTIONCHECK", 48);
                        }
                        catch (Exception ex) { Log.Write("Enter.colshape.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    colshape.OnEntityExitColShape += (s, e) =>
                    {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("Enter.colshape.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                    };

                    NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                    NAPI.TextLabel.CreateTextLabel("~g~Hotel", pos + new Vector3(0, 0, 0.5), 5f, 0.4f, 0, new Color(255, 255, 255));
                    HotelID++;
                }

                HotelID = 0;
                foreach (var pos in CarsGet)
                {
                    var colshape = NAPI.ColShape.CreateCylinderColShape(pos, 1.5f, 5f, 0);
                    colshape.SetData("ID", HotelID);
                    colshape.OnEntityEnterColShape += (s, e) =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(e)) return;
                            e.SetData("HOTEL_ID", s.GetData("ID"));
                            e.SetData("INTERACTIONCHECK", 50);
                        }
                        catch (Exception ex) { Log.Write("CarsGet.colshape.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    colshape.OnEntityExitColShape += (s, e) =>
                    {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("CarsGet.colshape.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                    };

                    NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                    NAPI.TextLabel.CreateTextLabel("~g~Hotel scooters", pos + new Vector3(0, 0, 0.5), 5f, 0.4f, 0, new Color(255, 255, 255));
                    HotelID++;
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_InteractPressed(Client player, int action)
        {
            switch (action)
            {
                case 48:
                    if (Main.Players[player].HotelID != -1 && Main.Players[player].HotelID == player.GetData("HOTEL_ID"))
                        SendToRoom(player);
                    else if (Main.Players[player].HotelID == -1)
                        OpenHotelBuyMenu(player);
                    else
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы уже арендовали отель", 3000);
                    return;
                case 49:
                    NAPI.Entity.SetEntityPosition(player, HotelEnters[Main.Players[player].InsideHotelID] + new Vector3(0, 0, 1.5));
                    Main.Players[player].InsideHotelID = -1;
                    NAPI.ColShape.DeleteColShape(player.GetData("InsideHotel_ColShape"));
                    NAPI.Entity.DeleteEntity(player.GetData("InsideHotel_Marker"));
                    Dimensions.DismissPrivateDimension(player);
                    NAPI.Entity.SetEntityDimension(player, 0);
                    player.SetData("INTERACTIONCHECK", 0);
                    return;
                case 50:
                    if (player.GetData("HOTEL_ID") == Main.Players[player].HotelID)
                    {
                        if (player.HasData("HOTELCAR"))
                            NAPI.Entity.DeleteEntity(player.GetData("HOTELCAR"));
                        var vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Faggio2, player.Position, player.Rotation, 0, 0, "HOTEL");
                        VehicleStreaming.SetEngineState(vehicle, true);
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        NAPI.Player.SetPlayerIntoVehicle(player, vehicle, -1);
                        player.SetData("HOTELCAR", vehicle);
                        vehicle.SetData("ACCESS", "HOTEL");
                        vehicle.SetData("OWNER", player);
                    }
                    return;
            }
        }

        public static void Event_OnPlayerDisconnected(Client player)
        {
            if (player.HasData("HOTELCAR"))
                NAPI.Entity.DeleteEntity(player.GetData("HOTELCAR"));
        }

        public static void SendToRoom(Client player)
        {
            if (!Main.Players.ContainsKey(player) || Main.Players[player].HotelID == -1) return;

            var dim = Dimensions.RequestPrivateDimension(player);
            NAPI.Entity.SetEntityPosition(player, InteriorDoor + new Vector3(0, 0, 1.12));
            NAPI.Entity.SetEntityDimension(player, dim);
            Main.Players[player].InsideHotelID = Main.Players[player].HotelID;
            var colShape = NAPI.ColShape.CreateCylinderColShape(InteriorDoor, 1.5f, 3, dim);
            colShape.OnEntityEnterColShape += (s, e) =>
            {
                try
                {
                    e.SetData("INTERACTIONCHECK", 49);
                }
                catch (Exception ex) { Log.Write("CarsGet.colshape.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
            };
            colShape.OnEntityExitColShape += (s, e) =>
            {
                try
                {
                    e.SetData("INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Log.Write("CarsGet.colshape.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
            };
            player.SetData("InsideHotel_ColShape", colShape);
            var marker = NAPI.Marker.CreateMarker(1, InteriorDoor, new Vector3(), new Vector3(), 1, new Color(255, 255, 255), false, dim);
            player.SetData("InsideHotel_Marker", marker);
        }

        public static void ExtendHotelRent(Client player, int hours)
        {
            if (!Main.Players.ContainsKey(player)) return;

            if (Main.Players[player].HotelID == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не поселены ни в один отель", 3000);
                return;
            }

            if (Main.Players[player].HotelLeft + hours > 10)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Аренда может быть оплачена только на 10 часов", 3000);
                return;
            }

            if (!MoneySystem.Wallet.Change(player, -HotelRent * hours))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                return;
            }
            GameLog.Money($"player({Main.Players[player].UUID})", $"server", HotelRent * hours, $"hotelRent");
            Main.Players[player].HotelLeft += hours;
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы продлили аренду на {hours} часов, Вас выселят через {Main.Players[player].HotelLeft} часов", 3000);
        }

        public static void MoveOutPlayer(Client player)
        {
            if (Main.Players[player].InsideHotelID != -1)
                NAPI.Entity.SetEntityPosition(player, HotelEnters[Main.Players[player].InsideHotelID] + new Vector3(0, 0, 1.12));

            if (!Main.Players.ContainsKey(player)) return;

            Main.Players[player].HotelID = -1;
            Main.Players[player].HotelLeft = 0;

            if (player.HasData("HOTELCAR"))
            {
                NAPI.Entity.DeleteEntity(player.GetData("HOTELCAR"));
                player.ResetData("HOTELCAR");
            }
        }

        public static void OpenHotelBuyMenu(Client player)
        {
            Menu menu = new Menu("hotelbuy", false, false);
            menu.Callback += callback_hotelbuy;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = $"Отель";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info", Menu.MenuItem.Card);
            menuItem.Text = $"Деньги за аренду будут сниматься каждый пейдей только когда Вы в игре.";
            menu.Add(menuItem);

            menuItem = new Menu.Item("rent", Menu.MenuItem.Button);
            menuItem.Text = $"Арендовать ({HotelRent}$)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_hotelbuy(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "rent":
                    if (Houses.HouseManager.GetHouse(player) != null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы проживаете в доме и не можете арендовать комнату в отеле", 3000);
                        return;
                    }

                    if (!MoneySystem.Wallet.Change(player, -HotelRent))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }
                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", HotelRent, $"hotelRent");
                    Main.Players[player].HotelID = player.GetData("HOTEL_ID");
                    Main.Players[player].HotelLeft = 1;

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы арендовали комнату в отеле на 1ч. Продлить аренду можно в телефоне (M)", 3000);
                    SendToRoom(player);
                    MenuManager.Close(player);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }

        public static void OpenHotelManageMenu(Client player)
        {
            Menu menu = new Menu("hotelmanage", false, false);
            menu.Callback += callback_hotelmanage;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = $"Отель";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info", Menu.MenuItem.Card);
            menuItem.Text = $"Аренда оплачена на {Main.Players[player].HotelLeft}ч";
            menu.Add(menuItem);

            menuItem = new Menu.Item("extend", Menu.MenuItem.Button);
            menuItem.Text = "Продлить аренду";
            menu.Add(menuItem);

            menuItem = new Menu.Item("moveout", Menu.MenuItem.Button);
            menuItem.Text = "Выселиться";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_hotelmanage(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "extend":
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", $"Продлить аренду ({HotelRent}$/ч)", "Введите количество часов", 1, "extend_hotel_rent");
                    return;
                case "moveout":
                    MoveOutPlayer(player);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы выселились из отеля", 3000);
                    MenuManager.Close(player);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }
    }
}
