using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using System;
using System.Linq;

namespace NeptuneEvo.Fractions
{
    class Fbi : Script
    {
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static Vector3 EnterFBI = new Vector3(-1581.552, -557.9453, 33.83302);
        private static Vector3 ExitFBI = new Vector3(107.1693, -744.6852, 44.75476);
        private static List<Vector3> fbiCheckpoints = new List<Vector3>()
        {
            new Vector3(147.2835, -757.7181, 241.032), // duty          0
            new Vector3(136.1821, -761.7615, 241.152), // 49 floor       1
            new Vector3(130.9762, -762.3011, 241.1518), // 49 floor to 53          2
            new Vector3(156.81, -757.24, 257.05), // 53 floor          3
            new Vector3(-1561.171, -568.5499, 113.3084), // roof          4
            new Vector3(118.9617, -729.1614, 241.152), // gun menu           5
            new Vector3(136.0578, -761.8408, 44.75204), // 1 floor        6
            new Vector3(-1538.791, -576.0259, 24.66784), // garage       7
            new Vector3(120.0081, -726.7838, 241.032),  // warg mode    8
            new Vector3(151.8786, -736.7075, 241.032), // fbi stock     9
        };
        public static bool warg_mode = false;

        private static nLog Log = new nLog("FBI");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                foreach (Vector3 vec in fbiCheckpoints)
                {
                    NAPI.Marker.CreateMarker(1, vec - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                }

                NAPI.TextLabel.CreateTextLabel("~g~Steve Hain", new Vector3(149.1317, -758.3485, 243.152), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                NAPI.TextLabel.CreateTextLabel("~g~Michael Bisping", new Vector3(120.0836, -726.7773, 243.152), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                #region cols
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[0], 1, 2, 0)); // duty fbi
                Cols[0].SetData("INTERACT", 20);
                Cols[0].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to change clothes"), new Vector3(fbiCheckpoints[0].X, fbiCheckpoints[0].Y, fbiCheckpoints[0].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(EnterFBI, 1, 2, 0)); // fbi enter
                Cols[1].SetData("INTERACT", 21);
                Cols[1].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[1].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(EnterFBI.X, EnterFBI.Y, EnterFBI.Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                NAPI.Marker.CreateMarker(21, EnterFBI + new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(ExitFBI, 1, 2, 0)); // fbi exit
                Cols[2].SetData("INTERACT", 22);
                Cols[2].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[2].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(ExitFBI.X, ExitFBI.Y, ExitFBI.Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                NAPI.Marker.CreateMarker(21, ExitFBI + new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[1], 1, 2, 0)); // 49 floor to 53
                Cols[3].SetData("INTERACT", 23);
                Cols[3].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[3].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(fbiCheckpoints[1].X, fbiCheckpoints[1].Y, fbiCheckpoints[1].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(4, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[2], 1, 2, 0)); // 49 floor
                Cols[4].SetData("INTERACT", 26);
                Cols[4].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[4].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(fbiCheckpoints[2].X, fbiCheckpoints[2].Y, fbiCheckpoints[2].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[3], 1, 2, 0)); // 53 floor to 49
                Cols[5].SetData("INTERACT", 27);
                Cols[5].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[5].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(fbiCheckpoints[3].X, fbiCheckpoints[3].Y, fbiCheckpoints[3].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[4], 1, 2, 0)); // roof
                Cols[6].SetData("INTERACT", 23);
                Cols[6].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[6].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(fbiCheckpoints[4].X, fbiCheckpoints[4].Y, fbiCheckpoints[4].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[5], 1, 2, 0)); // gun menu
                Cols[7].SetData("INTERACT", 24);
                Cols[7].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[7].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to open gun menu"), new Vector3(fbiCheckpoints[5].X, fbiCheckpoints[5].Y, fbiCheckpoints[5].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(8, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[6], 1, 2, 0)); // 1 floor
                Cols[8].SetData("INTERACT", 23);
                Cols[8].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[8].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(fbiCheckpoints[6].X, fbiCheckpoints[6].Y, fbiCheckpoints[6].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(9, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[7], 1, 2, 0)); // garage
                Cols[9].SetData("INTERACT", 23);
                Cols[9].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[9].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(fbiCheckpoints[7].X, fbiCheckpoints[7].Y, fbiCheckpoints[7].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(10, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[8], 1, 2, 0)); // warg
                Cols[10].SetData("INTERACT", 46);
                Cols[10].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[10].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to change SWAT clothes"), new Vector3(fbiCheckpoints[8].X, fbiCheckpoints[8].Y, fbiCheckpoints[8].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(11, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[9], 1, 2, 0)); // stock
                Cols[11].SetData("INTERACT", 61);
                Cols[11].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[11].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Open gun stock"), new Vector3(fbiCheckpoints[9].X, fbiCheckpoints[9].Y, fbiCheckpoints[9].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                #endregion
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Client player, int interact)
        {
            switch (interact)
            {
                case 20:
                    if (Main.Players[player].FractionID == 9)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы начали рабочий день", 3000);
                            Manager.setSkin(player, 9, Main.Players[player].FractionLVL);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                            break;
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                            Customization.ApplyCharacter(player);
                            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                            break;
                        }
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник FBI", 3000);
                    return;
                case 21:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    NAPI.Entity.SetEntityPosition(player, ExitFBI + new Vector3(0, 0, 1.12));
                    Main.PlayerEnterInterior(player, ExitFBI + new Vector3(0, 0, 1.12));
                    return;
                case 22:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    NAPI.Entity.SetEntityPosition(player, EnterFBI + new Vector3(0, 0, 1.12));
                    Main.PlayerEnterInterior(player, EnterFBI + new Vector3(0, 0, 1.12));
                    return;
                case 23:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    OpenFbiLiftMenu(player);
                    return;
                case 24:
                    if (Main.Players[player].FractionID != 9)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник FBI", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[9].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    OpenFbiGunMenu(player);
                    return;
                case 26:
                    NAPI.Entity.SetEntityPosition(player, fbiCheckpoints[3] + new Vector3(0, 0, 1.12));
                    return;
                case 27:
                    NAPI.Entity.SetEntityPosition(player, fbiCheckpoints[2] + new Vector3(0, 0, 1.12));
                    return;
                case 46:
                    if (Main.Players[player].FractionID != 9)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник FBI", 3000);
                        return;
                    }
                    if (!player.GetData("ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (player.GetData("IN_CP_MODE"))
                    {
                        Manager.setSkin(player, Main.Players[player].FractionID, Main.Players[player].FractionLVL);
                        player.SetData("IN_CP_MODE", false);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы переоделись в рабочую форму", 3000);
                    }
                    else
                    {
                        if (!warg_mode)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не включен режим ЧП", 3000);
                            return;
                        }
                        if (Main.Players[player].Gender)
                        {
                            Customization.SetHat(player, 39, 2);
                            player.SetClothes(11, 53, 1);
                            player.SetClothes(4, 31, 2);
                            player.SetClothes(6, 25, 0);
                            player.SetClothes(9, 28, 9);
                            player.SetClothes(8, 130, 0);
                            player.SetClothes(3, 49, 0);
                        }
                        else
                        {
                            Customization.SetHat(player, 38, 2);
                            player.SetClothes(11, 46, 1);
                            player.SetClothes(4, 30, 2);
                            player.SetClothes(6, 25, 0);
                            player.SetClothes(9, 31, 9);
                            player.SetClothes(8, 160, 0);
                            player.SetClothes(3, 49, 0);
                        }
                        player.SetData("IN_CP_MODE", true);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы переоделись в спец. форму", 3000);
                    }
                    return;
                case 61:
                    if (Main.Players[player].FractionID != 9)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[9].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 9);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[9].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        private void fbiShape_onEntityEnterColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData("INTERACT"));
            }
            catch (Exception ex) { Log.Write("fbiShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void fbiShape_onEntityExitColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("fbiShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        #region menus
        public static void OpenFbiLiftMenu(Client player)
        {
            Trigger.ClientEvent(player, "openlift", 0, "fbilift");
        }
        [RemoteEvent("fbilift")]
        public static void callback_fbilift(Client client, int floor)
        {
            try
            {
                if (client.IsInVehicle) return;
                if (client.HasData("FOLLOWING"))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }
                switch (floor)
                {
                    case 0: //garage
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[7] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[7] + new Vector3(0, 0, 1.12));
                        return;
                    case 1: //floor1
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[6] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[6] + new Vector3(0, 0, 1.12));
                        return;
                    case 2: //floor49
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[1] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[1] + new Vector3(0, 0, 1.12));
                        return;
                    case 3: //roof
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[4] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[4] + new Vector3(0, 0, 1.12));
                        return;
                }
            }
            catch (Exception e) { Log.Write("fbilift: " + e.Message, nLog.Type.Error); }
        }

        public static void OpenFbiGunMenu(Client player)
        {
            Trigger.ClientEvent(player, "fbiguns");
        }
        [RemoteEvent("fbigun")]
        public static void callback_fbiguns(Client client, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Fractions.Manager.giveGun(client, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 1:
                        Fractions.Manager.giveGun(client, Weapons.Hash.CombatPistol, "CombatPistol");
                        return;
                    case 2:
                        var minrank = (warg_mode) ? 2 : 6;
                        Fractions.Manager.giveGun(client, Weapons.Hash.CombatPDW, "CombatPDW");
                        return;
                    case 3:
                        minrank = (warg_mode) ? 2 : 5;
                        Fractions.Manager.giveGun(client, Weapons.Hash.CarbineRifle, "CarbineRifle");
                        return;
                    case 4:
                        minrank = (warg_mode) ? 2 : 9;
                        Fractions.Manager.giveGun(client, Weapons.Hash.HeavySniper, "HeavySniper");
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(client, "armor")) return;
                        if (Fractions.Stocks.fracStocks[9].Materials < Fractions.Manager.matsForArmor)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var aItem = nInventory.Find(Main.Players[client].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= Fractions.Manager.matsForArmor;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "armor", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 6: // medkit
                        if (!Manager.canGetWeapon(client, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[9].Medkits == 0)
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
                        Fractions.Stocks.fracStocks[9].Medkits--;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 7:
                        Fractions.Manager.giveAmmo(client, ItemType.PistolAmmo, 12);
                        return;
                    case 8:
                        minrank = (warg_mode) ? 2 : 6;
                        Fractions.Manager.giveAmmo(client, ItemType.SMGAmmo, 30);
                        return;
                    case 9:
                        minrank = (warg_mode) ? 2 : 5;
                        Fractions.Manager.giveAmmo(client, ItemType.RiflesAmmo, 30);
                        return;
                    case 10:
                        minrank = (warg_mode) ? 2 : 9;
                        Fractions.Manager.giveAmmo(client, ItemType.SniperAmmo, 5);
                        return;
                    case 11:
                        var data = (Main.Players[client].Gender) ? "128_0_true" : "98_0_false";
                        if (nInventory.Items[Main.Players[client].UUID].FirstOrDefault(i => i.Type == ItemType.Jewelry && i.Data == data) != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть бейдж", 3000);
                            return;
                        }

                        var tryAdd = nInventory.TryAdd(client, new nItem(ItemType.Jewelry));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }

                        nInventory.Add(client, new nItem(ItemType.Jewelry, 1, data));
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бейдж FIB", 3000);
                        return;
                }
            }
            catch (Exception e) { Log.Write("Fbigun: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}
