using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.MoneySystem;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NeptuneEvo.GUI
{
    class Dashboard : Script
    {
        public static void Event_OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (!isopen.ContainsKey(player)) return;
                isopen.Remove(player);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        private static nLog Log = new nLog("Dashboard");
        public static Dictionary<Client, bool> isopen = new Dictionary<Client, bool>();
        private static Dictionary<int, string> Status = new Dictionary<int, string>
        {// Group id, Status
            {0, "Игрок" },
            {16, "Администратор" }
        };

        [RemoteEvent("Inventory")]
        public void ClientEvent_Inventory(Client player, params object[] arguments)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (arguments.Length < 3) return;
                int type = Convert.ToInt32(arguments[0]);
                int index = Convert.ToInt32(arguments[1]);
                string data = Convert.ToString(arguments[2]);
                Log.Debug($"Type: {type} | Index: {index} | Data: {data}");
                Core.Character.Character acc = Main.Players[player];
                List<nItem> items;
                nItem item;
                switch (type)
                {
                    case 0:
                        {// self inventory
                            items = nInventory.Items[acc.UUID];
                            item = items[index];
                            if (data == "drop")
                            {//remove one item from player inventory
                                if(item.Type == ItemType.GasCan)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Возможность выкладывать канистры временно отключена", 3000);
                                    return;
                                }
                                else if (item.Type == ItemType.BagWithDrill)
                                {
                                    SafeMain.dropDrillBag(player);
                                    return;
                                }
                                else if (item.Type == ItemType.BagWithMoney)
                                {
                                    SafeMain.dropMoneyBag(player);
                                    return;
                                }
                                else if (nInventory.ClothesItems.Contains(item.Type))
                                {
                                    if (item.IsActive)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны сначала снять эту одежду", 3000);
                                        return;
                                    }
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data), null);
                                    sendItems(player);
                                    return;
                                }
                                else if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun)
                                {
                                    if (item.IsActive)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны убрать оружие из рук", 3000);
                                        return;
                                    }
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data), null);
                                    sendItems(player);
                                    return;
                                }
                                else if (item.Type == ItemType.CarKey)
                                {
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data), null);
                                    sendItems(player);
                                    return;
                                }
                                if(player.IsInVehicle) {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя выбрасывать вещи, находясь в машине", 3000);
                                    return;
                                }
                                if (item.Count > 1)
                                {
                                    Close(player);
                                    player.SetData("ITEMTYPE", item.Type);
                                    player.SetData("ITEMINDEX", index);
                                    Trigger.ClientEvent(player, "openInput", "Выбросить предмет", "Введите количество", 3, "item_drop");
                                    return;
                                }
                                nInventory.Remove(player, item.Type, 1);
                                Items.onDrop(player, new nItem(item.Type, 1, item.Data), null);
                            }
                            else if (data == "use")
                            {
                                try
                                {
                                    Log.Debug($"ItemID: {item.ID} | ItemType: {item.Type} | ItemData: {item.Data} | ItemName: {nInventory.ItemsNames[(int)item.Type]}");
                                    if (player.HasData("CHANGE_WITH")) {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Чтобы использовать вещи, нужно закрыть обмен вещами", 3000);
                                        return;
                                    }
                                    Items.onUse(player, item, index);
                                    return;
                                }
                                catch (Exception e)
                                {
                                    Log.Write(e.ToString(), nLog.Type.Error);
                                }
                            }
                            else if (data == "transfer")
                            {
                                if (!player.HasData("OPENOUT_TYPE")) return;
                                if (nInventory.ClothesItems.Contains(item.Type) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны сначала снять эту одежду", 3000);
                                    return;
                                }
                                else if ((nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type)) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны убрать оружие из рук", 3000);
                                    return;
                                }
                                switch (player.GetData("OPENOUT_TYPE"))
                                {
                                    case 1:
                                        return;
                                    case 2:
                                        {
                                            Vehicle veh = player.GetData("SELECTEDVEH");
                                            if (veh is null) return;
                                            if (veh.Dimension != player.Dimension)
                                            {
                                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                                return;
                                            }
                                            if (veh.Position.DistanceTo(player.Position) > 10f)
                                            {
                                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                                return;
                                            }

                                            int tryAdd = VehicleInventory.TryAdd(veh, new nItem(item.Type, item.Count));
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В машине недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.BagWithDrill)
                                            {
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HEIST_DRILL");
                                            }
                                            else if (item.Type == ItemType.BagWithMoney)
                                            {
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HAND_MONEY");
                                            }

                                            if (item.Count > 1)
                                            {
                                                Close(player);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.ClientEvent(player, "openInput", "Переложить предмет", "Введите количество", 3, "item_transfer_toveh");
                                                return;
                                            }
                                            if (item.Type == ItemType.Material)
                                            {
                                                int maxMats = (Fractions.Stocks.maxMats.ContainsKey(veh.DisplayName)) ? Fractions.Stocks.maxMats[veh.DisplayName] : 600;
                                                if (VehicleInventory.GetCountOfType(veh, ItemType.Material) + 1 > maxMats)
                                                {
                                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                                                    return;
                                                }
                                            }

                                            VehicleInventory.Add(veh, new nItem(item.Type, 1, item.Data));
                                            nInventory.Remove(player, item);
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"vehicle({veh.NumberPlate})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            return;
                                        }
                                    case 3:
                                        {
                                            if (item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type))
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта вещь не предназначена для этого шкафа", 3000);
                                                return;
                                            }
                                            if (Main.Players[player].InsideHouseID == -1) return;
                                            int houseID = Main.Players[player].InsideHouseID;
                                            int furnID = player.GetData("OpennedSafe");
                                            if (item.Count > 1)
                                            {
                                                Close(player);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.ClientEvent(player, "openInput", "Переложить предмет", "Введите количество", 3, "item_transfer_tosafe");
                                                return;
                                            }

                                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"itemSafe({furnID} | house: {houseID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Remove(player, item.Type, 1);
                                            sendItems(player);
                                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type));
                                            return;
                                        }
                                    case 4:
                                        {
                                            if (!nInventory.ClothesItems.Contains(item.Type))
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Шкаф для одежды может хранить только одежду", 3000);
                                                return;
                                            }
                                            if (Main.Players[player].InsideHouseID == -1) return;
                                            int houseID = Main.Players[player].InsideHouseID;
                                            int furnID = player.GetData("OpennedSafe");

                                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"clothSafe({furnID} | house: {houseID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Items[acc.UUID].Remove(item);
                                            sendItems(player);
                                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type, 1, item.Data));
                                            return;
                                        }
                                    case 5:
                                        {
                                            if (!player.HasData("CHANGE_WITH"))
                                            {
                                                Close(player);
                                                return;
                                            }
                                            Client target = player.GetData("CHANGE_WITH");
                                            if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
                                            {
                                                Close(player);
                                                return;
                                            }

                                            int tryAdd = nInventory.TryAdd(target, new nItem(item.Type, 1));
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У игрока недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[target].UUID, ItemType.BodyArmor) != null)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.BagWithDrill)
                                            {
                                                if (target.HasData("HEIST_DRILL") || target.HasData("HAND_MONEY"))
                                                {
                                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть дрель или деньги в руках", 3000);
                                                    return;
                                                }

                                                target.SetClothes(5, 41, 0);
                                                target.SetData("HEIST_DRILL", true);
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HEIST_DRILL");
                                            }
                                            else if (item.Type == ItemType.BagWithMoney)
                                            {
                                                if (target.HasData("HEIST_DRILL") || target.HasData("HAND_MONEY"))
                                                {
                                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть сумка", 3000);
                                                    return;
                                                }

                                                target.SetClothes(5, 45, 0);
                                                target.SetData("HAND_MONEY", true);
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HAND_MONEY");
                                            }

                                            if (item.Count > 1)
                                            {
                                                Close(player, true);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.ClientEvent(player, "openInput", "Передать предмет", "Введите количество", 3, "item_transfer_toplayer");
                                                return;
                                            }

                                            nInventory.Add(target, item);
                                            nInventory.Remove(player, item);
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            return;
                                        }
                                    case 6:
                                        {
                                            if (!nInventory.WeaponsItems.Contains(item.Type) && !nInventory.AmmoItems.Contains(item.Type)) return;
                                            int onFraction = player.GetData("ONFRACSTOCK");

                                            if (onFraction == 0) return;

                                            if (Fractions.Stocks.TryAdd(onFraction, new nItem(item.Type, 1)) != 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На складе недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Count > 1)
                                            {
                                                Close(player, true);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.ClientEvent(player, "openInput", "Передать предмет", "Введите количество", 3, "item_transfer_tofracstock");
                                                return;
                                            }

                                            string serial = (nInventory.WeaponsItems.Contains(item.Type)) ? $"({(string)item.Data})" : "";
                                            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}{serial}", 1, false);
                                            Fractions.Stocks.Add(onFraction, item);
                                            nInventory.Remove(player, item);
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"fracstock({onFraction})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            return;
                                        }
                                    case 7:
                                        {
                                            nItem keyring = nInventory.Items[Main.Players[player].UUID][player.GetData("KEYRING")];
                                            string keysData = Convert.ToString(keyring.Data);
                                            List<string> keys = (keysData.Length == 0) ? new List<string>() : new List<string>(keysData.Split('/'));
                                            if (keys.Count > 0 && string.IsNullOrEmpty(keys[keys.Count - 1]))
                                                keys.RemoveAt(keys.Count - 1);

                                            if (keys.Count >= 5)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Максимум 5 ключей", 3000);
                                                return;
                                            }
                                            
                                            if (item.Type != ItemType.CarKey)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Применимо только для ключей", 3000);
                                                return;
                                            }

                                            keys.Add(item.Data);
                                            keysData = "";
                                            foreach (string key in keys)
                                                keysData += $"{key}/";
                                            keyring.Data = keysData; // ¯\_(ツ)_/¯
                                            nInventory.Items[Main.Players[player].UUID][player.GetData("KEYRING")] = keyring;

                                            nInventory.Remove(player, item);

                                            List<nItem> keyringItems = new List<nItem>();
                                            foreach (string key in keys)
                                                keyringItems.Add(new nItem(ItemType.CarKey, 1, key));

                                            player.SetData("KEYRING", nInventory.Items[Main.Players[player].UUID].IndexOf(keyring));
                                            OpenOut(player, keyringItems, "Связка ключей", 7);
                                            return;
                                        }
                                    case 8: // Оружейный сейф
                                        {
                                            if (!nInventory.WeaponsItems.Contains(item.Type) && !nInventory.MeleeWeaponsItems.Contains(item.Type))
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Оружейный сейф может хранить только оружие", 3000);
                                                return;
                                            }
                                            if (Main.Players[player].InsideHouseID == -1) return;
                                            int houseID = Main.Players[player].InsideHouseID;
                                            int furnID = player.GetData("OpennedSafe");

                                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"weapSafe({furnID} | house: {houseID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Items[acc.UUID].Remove(item);
                                            sendItems(player);
                                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type, 1, item.Data));
                                            return;
                                        }
                                }
                                Items.onTransfer(player, item, null);
                                Close(player);
                                return;
                            }
                            break;
                        }
                    case 1:
                        { // droped items
                          //TODO
                            break;
                        }
                    case 2:
                        { // in car items
                            Vehicle veh = player.GetData("SELECTEDVEH");
                            if (veh is null) return;
                            if (veh.Dimension != player.Dimension)
                            {
                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                return;
                            }
                            if (veh.Position.DistanceTo(player.Position) > 10f)
                            {
                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                return;
                            }
                            items = veh.GetData("ITEMS");
                            item = items[index];

                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BagWithDrill)
                            {
                                if (player.HasData("HEIST_DRILL") || player.HasData("HAND_MONEY"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть дрель или деньги в руках", 3000);
                                    return;
                                }

                                player.SetClothes(5, 41, 0);
                                player.SetData("HEIST_DRILL", true);
                            }
                            else if (item.Type == ItemType.BagWithMoney)
                            {
                                if (player.HasData("HEIST_DRILL") || NAPI.Data.HasEntityData(player, "HAND_MONEY"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть сумка", 3000);
                                    return;
                                }

                                player.SetClothes(5, 45, 0);
                                player.SetData("HAND_MONEY", true);
                            }

                            if (item.Count > 1)
                            {
                                Close(player);
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.ClientEvent(player, "openInput", "Взять предмет", "Введите количество", 3, "item_transfer_fromveh");
                                return;
                            }

                            VehicleInventory.Remove(veh, item);
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data));
                            GameLog.Items($"vehicle({veh.NumberPlate})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            break;
                        }
                    case 3: // Взять
                        {
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];
                            items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            item = items[index];

                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            if (item.Count > 1)
                            {
                                Close(player);
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.ClientEvent(player, "openInput", "Взять предмет", "Введите количество", 3, "item_transfer_fromsafe");
                                return;
                            }
                            GameLog.Items($"itemSafe({furnID} | house: {houseID})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            items.RemoveAt(index);
                            Houses.FurnitureManager.FurnituresItems[houseID][furnID] = items;
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data));
                            sendItems(player);
                            foreach (Client p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if ((p.HasData("OPENOUT_TYPE") && p.GetData("OPENOUT_TYPE") == 3) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData("OpennedSafe") == furnID))
                                    GUI.Dashboard.OpenOut(p, items, furniture.Name, 3);
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];
                            items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            item = items[index];

                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            GameLog.Items($"clothSafe({furnID} | house: {houseID})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            nInventory.Items[Main.Players[player].UUID].Add(item);
                            sendItems(player);

                            items.RemoveAt(index);
                            Houses.FurnitureManager.FurnituresItems[houseID][furnID] = items;
                            foreach (Client p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if ((p.HasData("OPENOUT_TYPE") && p.GetData("OPENOUT_TYPE") == 4) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData("OpennedSafe") == furnID))
                                    GUI.Dashboard.OpenOut(p, items, furniture.Name, 4);
                            }
                            break;
                        }
                    case 6:
                        {
                            if (!player.HasData("ONFRACSTOCK") || player.GetData("ONFRACSTOCK") == 0) return;
                            int onFrac = player.GetData("ONFRACSTOCK");
                            items = Fractions.Stocks.fracStocks[onFrac].Weapons;
                            item = items[index];

                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, 1));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Count > 1)
                            {
                                Close(player);
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.ClientEvent(player, "openInput", "Взять предмет", "Введите количество", 3, "item_transfer_fromfracstock");
                                return;
                            }

                            nInventory.Add(player, item);
                            Fractions.Stocks.Remove(onFrac, item);
                            string serial = (nInventory.WeaponsItems.Contains(item.Type)) ? $"({(string)item.Data})" : "";
                            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}{serial}", 1, true);
                            GameLog.Items($"fracstock({onFrac})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            break;
                        }
                    case 7:
                        { // keyring items
                            nItem keyring = nInventory.Items[Main.Players[player].UUID][player.GetData("KEYRING")];
                            string keysData = Convert.ToString(keyring.Data);
                            List<string> keys = (keysData.Length == 0) ? new List<string>() : new List<string>(keysData.Split('/'));
                            if (keys.Count > 0 && keys[keys.Count - 1] == "")
                                keys.RemoveAt(keys.Count - 1);

                            item = new nItem(ItemType.CarKey, 1, keys[index]);
                            int tryAdd = nInventory.TryAdd(player, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно места", 3000);
                                return;
                            }

                            keys.RemoveAt(index);
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data));

                            keysData = "";
                            foreach (string key in keys)
                                keysData += $"{key}/";
                            keyring.Data = keysData; // ¯\_(ツ)_/¯
                            nInventory.Items[Main.Players[player].UUID][player.GetData("KEYRING")] = keyring;

                            List<nItem> keyringItems = new List<nItem>();
                            foreach (string key in keys)
                                keyringItems.Add(new nItem(ItemType.CarKey, 1, key));
                            OpenOut(player, keyringItems, "Связка ключей", 7);
                            break;
                        }
                    case 8: // Взять
                        {
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];
                            items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            item = items[index];

                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            GameLog.Items($"weapSafe({furnID} | house: {houseID})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            nInventory.Items[Main.Players[player].UUID].Add(item);
                            sendItems(player);

                            items.RemoveAt(index);
                            Houses.FurnitureManager.FurnituresItems[houseID][furnID] = items;
                            foreach (Client p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if ((p.HasData("OPENOUT_TYPE") && p.GetData("OPENOUT_TYPE") == 8) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData("OpennedSafe") == furnID))
                                    GUI.Dashboard.OpenOut(p, items, furniture.Name, 8);
                            }
                            break;
                        }
                    case 20: 
                        {
                            if(Main.Players[player].AdminLVL >= 6 && Main.Players[player].InsideHouseID == -1) {
                                if (!player.HasData("CHANGE_WITH")) {
                                    Close(player);
                                    return;
                                }
                                Client target = player.GetData("CHANGE_WITH");
                                if (!Main.Players.ContainsKey(target))
                                {
                                    Close(player);
                                    return;
                                }
                                items = nInventory.Items[Main.Players[target].UUID];
                                item = items[index];
                                if (nInventory.ClothesItems.Contains(item.Type) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок должен снять эту одежду", 3000);
                                    return;
                                }
                                else if ((nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type)) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок должен убрать это оружие из рук", 3000);
                                    return;
                                }
                                int tryAdd1 = nInventory.TryAdd(player, new nItem(item.Type, 1));
                                if (tryAdd1 == -1 || tryAdd1 > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно места", 3000);
                                    return;
                                }
                                if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                    return;
                                }
                                if (item.Count > 1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такие вещи нельзя забрать", 3000);
                                    return;
                                }
                                nInventory.Add(player, item);
                                nInventory.Remove(target, item);
                                Close(player);
                                Commands.CMD_showPlayerStats(player, target.Value); // reopen target inventory
                                GameLog.Admin(player.Name, $"takeItem({item.Type} | {item.Data})", target.Name);
                                return;
                            }
                            break;
                        }
                }
            }
            catch (Exception e) { Log.Write("Inventory: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("openInventory")]
        public void ClientEvent_openInventory(Client player, params object[] arguments)
        {
            try
            {
                if (isopen[player])
                {
                    Close(player);

                }
                else
                    Open(player);
            }
            catch (Exception e) { Log.Write("openInventory: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("closeInventory")]
        public void ClientEvent_closeInventory(Client player, params object[] arguments)
        {
            try
            {
                if (player.HasData("OPENOUT_TYPE") && player.GetData("OPENOUT_TYPE") == 20) sendItems(player);

                if(player.HasData("SELECTEDVEH")) {
                    Vehicle vehicle = player.GetData("SELECTEDVEH");
                    vehicle.SetData("BAGINUSE", false);
                }

                player.ResetData("OPENOUT_TYPE");

                if (player.HasData("CHANGE_WITH") && Main.Players.ContainsKey(player.GetData("CHANGE_WITH")))
                {
                    Close(player.GetData("CHANGE_WITH"));
                    NAPI.Data.ResetEntityData(player.GetData("CHANGE_WITH"), "CHANGE_WITH");
                    player.ResetData("CHANGE_WITH");
                    if(Main.Players[player].AdminLVL != 0) sendStats(player);
                }
            }
            catch (Exception e) { Log.Write($"CloseInventory: " + e.Message, nLog.Type.Error); }
        }
        
        public static void Close(Client player, bool resetOpenOut = false)
        {
            int data = (resetOpenOut) ? 11 : 1;
            Trigger.ClientEvent(player, "board", data);
            player.ResetData("OPENOUT_TYPE");
        }
        public static void sendStats(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Core.Character.Character acc = Main.Players[player];

                string status =
                    (acc.AdminLVL >= 1) ? "Администратор" :
                    (Main.Accounts[player].VipLvl > 0) ? $"{Group.GroupNames[Main.Accounts[player].VipLvl]} до {Main.Accounts[player].VipDate.ToString("dd.MM.yyyy")}" :
                    $"{Group.GroupNames[Main.Accounts[player].VipLvl]}";

                long bank = (acc.Bank != 0) ? Bank.Accounts[acc.Bank].Balance : 0;
                
                string lic = "";
                for (int i = 0; i < acc.Licenses.Count; i++)
                    if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                if (lic == "") lic = "Отсутствуют";

                string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID - 1] : "Безработный";
                string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";

                string number = (acc.Sim == -1) ? "Нет сим-карты" : Main.Players[player].Sim.ToString();

                List<object> data = new List<object>
                {
                    acc.LVL,
                    $"{acc.EXP}/{3 + acc.LVL * 3}",
                    number,
                    status,
                    0,
                    acc.Warns,
                    lic,
                    acc.CreateDate.ToString("dd.MM.yyyy"),
                    acc.UUID,
                    acc.Bank,
                    work,
                    fraction,
                    acc.FractionLVL,
                };

                string json = JsonConvert.SerializeObject(data);
                Log.Debug("data is: " + json.ToString());
                Trigger.ClientEvent(player, "board", 2, json);

                data.Clear();

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDSTATS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static async Task SendStatsAsync(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Core.Character.Character acc = Main.Players[player];

                string status =
                    (acc.AdminLVL >= 1) ? "Администратор" :
                    (Main.Accounts[player].VipLvl > 0) ? $"{Group.GroupNames[Main.Accounts[player].VipLvl]} до {Main.Accounts[player].VipDate.ToString("dd.MM.yyyy")}" :
                    $"{Group.GroupNames[Main.Accounts[player].VipLvl]}";

                long bank = (acc.Bank != 0) ? Bank.Accounts[acc.Bank].Balance : 0;

                string lic = "";
                for (int i = 0; i < acc.Licenses.Count; i++)
                    if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                if (lic == "") lic = "Отсутствуют";

                string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID - 1] : "Безработный";
                string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";

                string number = (acc.Sim == -1) ? "Нет сим-карты" : Main.Players[player].Sim.ToString();

                List<object> data = new List<object>
                {
                    acc.LVL,
                    $"{acc.EXP}/{3 + acc.LVL * 3}",
                    number,
                    status,
                    0,
                    acc.Warns,
                    lic,
                    acc.CreateDate.ToString("dd.MM.yyyy"),
                    acc.UUID,
                    acc.Bank,
                    work,
                    fraction,
                    acc.FractionLVL,
                };

                string json = JsonConvert.SerializeObject(data);
                Log.Debug("data is: " + json.ToString());
                Trigger.ClientEvent(player, "board", 2, json);

                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDSTATS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void sendItems(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                int UUID = Main.Players[player].UUID;

                if (!nInventory.Items.ContainsKey(UUID)) return;
                List<nItem> items = new List<nItem>(nInventory.Items[UUID]);

                List<object> data = new List<object>();
                foreach (nItem item in items)
                {
                    List<object> idata = new List<object>
                    {
                        item.ID,
                        item.Count,
                        (item.IsActive) ? 1 : 0,
                        (nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun) ? "Serial: " + item.Data : (item.Type == ItemType.CarKey) ? $"{(string)item.Data.Split('_')[0]}" : ""
                    };
                    data.Add(idata);
                }

                string json = JsonConvert.SerializeObject(data);
                Log.Debug(json);
                Trigger.ClientEvent(player, "board", 3, json);

                items.Clear();
                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDITEMS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static async Task SendItemsAsync(Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                int UUID = Main.Players[player].UUID;

                if (!nInventory.Items.ContainsKey(UUID)) return;
                List<nItem> items = new List<nItem>(nInventory.Items[UUID]);

                List<object> data = new List<object>();
                foreach (nItem item in items)
                {
                    List<object> idata = new List<object>
                    {
                        item.ID,
                        item.Count,
                        (item.IsActive) ? 1 : 0,
                        (nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun) ? "Serial: " + item.Data : (item.Type == ItemType.CarKey) ? $"{(string)item.Data.Split('_')[0]}" : ""
                    };
                    data.Add(idata);
                }

                string json = JsonConvert.SerializeObject(data);
                await Log.DebugAsync(json);
                NAPI.Task.Run(() => Trigger.ClientEvent(player, "board", 3, json));

                items.Clear();
                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDITEMS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void Open(Client client)
        {
            Trigger.ClientEvent(client, "board", 0);
        }
        public static void OpenOut(Client client, List<nItem> items, string title, int type = 1)
        {
            try
            {
                if (type == 0) return;
                List<object> data = new List<object>();
                data.Add(type);
                data.Add(title);
                List<object> Items = new List<object>();
                foreach (nItem item in items)
                {
                    List<object> idata = new List<object>
                    {
                        item.ID,
                        item.Count,
                        (item.IsActive) ? 1 : 0,
                        (nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun) ? "Serial: " + item.Data : (item.Type == ItemType.CarKey) ? $"{item.Data}" : ""
                    };
                    Items.Add(idata);
                }
                data.Add(Items);

                string json = JsonConvert.SerializeObject(data);
                Log.Debug(json);
                client.SetData("OPENOUT_TYPE", type);
                Trigger.ClientEvent(client, "board", 4, json);
                Trigger.ClientEvent(client, "board", 5, true);
                Trigger.ClientEvent(client, "board", 0);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_OPENOUT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void Update(Client client, nItem item, int index)
        {
            List<object> idata = new List<object>
                    {
                        item.ID,
                        item.Count,
                        (item.IsActive) ? 1 : 0,
                        (nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun) ? "Serial: " + item.Data : (item.Type == ItemType.CarKey) ? $"{(string)item.Data.Split('_')[0]}" : ""
                    };
            string json = JsonConvert.SerializeObject(idata);
            Trigger.ClientEvent(client, "board", 6, json, index);
        }
        public static async Task UpdateAsync(Client client, nItem item, int index)
        {
            try
            {
                List<object> idata = new List<object>
                    {
                        item.ID,
                        item.Count,
                        (item.IsActive) ? 1 : 0,
                        (nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun) ? "Serial: " + item.Data : (item.Type == ItemType.CarKey) ? $"{(string)item.Data.Split('_')[0]}" : ""
                    };
                string json = JsonConvert.SerializeObject(idata);
                NAPI.Task.Run(() => Trigger.ClientEvent(client, "board", 6, json, index));
            }
            catch (Exception e) { Log.Write("UpdateAsync: " + e.Message); }
        }
    }
}
