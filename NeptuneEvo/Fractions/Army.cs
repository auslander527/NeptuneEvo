using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using System;

namespace NeptuneEvo.Fractions
{
    class Army : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.TextLabel.CreateTextLabel("~g~Benson Pain", new Vector3(-2347.958, 3268.936, 33.81076), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[0], 1, 2, 0));
                Cols[0].SetData("INTERACT", 34);
                Cols[0].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[0].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E, to open gun menu"), new Vector3(ArmyCheckpoints[0].X, ArmyCheckpoints[0].Y, ArmyCheckpoints[0].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 35);
                Cols[1].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[1].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E, to change clothes"), new Vector3(ArmyCheckpoints[1].X, ArmyCheckpoints[1].Y, ArmyCheckpoints[1].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[2], 5, 6, 0));
                Cols[2].SetData("INTERACT", 36);
                Cols[2].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[2].OnEntityExitColShape += onEntityExitArmyMats;

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[3], 1, 2, 0));
                Cols[3].SetData("INTERACT", 25);
                Cols[3].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[3].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Lift"), new Vector3(ArmyCheckpoints[3].X, ArmyCheckpoints[3].Y, ArmyCheckpoints[3].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(4, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[4], 1, 2, 0));
                Cols[4].SetData("INTERACT", 25);
                Cols[4].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[4].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Lift"), new Vector3(ArmyCheckpoints[4].X, ArmyCheckpoints[4].Y, ArmyCheckpoints[4].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[5], 1, 2, 0));
                Cols[5].SetData("INTERACT", 60);
                Cols[5].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[5].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Open gun stock"), new Vector3(ArmyCheckpoints[5].X, ArmyCheckpoints[5].Y, ArmyCheckpoints[5].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[2], new Vector3(), new Vector3(), 5f, new Color(155, 0, 0));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[4] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[5] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static nLog Log = new nLog("Army");

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> ArmyCheckpoints = new List<Vector3>()
        {
            new Vector3(-2345.839, 3268.359, 31.81075), // guns     0
            new Vector3(-2357.993, 3255.133, 31.81073), // dressing room    1
            new Vector3(-108.0619, -2414.873, 5.000001), // army docks mats     2
            new Vector3(-2360.946, 3249.595, 31.81073), // army lift 1 floor     3
            new Vector3(-2360.66, 3249.115, 91.90369), // army lift 9 floor     4
            new Vector3(-2349.892, 3266.55, 31.69072), // army stock    5
        };

        [RemoteEvent("armygun")]
        public static void callback_armyGuns(Client client, int index)
        {
            try
            {
                switch (index)
                {
                    case 0: //pistol
                        Fractions.Manager.giveGun(client, Weapons.Hash.Pistol, "pistol");
                        return;
                    case 1: //carbine
                        Fractions.Manager.giveGun(client, Weapons.Hash.CarbineRifle, "carbine");
                        return;
                    case 2: // combat
                        Fractions.Manager.giveGun(client, Weapons.Hash.CombatMG, "CombatMG");
                        return;
                    case 3: //armor
                        if (!Manager.canGetWeapon(client, "armor")) return;
                        if (Fractions.Stocks.fracStocks[14].Materials > Fractions.Manager.matsForArmor && nInventory.Find(Main.Players[client].UUID, ItemType.BodyArmor) == null)
                        {
                            nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                            Fractions.Stocks.fracStocks[14].Materials -= Fractions.Manager.matsForArmor;
                            Fractions.Stocks.fracStocks[14].UpdateLabel();
                            GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "armor", 1, false);
                            Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        }
                        return;
                    case 4:
                        if (!Manager.canGetWeapon(client, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[14].Medkits == 0)
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
                        Fractions.Stocks.fracStocks[14].Medkits--;
                        Fractions.Stocks.fracStocks[14].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 5: //pistolammo
                        if (!Manager.canGetWeapon(client, "PistolAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.PistolAmmo, 12);
                        return;
                    case 6: //riflesammo
                        if (!Manager.canGetWeapon(client, "RiflesAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.RiflesAmmo, 30);
                        return;
                    case 7: //smgammo
                        if (!Manager.canGetWeapon(client, "SMGAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.SMGAmmo, 100);
                        return;
                }
            }
            catch (Exception e) { Log.Write("ArmyGun: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Client player, int interact)
        {
            switch (interact)
            {
                case 34:
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[14].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    Trigger.ClientEvent(player, "armyguns");
                    return;
                case 35:
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    OpenArmyClothesMenu(player);
                    return;
                case 36:
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    if (!player.IsInVehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                        return;
                    }
                    if (!player.Vehicle.HasData("CANMATS"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На этой машине нельзя перевозить маты", 3000);
                        return;
                    }
                    if (player.HasData("loadMatsTimer"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже загружаете материалы в машину", 3000);
                        return;
                    }
                    if (!Fractions.Stocks.maxMats.ContainsKey(player.Vehicle.DisplayName)) return;
                    var count = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
                    if (count >= Fractions.Stocks.maxMats[player.Vehicle.DisplayName])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине максимальное кол-во материала", 3000);
                        return;
                    }
                    //player.SetData("loadMatsTimer", Main.StartT(20000, 99999999, (o) => loadMaterialsTimer(player), "ALOADMATS_TIMER"));
                    player.SetData("loadMatsTimer", Timers.StartOnce(20000, () => loadMaterialsTimer(player)));
                    player.Vehicle.SetData("loaderMats", player);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Загрузка материалов началась (20 секунд)", 3000);
                    Trigger.ClientEvent(player, "showLoader", "Загрузка материалов", 1);
                    player.SetData("vehicleMats", player.Vehicle);
                    player.SetData("whereLoad", "ARMY");
                    return;
                case 25:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    if (player.Position.Z > 50)
                    {
                        NAPI.Entity.SetEntityPosition(player, new Vector3(ArmyCheckpoints[3].X, ArmyCheckpoints[3].Y, ArmyCheckpoints[3].Z + 1));
                        Main.PlayerEnterInterior(player, new Vector3(ArmyCheckpoints[3].X, ArmyCheckpoints[3].Y, ArmyCheckpoints[3].Z + 1));
                    }
                    else
                    {
                        NAPI.Entity.SetEntityPosition(player, new Vector3(ArmyCheckpoints[4].X, ArmyCheckpoints[4].Y, ArmyCheckpoints[4].Z + 1));
                        Main.PlayerEnterInterior(player, new Vector3(ArmyCheckpoints[4].X, ArmyCheckpoints[4].Y, ArmyCheckpoints[4].Z + 1));
                    }
                    return;
                case 60: // open stock gun
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[14].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    player.SetData("ONFRACSTOCK", 14);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[14].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        #region shapes

        private static void onEntityEnterColshape(ColShape shape, Client player)
        {
            try
            {
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", shape.GetData("INTERACT"));
                if (shape.GetData("INTERACT") == 36) Trigger.ClientEvent(player, "interactHint", true);
            }
            catch (Exception e) { Log.Write("onEntityEnterColshape: " + e.Message, nLog.Type.Error); }
        }

        private static void onEntityExitColshape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("onEntityExitColshape: " + e.Message, nLog.Type.Error); }
        }

        private static void onEntityExitArmyMats(ColShape shape, Client player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
            Trigger.ClientEvent(player, "interactHint", false);
            if (NAPI.Data.HasEntityData(player, "loadMatsTimer"))
            {
                //Main.StopT(player.GetData("loadMatsTimer"), "onEntityExitArmyMats");
                Timers.Stop(player.GetData("loadMatsTimer"));
                player.ResetData("loadMatsTimer");
                Trigger.ClientEvent(player, "hideLoader");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
            }
        }

        #endregion

        public static void loadMaterialsTimer(Client player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("vehicleMats")) return;
                    if (!player.IsInVehicle) return;
                    Vehicle vehicle = player.GetData("vehicleMats");

                    var itemCount = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
                    if (player.GetData("whereLoad") == "WAR" && !Fractions.MatsWar.isWar)
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                        //Main.StopT(player.GetData("loadMatsTimer"), "loadMaterialsTimer");
                        player.ResetData("loadMatsTimer");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Корабль уже уехал", 3000);
                        return;
                    }
                    if (itemCount >= Fractions.Stocks.maxMats[vehicle.DisplayName])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине максимальное кол-во материала", 3000);
                        //Main.StopT(player.GetData("loadMatsTimer"), "loadMaterialsTimer_1");
                        player.ResetData("loadMatsTimer");
                        return;
                    }
                    var data = new nItem(ItemType.Material);
                    if (player.GetData("whereLoad") == "WAR")
                    {
                        var count = Fractions.Stocks.maxMats[vehicle.DisplayName] - itemCount;
                        if (count >= Fractions.MatsWar.matsLeft)
                        {
                            data.Count = itemCount + Fractions.MatsWar.matsLeft;
                            Fractions.MatsWar.matsLeft = 0;
                            Fractions.MatsWar.endWar();
                        }
                        else
                        {
                            data.Count = count;
                            Fractions.MatsWar.matsLeft -= count;
                        }
                    }
                    else
                        data.Count = Fractions.Stocks.maxMats[vehicle.DisplayName] - itemCount;
                    VehicleInventory.Add(vehicle, data);
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                    //Main.StopT(player.GetData("loadMatsTimer"), "loadMaterialsTimer_2");
                    player.ResetData("loadMatsTimer");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы загрузили материалы в машину", 3000);
                }
                catch (Exception e) { Log.Write("LoadMatsTimer: " + e.Message, nLog.Type.Error); }
            });
        }
        
        public static void Event_PlayerDeath(Client player, Client entityKiller, uint weapon)
        {
            try
            {
                if (player.HasData("loadMatsTimer"))
                {
                    Trigger.ClientEvent(player, "hideLoader");
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как Вы умерли", 3000);
                    //Main.StopT(player.GetData("loadMatsTimer"), "Event_PlayerDeath_army");
                    Timers.Stop(player.GetData("loadMatsTimer"));
                    var vehicle = player.GetData("vehicleMats");
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                    player.ResetData("loadMatsTimer");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void onPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("loadMatsTimer"))
                {
                    //Main.StopT(player.GetData("loadMatsTimer"), "army_onPlayerDisconnected");
                    Timers.Stop(player.GetData("loadMatsTimer"));
                    var vehicle = player.GetData("vehicleMats");
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.VehicleDeath)]
        public void onVehicleDeath(Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.HasEntityData(vehicle, "loaderMats"))
                {
                    Client player = NAPI.Data.GetEntityData(vehicle, "loaderMats");
                    //Main.StopT(player.GetData("loadMatsTimer"), "");
                    Timers.Stop(player.GetData("loadMatsTimer"));
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                    player.ResetData("loadMatsTimer");
                    Trigger.ClientEvent(player, "hideLoader");
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                }
            }
            catch (Exception e) { Log.Write("VehicleDeath: " + e.Message, nLog.Type.Error); }
        }

        #region menu
        public static void OpenArmyClothesMenu(Client player)
        {
            Menu menu = new Menu("armyclothes", false, false);
            menu.Callback = callback_armyclothes;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Одежда";
            menu.Add(menuItem);

            menuItem = new Menu.Item("change", Menu.MenuItem.Button);
            menuItem.Text = "Переодеться";
            menu.Add(menuItem);

            menuItem = new Menu.Item("combat", Menu.MenuItem.Button);
            menuItem.Text = "Боевая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_armyclothes(Client client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "change":
                    if (Main.Players[client].FractionLVL < 6)
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете переодеться/раздеться", 3000);
                        return;
                    }
                    if (!client.GetData("ON_DUTY"))
                    {
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в служебную форму", 3000);
                        Manager.setSkin(client, 14, Main.Players[client].FractionLVL);
                        client.SetData("ON_DUTY", true);
                    }
                    else
                    {
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в повседневную одежду", 3000);
                        Customization.ApplyCharacter(client);
                        if (client.HasData("HAND_MONEY")) client.SetClothes(5, 45, 0);
                        else if (client.HasData("HEIST_DRILL")) client.SetClothes(5, 41, 0);
                        client.SetData("ON_DUTY", false);
                    }
                    return;
                case "combat":
                    MenuManager.Close(client);
                    OpenArmyCombatMenu(client);
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }

        public static void OpenArmyCombatMenu(Client player)
        {
            Menu menu = new Menu("armycombat", false, false);
            menu.Callback = callback_armycombat;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Боевая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam1", Menu.MenuItem.Button);
            menuItem.Text = "Песочный камуфляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam2", Menu.MenuItem.Button);
            menuItem.Text = "Городской камуфляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam3", Menu.MenuItem.Button);
            menuItem.Text = "Зеленый камуфляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam4", Menu.MenuItem.Button);
            menuItem.Text = "Черно-зеленый камфуляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam5", Menu.MenuItem.Button);
            menuItem.Text = "Какой-то камфуляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam6", Menu.MenuItem.Button);
            menuItem.Text = "И ещё один камфуляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("takeoff", Menu.MenuItem.Button);
            menuItem.Text = "Снять форму";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_armycombat(Client client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "back")
            {
                MenuManager.Close(client);
                OpenArmyClothesMenu(client);
                return;
            }
            if (Main.Players[client].FractionID != 14)
            {
                Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                return;
            }
            if (!client.GetData("ON_DUTY"))
            {
                Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать службу", 3000);
                return;
            }
            client.SetData("IN_CP_MODE", true);
            var gender = Main.Players[client].Gender;
            Customization.ClearClothes(client, gender);
            switch (item.ID)
            {
                case "cam1":
                    if (gender)
                    {
                        client.SetClothes(1, 104, 5);
                        Customization.SetHat(client, 117, 13);
                        client.SetClothes(11, 221, 5);
                        client.SetClothes(4, 87, 5);
                        client.SetClothes(6, 62, 0);
                        client.SetClothes(9, 16, 0);
                        client.SetClothes(3, 49, 1);
                    }
                    else
                    {
                        Customization.SetHat(client, 116, 13);
                        client.SetClothes(1, 104, 5);
                        client.SetClothes(4, 90, 5);
                        client.SetClothes(11, 224, 5);
                        client.SetClothes(6, 65, 0);
                        client.SetClothes(3, 46, 1);
                        client.SetClothes(9, 18, 0);
                    }
                    return;
                case "cam2":
                    if (gender)
                    {
                        client.SetClothes(1, 104, 10);
                        Customization.SetHat(client, 117, 18);
                        client.SetClothes(11, 222, 10);
                        client.SetClothes(4, 87, 10);
                        client.SetClothes(6, 62, 2);
                        client.SetClothes(9, 16, 2);
                        client.SetClothes(3, 48, 0);
                    }
                    else
                    {
                        Customization.SetHat(client, 116, 18);
                        client.SetClothes(1, 104, 10);
                        client.SetClothes(4, 90, 10);
                        client.SetClothes(11, 224, 10);
                        client.SetClothes(6, 65, 2);
                        client.SetClothes(3, 46, 0);
                        client.SetClothes(9, 18, 2);
                    }
                    return;
                case "cam3":
                    if (gender)
                    {
                        client.SetClothes(1, 104, 15);
                        Customization.SetHat(client, 117, 22);
                        client.SetClothes(11, 220, 15);
                        client.SetClothes(4, 87, 15);
                        client.SetClothes(6, 25, 0);
                        client.SetClothes(9, 16, 2);
                        client.SetClothes(3, 49, 0);
                    }
                    else
                    {
                        Customization.SetHat(client, 116, 22);
                        client.SetClothes(1, 104, 15);
                        client.SetClothes(4, 90, 15);
                        client.SetClothes(11, 224, 15);
                        client.SetClothes(6, 25, 0);
                        client.SetClothes(3, 46, 0);
                        client.SetClothes(9, 18, 2);
                    }
                    return;
                case "cam4":
                    if (gender)
                    {
                        client.SetClothes(1, 104, 12);
                        Customization.SetHat(client, 117, 20);
                        client.SetClothes(11, 221, 12);
                        client.SetClothes(4, 87, 12);
                        client.SetClothes(6, 25, 0);
                        client.SetClothes(9, 16, 2);
                        client.SetClothes(3, 49, 0);
                    }
                    else
                    {
                        Customization.SetHat(client, 116, 20);
                        client.SetClothes(1, 104, 12);
                        client.SetClothes(4, 90, 12);
                        client.SetClothes(11, 224, 12);
                        client.SetClothes(6, 25, 0);
                        client.SetClothes(3, 46, 0);
                        client.SetClothes(9, 18, 2);
                    }
                    return;
                case "cam5":
                    if (gender)
                    {
                        client.SetClothes(1, 104, 16);
                        Customization.SetHat(client, 117, 23);
                        client.SetClothes(11, 220, 16);
                        client.SetClothes(4, 87, 16);
                        client.SetClothes(6, 62, 7);
                        client.SetClothes(9, 16, 0);
                        client.SetClothes(3, 48, 0);
                    }
                    else
                    {
                        Customization.SetHat(client, 116, 23);
                        client.SetClothes(1, 104, 16);
                        client.SetClothes(4, 90, 16);
                        client.SetClothes(11, 224, 16);
                        client.SetClothes(6, 65, 7);
                        client.SetClothes(3, 46, 1);
                        client.SetClothes(9, 18, 0);
                    }
                    return;
                case "cam6":
                    if (gender)
                    {
                        client.SetClothes(1, 104, 11);
                        Customization.SetHat(client, 117, 19);
                        client.SetClothes(11, 222, 11);
                        client.SetClothes(4, 87, 11);
                        client.SetClothes(6, 25, 0);
                        client.SetClothes(9, 16, 2);
                        client.SetClothes(3, 48, 0);
                    }
                    else
                    {
                        Customization.SetHat(client, 116, 19);
                        client.SetClothes(1, 104, 11);
                        client.SetClothes(4, 90, 11);
                        client.SetClothes(11, 224, 10);
                        client.SetClothes(6, 25, 0);
                        client.SetClothes(3, 46, 0);
                        client.SetClothes(9, 18, 2);
                    }
                    return;
                case "takeoff":
                    Manager.setSkin(client, Main.Players[client].FractionID, Main.Players[client].FractionLVL);
                    client.SetData("IN_CP_MODE", false);
                    return;
            }
        }
        #endregion
    }
}
