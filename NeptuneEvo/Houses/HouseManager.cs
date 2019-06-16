using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEvo.Core;
using Redage.SDK;
using System.Linq;
using System.Data;
using NeptuneEvo.GUI;

namespace NeptuneEvo.Houses
{
    #region HouseType Class
    public class HouseType
    {
        public string Name { get; }
        public Vector3 Position { get; }
        public string IPL { get; set; }
        public Vector3 PetPosition { get; }
        public float PetRotation { get; }

        public HouseType(string name, Vector3 position, Vector3 petpos, float rotation, string ipl = "")
        {
            Name = name;
            Position = position;
            IPL = ipl;
            PetPosition = petpos;
            PetRotation = rotation;
        }

        public void Create()
        {
            if (IPL != "") NAPI.World.RequestIpl(IPL);
        }
    }
    #endregion

    #region House Class
    class House
    {
        public int ID { get; }
        public string Owner { get; private set; }
        public int Type { get; private set; }
        public Vector3 Position { get; }
        public int Price { get; set; }
        public bool Locked { get; private set; }
        public int GarageID { get; set; }
        public int BankID { get; set; }
        public List<string> Roommates { get; set; } = new List<string>();
        [JsonIgnore] public int Dimension { get; set; }

        [JsonIgnore]
        public Blip blip;
        [JsonIgnore]
        public string PetName;
        [JsonIgnore]
        private TextLabel label;
        [JsonIgnore]
        private ColShape shape;

        [JsonIgnore]
        private ColShape intshape;
        [JsonIgnore]
        private Marker intmarker;

        [JsonIgnore]
        private List<GTANetworkAPI.Object> Objects = new List<GTANetworkAPI.Object>();

        [JsonIgnore]
        private List<NetHandle> PlayersInside = new List<NetHandle>();

        public House(int id, string owner, int type, Vector3 position, int price, bool locked, int garageID, int bank, List<string> roommates)
        {
            ID = id;
            Owner = owner;
            Type = type;
            Position = position;
            Price = price;
            Locked = locked;
            GarageID = garageID;
            BankID = bank;
            Roommates = roommates;

            #region Creating Blip
            blip = NAPI.Blip.CreateBlip(Position);
            if (string.IsNullOrEmpty(Owner))
            {
                blip.Sprite = 374;
                blip.Color = 52;
            }
            else
            {
                blip.Sprite = 40;
                blip.Color = 49;
            }

            blip.Scale = 0.6f;
            blip.ShortRange = true;
            #endregion

            #region Creating Marker & Colshape
            shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
            shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "HOUSEID", id);
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 6);
                    Jobs.Gopostal.GoPostal_onEntityEnterColShape(s, ent);
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
            };
            shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    NAPI.Data.ResetEntityData(ent, "HOUSEID");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
            };
            #endregion

            label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"House {id}"), position + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            UpdateLabel();
        }
        public void UpdateLabel()
        {
            try
            {
                string text = $"Style: ~g~{HouseManager.HouseTypeList[Type].Name}\n";
                if (!string.IsNullOrEmpty(Owner)) text += $"~w~Owner: ~g~{Owner.Replace('_', ' ')}\n";
                else text += $"~w~Sell for ~g~{Price}$\n";
                if (GarageID != 0) text += $"~w~Garage space: ~g~{GarageManager.GarageTypes[GarageManager.Garages[GarageID].Type].MaxCars}\n";
                text += (Locked) ? "~r~Closed\n" : "~g~Opened\n";
                text += $"~w~ID: ~g~{ID}";
                label.Text = Main.StringToU16(text);

            } catch(Exception e)
            {
                blip.Color = 48;
                Console.WriteLine(ID.ToString() + e.ToString());
            }
        }
        public void CreateAllFurnitures()
        {
            if (FurnitureManager.HouseFurnitures.ContainsKey(ID)) {
                if (FurnitureManager.HouseFurnitures[ID].Count >= 1) {
                    foreach (var f in FurnitureManager.HouseFurnitures[ID].Values) if (f.IsSet) CreateFurniture(f);
                }
            }
        }
        public void CreateFurniture(HouseFurniture f)
        {
            try {
                var obj = f.Create((uint)Dimension);
                NAPI.Data.SetEntityData(obj, "HOUSE", ID);
                NAPI.Data.SetEntityData(obj, "ID", f.ID);
                NAPI.Entity.SetEntityDimension(obj, (uint)Dimension);
                if (f.Name == "Оружейный сейф") NAPI.Data.SetEntitySharedData(obj, "TYPE", "WeaponSafe");
                else if (f.Name == "Шкаф с одеждой") NAPI.Data.SetEntitySharedData(obj, "TYPE", "ClothesSafe");
                else if (f.Name == "Шкаф с предметами") NAPI.Data.SetEntitySharedData(obj, "TYPE", "SubjectSafe");
                Objects.Add(obj);
            } catch {
            }
        }
        public void DestroyFurnitures()
        {
            try
            {
                foreach (var obj in Objects) NAPI.Entity.DeleteEntity(obj);
                Objects = new List<GTANetworkAPI.Object>();
            }
            catch { }
        }
        public void DestroyFurniture(int id)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var obj in Objects)
                    {
                        if (obj.HasData("ID") && obj.GetData("ID") == id)
                        {
                            NAPI.Entity.DeleteEntity(obj);
                            //Log.Debug("HOUSEFURNITURE: deleted " + id);
                            break;
                        }
                    }
                }
                catch { }
            });
        }
        
        public void UpdateBlip()
        {
            if (string.IsNullOrEmpty(Owner))
            {
                blip.Sprite = 374;
                blip.Color = 52;
            }
            else
            {
                blip.Sprite = 40;
                blip.Color = 49;
            }
        }
        public void Create()
        {
            MySQL.Query($"INSERT INTO `houses`(`id`,`owner`,`type`,`position`,`price`,`locked`,`garage`,`bank`,`roommates`) " +
                $"VALUES ('{ID}','{Owner}',{Type},'{JsonConvert.SerializeObject(Position)}',{Price},{Locked},{GarageID},{BankID},'{JsonConvert.SerializeObject(Roommates)}')");
        }
        public void Save()
        {
            MoneySystem.Bank.Save(BankID);
            MySQL.Query($"UPDATE `houses` SET `owner`='{Owner}',`type`={Type},`position`='{JsonConvert.SerializeObject(Position)}',`price`={Price}," +
                $"`locked`={Locked},`garage`={GarageID},`bank`={BankID},`roommates`='{JsonConvert.SerializeObject(Roommates)}' WHERE `id`='{ID}'");
        }
        public void Destroy()
        {
            RemoveAllPlayers();
            blip.Delete();
            NAPI.ColShape.DeleteColShape(shape);
            NAPI.ColShape.DeleteColShape(intshape);
            label.Delete();
            intmarker.Delete();
            DestroyFurnitures();
        }
        public void SetLock(bool locked)
        {
            Locked = locked;

            UpdateLabel();
            Save();
        }
        public void SetOwner(Client player)
        {
            GarageManager.Garages[GarageID].DestroyCars();
            Owner = (player == null) ? string.Empty : player.Name;
            UpdateBlip();
            UpdateLabel();
            if (player != null)
            {
                Trigger.ClientEvent(player, "changeBlipColor", blip, 73);
                Trigger.ClientEvent(player, "createCheckpoint", 333, 1, GarageManager.Garages[GarageID].Position - new Vector3(0, 0, 1.12), 1, NAPI.GlobalDimension, 220, 220, 0);
                Trigger.ClientEvent(player, "createGarageBlip", GarageManager.Garages[GarageID].Position);
                Hotel.MoveOutPlayer(player);

                var vehicles = VehicleManager.getAllPlayerVehicles(Owner);
                if (GarageManager.Garages[GarageID].Type != -1)
                    NAPI.Task.Run(() => { try { GarageManager.Garages[GarageID].SpawnCars(vehicles); } catch { } });
            }

            foreach (var r in Roommates)
            {
                var roommate = NAPI.Player.GetPlayerFromName(r);
                if (roommate != null)
                {
                    Notify.Send(roommate, NotifyType.Warning, NotifyPosition.BottomCenter, "Вы были выселены из дома", 3000);
                    roommate.TriggerEvent("deleteCheckpoint", 333);
                    roommate.TriggerEvent("deleteGarageBlip");
                }
            }

            Roommates = new List<string>();
            Save();
        }
        public string GaragePlayerExit(Client player)
        {
            var players = Main.Players.Keys.ToList();
            var online = players.FindAll(p => Roommates.Contains(p.Name) && p.Name != player.Name);

            var owner = NAPI.Player.GetPlayerFromName(Owner);
            if (Roommates.Contains(player.Name) && owner != null && Main.Players.ContainsKey(owner))
                online.Add(owner);

            var garage = GarageManager.Garages[GarageID];
            var number = garage.SendVehiclesInsteadNearest(online, player);

            return number;
        }
        public void SendPlayer(Client player)
        {
            NAPI.Entity.SetEntityPosition(player, HouseManager.HouseTypeList[Type].Position + new Vector3(0, 0, 1.12));
            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
            Main.Players[player].InsideHouseID = ID;
            if(HouseManager.HouseTypeList[Type].PetPosition != null) {
                if(!PetName.Equals("null")) Trigger.ClientEvent(player, "petinhouse",  PetName, HouseManager.HouseTypeList[Type].PetPosition.X, HouseManager.HouseTypeList[Type].PetPosition.Y, HouseManager.HouseTypeList[Type].PetPosition.Z, HouseManager.HouseTypeList[Type].PetRotation, Dimension);
            }
            DestroyFurnitures();
            CreateAllFurnitures();
            if (!PlayersInside.Contains(player)) PlayersInside.Add(player);
        }
        public void RemovePlayer(Client player, bool exit = true)
        {
            if (exit)
            {
                NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                NAPI.Entity.SetEntityDimension(player, 0);
            }
            player.ResetData("InvitedHouse_ID");
            Main.Players[player].InsideHouseID = -1;

            if (PlayersInside.Contains(player.Handle)) PlayersInside.Remove(player.Handle);
        }
        public void RemoveFromList(Client player)
        {
            if (PlayersInside.Contains(player)) PlayersInside.Remove(player);
        }
        public void RemoveAllPlayers(Client requster = null)
        {
            for (int i = PlayersInside.Count - 1; i >= 0; i--)
            {
                Client player = NAPI.Entity.GetEntityFromHandle<Client>(PlayersInside[i]);
                if (requster != null && player == requster) continue;

                if (player != null)
                {
                    NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                    NAPI.Entity.SetEntityDimension(player, 0);

                    player.ResetData("InvitedHouse_ID");
                    Main.Players[player].InsideHouseID = -1;
                }

                PlayersInside.RemoveAt(i);
            }
        }
        public void CreateInterior()
        {
            #region Creating Interior ColShape & Marker
            intmarker = NAPI.Marker.CreateMarker(1, HouseManager.HouseTypeList[Type].Position - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, (uint)Dimension);

            intshape = NAPI.ColShape.CreateCylinderColShape(HouseManager.HouseTypeList[Type].Position - new Vector3(0.0, 0.0, 1.0), 2f, 4f, (uint)Dimension);
            intshape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 7);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityEnterColShape: " + ex.Message); }
            };

            intshape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityExitColShape: " + ex.Message); }
            };
            #endregion
        }

        public void changeOwner(string newName)
        {
            Owner = newName;
            this.UpdateLabel();
            this.Save();
        }
    }
    #endregion

    class HouseManager : Script
    {
        public static nLog Log = new nLog("HouseManager");

        public static List<House> Houses = new List<House>();
        public static List<HouseType> HouseTypeList = new List<HouseType>
        {
            // name, position
            new HouseType("Trailer", new Vector3(1973.124, 3816.065, 32.30873), null, 0.0f, "trevorstrailer"),
            new HouseType("Econom", new Vector3(151.2052, -1008.007, -100.12), null, 0.0f,"hei_hw1_blimp_interior_v_motel_mp_milo_"),
            new HouseType("Econom+", new Vector3(265.9691, -1007.078, -102.0758), null, 0.0f,"hei_hw1_blimp_interior_v_studio_lo_milo_"),
            new HouseType("Comfort", new Vector3(346.6991, -1013.023, -100.3162), new Vector3(349.5223, -994.5601, -99.7562), 264.0f, "hei_hw1_blimp_interior_v_apart_midspaz_milo_"),
            new HouseType("Comfort+", new Vector3(-31.35483, -594.9686, 78.9109),  new Vector3(-25.42115, -581.4933, 79.12776), 159.84f, "hei_hw1_blimp_interior_32_dlc_apart_high2_new_milo_"),
            new HouseType("Premium", new Vector3(-17.85757, -589.0983, 88.99482), new Vector3(-38.84652, -578.466, 88.58952), 50.8f, "hei_hw1_blimp_interior_10_dlc_apart_high_new_milo_"),
            new HouseType("Premium+", new Vector3(-173.9419, 497.8622, 136.5341), new Vector3(-164.9799, 480.7568, 137.1526), 40.0f, "apa_ch2_05e_interior_0_v_mp_stilts_b_milo_"),
        };
        private static List<int> MaxRoommates = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };

        private static int GetUID()
        {
            int newUID = 0;
            while (Houses.FirstOrDefault(h => h.ID == newUID) != null) newUID++;
            return newUID;
        }

        public static int DimensionID = 10000;

        #region Events
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                foreach (HouseType house_type in HouseTypeList) house_type.Create();

                var result = MySQL.QueryRead($"SELECT * FROM `houses`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    /*House house = JsonConvert.DeserializeObject<House>(Row["data"].ToString());
                    house.Dimension = DimensionID;
                    house.CreateInterior();
                    house.CreateAllFurnitures();

                    Houses.Add(house);
                    DimensionID++;

                    MySQL.Query($"UPDATE houses SET owner='{house.Owner}',type={house.Type},position='{JsonConvert.SerializeObject(house.Position)}',price={house.Price},locked={house.Locked}," +
                        $"garage={house.GarageID},bank={house.BankID},roommates='{JsonConvert.SerializeObject(house.Roommates)}' WHERE id='{house.ID}'");*/

                    try
                    {
                        var id = Convert.ToInt32(Row["id"].ToString());
                        var owner = Convert.ToString(Row["owner"]);
                        var type = Convert.ToInt32(Row["type"]);
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var price = Convert.ToInt32(Row["price"]);
                        var locked = Convert.ToBoolean(Row["locked"]);
                        var garage = Convert.ToInt32(Row["garage"]);
                        var bank = Convert.ToInt32(Row["bank"]);
                        var roommates = JsonConvert.DeserializeObject<List<string>>(Row["roommates"].ToString());

                        House house = new House(id, owner, type, position, price, locked, garage, bank, roommates);
                        house.Dimension = DimensionID;
                        house.CreateInterior();
                        FurnitureManager.Create(id);
                        house.CreateAllFurnitures();

                        Houses.Add(house);
                        DimensionID++;

                    } catch(Exception e)
                    {
                        Log.Write(Row["id"].ToString() + e.ToString(), nLog.Type.Error);
                    }
                    
                }

                NAPI.Object.CreateObject(0x07e08443, new Vector3(1972.76892, 3815.36694, 33.6632576), new Vector3(0, 0, -109.999962), 255, NAPI.GlobalDimension);
                GarageManager.spawnCarsInGarage();
                Log.Write($"Loaded {Houses.Count} houses.", nLog.Type.Success);
            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        
        public static void Event_OnPlayerDeath(Client player, Client entityKiller, uint weapon)
        {
            try
            {
                NAPI.Entity.SetEntityDimension(player, 0);
                RemovePlayerFromHouseList(player);
            } catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }   
        }

        public static void Event_OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            try
            {
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void SavingHouses()
        {
            foreach (var h in Houses) h.Save();
            Log.Write("Houses has been saved to DB", nLog.Type.Success);
        }

        [ServerEvent(Event.ResourceStop)]
        public void Event_OnResourceStop()
        {
            try
            {
                SavingHouses();
            } catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        #region Methods
        public static House GetHouse(Client player, bool checkOwner = false)
        {
            House house = Houses.FirstOrDefault(h => h.Owner == player.Name);
            if (house != null)
                return house;
            else if (!checkOwner)
            {
                house = Houses.FirstOrDefault(h => h.Roommates.Contains(player.Name));
                return house;
            }
            else
                return null;
        }

        public static House GetHouse(string name, bool checkOwner = false)
        {
            House house = Houses.FirstOrDefault(h => h.Owner == name);
            if (house != null)
                return house;
            else if (!checkOwner)
            {
                house = Houses.FirstOrDefault(h => h.Roommates.Contains(name));
                return house;
            }
            else
                return null;
        }

        public static void RemovePlayerFromHouseList(Client player)
        {
            if (Main.Players[player].InsideHouseID != -1)
            {
                House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                if (house == null) return;
                house.RemoveFromList(player);
            }
        }

        public static void CheckAndKick(Client player)
        {
            var house = GetHouse(player);
            if (house == null) return;
            if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
        }

        public static void ChangeOwner(string oldName, string newName)
        {
            lock (Houses)
            {
                foreach(House h in Houses)
                {
                    if (h.Owner != oldName) continue;
                    Log.Write($"The house was found! [{h.ID}]");
                    h.changeOwner(newName);
                    h.Save();
                }
            }
        }
        #endregion

        public static void interactPressed(Client player, int id)
        {
            switch (id)
            {
                case 6:
                    {
                        if (player.IsInVehicle) return;
                        if (!player.HasData("HOUSEID")) return;

                        House house = Houses.FirstOrDefault(h => h.ID == player.GetData("HOUSEID"));
                        if (house == null) return;
                        if (string.IsNullOrEmpty(house.Owner))
                        {
                            OpenHouseBuyMenu(player);
                            return;
                        }
                        else
                        {
                            if (house.Locked)
                            {
                                var playerHouse = GetHouse(player);
                                if (playerHouse != null && playerHouse.ID == house.ID)
                                    house.SendPlayer(player);
                                else if (player.HasData("InvitedHouse_ID") && player.GetData("InvitedHouse_ID") == house.ID)
                                    house.SendPlayer(player);
                                else
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет доступа", 3000);
                            }
                            else
                                house.SendPlayer(player);
                        }
                        return;
                    }
                case 7:
                    {
                        if (Main.Players[player].InsideHouseID == -1) return;

                        House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                        if (house == null) return;

                        if (player.HasData("IS_EDITING"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить редактирование", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        house.RemovePlayer(player);
                        return;
                    }
            }
        }

        #region Menus
        public static void OpenHouseBuyMenu(Client player)
        {
            Menu menu = new Menu("housebuy", false, false);
            menu.Callback = callback_housebuy;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Покупка дома";
            menu.Add(menuItem);

            if (Main.Players[player].HotelID != -1)
            {
                menuItem = new Menu.Item("hotelinfo", Menu.MenuItem.Card);
                menuItem.Text = "Внимание! При покупке дома Вас выселят из отеля";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("buy", Menu.MenuItem.Button);
            menuItem.Text = "Купить дом";
            menu.Add(menuItem);

            menuItem = new Menu.Item("interior", Menu.MenuItem.Button);
            menuItem.Text = "Посмотреть интерьер";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_housebuy(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "buy":
                    MenuManager.Close(player);
                    if (!player.HasData("HOUSEID")) return;

                    House house = Houses.FirstOrDefault(h => h.ID == player.GetData("HOUSEID"));
                    if (house == null) return;

                    if (!string.IsNullOrEmpty(house.Owner))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этом доме уже имеется хозяин", 3000);
                        return;
                    }

                    if (house.Price > Main.Players[player].Money)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает средств для покупки дома", 3000);
                        return;
                    }

                    if (Houses.Count(h => h.Owner == player.Name) >= 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете купить больше одного дома", 3000);
                        return;
                    }
                    var vehicles = VehicleManager.getAllPlayerVehicles(player.Name).Count;
                    var maxcars = GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars;
                    if (vehicles > maxcars)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Дом, который Вы покупаете, имеет {maxcars} машиномест, продайте лишние машины", 3000);
                        OpenCarsSellMenu(player);
                        return;
                    }
                    if(HouseTypeList[house.Type].PetPosition != null) house.PetName = Main.Players[player].PetName;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили этот дом, не забудьте внести налог за него в банкомате", 3000);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.Center, $"НЕ ЗАБУДЬТЕ ВНЕСТИ НАЛОГИ ЗА ДОМ В БЛИЖАЙШЕМ БАНКОМАТЕ!", 8000);
                    CheckAndKick(player);
                    house.SetLock(true);
                    house.SetOwner(player);
                    house.SendPlayer(player);
                    MoneySystem.Bank.Accounts[house.BankID].Balance = Convert.ToInt32(house.Price / 100 * 0.02) * 2;

                    MoneySystem.Wallet.Change(player, -house.Price);
                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", house.Price, $"houseBuy({house.ID})");
                    return;
                case "interior":
                    MenuManager.Close(player);
                    if (!player.HasData("HOUSEID")) return;

                    house = Houses.FirstOrDefault(h => h.ID == player.GetData("HOUSEID"));
                    if (house == null) return;

                    if (!string.IsNullOrEmpty(house.Owner))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этом доме уже имеется хозяин", 3000);
                        return;
                    }

                    house.SendPlayer(player);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }

        public static void OpenHouseManageMenu(Client player)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("housemanage", false, false);
            menu.Callback = callback_housemanage;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Управление домом";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changestate", Menu.MenuItem.Button);
            menuItem.Text = "Открыть/закрыть";
            menu.Add(menuItem);

            menuItem = new Menu.Item("removeall", Menu.MenuItem.Button);
            menuItem.Text = "Выгнать всех";
            menu.Add(menuItem);

            menuItem = new Menu.Item("furniture", Menu.MenuItem.Button);
            menuItem.Text = "Мебель";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cars", Menu.MenuItem.Button);
            menuItem.Text = "Машины";
            menu.Add(menuItem);

            menuItem = new Menu.Item("roommates", Menu.MenuItem.Button);
            menuItem.Text = "Сожители";
            menu.Add(menuItem);

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать гос-ву за {Convert.ToInt32(house.Price * 0.6)}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_housemanage(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            switch (item.ID)
            {
                case "changestate":
                    house.SetLock(!house.Locked);
                    if (house.Locked) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли дом", 3000);
                    else Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли дом", 3000);
                    return;
                case "removeall":
                    house.RemoveAllPlayers(player);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгнали всех из дома", 3000);
                    return;
                case "furniture":
                    MenuManager.Close(player);
                    OpenFurnitureMenu(player);
                    return;
                case "sell":
                    int price = 0;
                    switch(Main.Accounts[player].VipLvl) {
                        case 0: // None
                            price = Convert.ToInt32(house.Price * 0.6);
                            break;
                        case 1: // Bronze
                            price = Convert.ToInt32(house.Price * 0.65);
                            break;
                        case 2: // Silver
                            price = Convert.ToInt32(house.Price * 0.7);
                            break;
                        case 3: // Gold
                            price = Convert.ToInt32(house.Price * 0.75);
                            break;
                        case 4: // Platinum
                            price = Convert.ToInt32(house.Price * 0.8);
                            break;
                    }
                    Trigger.ClientEvent(player, "openDialog", "HOUSE_SELL_TOGOV", $"Вы действительно хотите продать дом за ${price}?");
                    MenuManager.Close(player);
                    return;
                case "cars":
                    OpenCarsMenu(player);
                    return;
                case "roommates":
                    OpenRoommatesMenu(player);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }
        public static void acceptHouseSellToGov(Client player)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }

            if (Main.Players[player].InsideGarageID != -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны выйти из гаража", 3000);
                return;
            }
            house.RemoveAllPlayers();
            house.SetOwner(null);
            house.PetName = "null";
            Trigger.ClientEvent(player, "deleteCheckpoint", 333);
            Trigger.ClientEvent(player, "deleteGarageBlip");
            int price = 0;
            switch(Main.Accounts[player].VipLvl) {
                case 0: // None
                    price = Convert.ToInt32(house.Price * 0.6);
                    break;
                case 1: // Bronze
                    price = Convert.ToInt32(house.Price * 0.65);
                    break;
                case 2: // Silver
                    price = Convert.ToInt32(house.Price * 0.7);
                    break;
                case 3: // Gold
                    price = Convert.ToInt32(house.Price * 0.75);
                    break;
                case 4: // Platinum
                    price = Convert.ToInt32(house.Price * 0.8);
                    break;
            }
            MoneySystem.Wallet.Change(player, price);
            GameLog.Money($"server", $"player({Main.Players[player].UUID})", Convert.ToInt32(house.Price * 0.6), $"houseSell({house.ID})");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали свой дом государству за {price}$", 3000);
        }

        public static void OpenCarsSellMenu(Client player)
        {
            Menu menu = new Menu("carsell", false, false);
            menu.Callback = callback_carsell;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа автомобилей";
            menu.Add(menuItem);

            menuItem = new Menu.Item("label", Menu.MenuItem.Card);
            menuItem.Text = "Выберите машину, которую хотите продать";
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                var vData = VehicleManager.Vehicles[v];
                var price = (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) ? Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5) : 0;
                menuItem = new Menu.Item(v, Menu.MenuItem.Button);
                menuItem.Text = $"{vData.Model} - {v} ({price}$)";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_carsell(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            var vData = VehicleManager.Vehicles[item.ID];
            var price = (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) ? Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5) : 0;
            MoneySystem.Wallet.Change(player, price);
            GameLog.Money($"server", $"player({Main.Players[player].UUID})", price, $"carSell({vData.Model})");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {vData.Model} ({item.ID}) за {price}$", 3000);
            VehicleManager.Remove(item.ID);
            MenuManager.Close(player);
        }

        public static void OpenFurnitureMenu(Client player)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("furnitures", false, false);
            menu.Callback = callback_furniture0;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Мебель";
            menu.Add(menuItem);

            menuItem = new Menu.Item("buyfurniture", Menu.MenuItem.Button);
            menuItem.Text = "Покупка мебели";
            menu.Add(menuItem);

            menuItem = new Menu.Item("tofurniture", Menu.MenuItem.Button);
            menuItem.Text = "Управление мебелью";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_furniture0(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if(house.ID != Main.Players[player].InsideHouseID) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            if (item.ID == "tofurniture")
            {
                if (!FurnitureManager.HouseFurnitures.ContainsKey(house.ID) || FurnitureManager.HouseFurnitures[house.ID].Count() == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет мебели", 3000);
                    MenuManager.Close(player);
                    return;
                }
                Menu nmenu = new Menu("furnitures", false, false);
                nmenu.Callback = callback_furniture;
            
                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Управление мебелью";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("furniture", Menu.MenuItem.List);
                menuItem.Text = "ID:";
                var list = new List<string>();
                foreach (var f in FurnitureManager.HouseFurnitures[house.ID]) list.Add(f.Value.ID.ToString());
                menuItem.Elements = list;
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("sellit", Menu.MenuItem.Button);
                menuItem.Text = "Продать (7500$)";
                nmenu.Add(menuItem);

                var furn = FurnitureManager.HouseFurnitures[house.ID][Convert.ToInt32(list[0])];
                menuItem = new Menu.Item("type", Menu.MenuItem.Card);
                menuItem.Text = $"Тип: {furn.Name}";
                nmenu.Add(menuItem);

                var open = (furn.IsSet) ? "Да" : "Нет";
                menuItem = new Menu.Item("isSet", Menu.MenuItem.Card);
                menuItem.Text = $"Установлено: {open}";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("change", Menu.MenuItem.Button);
                menuItem.Text = "Установить/Убрать";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("close", Menu.MenuItem.Button);
                menuItem.Text = "Закрыть";
                nmenu.Add(menuItem);

                nmenu.Open(player);
                return;
            } else if(item.ID == "buyfurniture") {

                Menu nmenu = new Menu("furnitures", false, false);
                nmenu.Callback = callback_furniture1;
            
                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Покупка мебели";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("buy1", Menu.MenuItem.Button);
                menuItem.Text = "Оружейный сейф (15000$)";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("buy2", Menu.MenuItem.Button);
                menuItem.Text = "Шкаф с одеждой (15000$)";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("buy3", Menu.MenuItem.Button);
                menuItem.Text = "Шкаф с предметами (15000$)";
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("close", Menu.MenuItem.Button);
                menuItem.Text = "Закрыть";
                nmenu.Add(menuItem);

                nmenu.Open(player);
            }
        }

        private static void callback_furniture1(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if(house.ID != Main.Players[player].InsideHouseID) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            if(FurnitureManager.HouseFurnitures[house.ID].Count() >= 50) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В Вашей квартире уже слишком много мебели, продайте что-то", 3000);
                return;
            }
            if(item.ID == "buy1") {
                if(Main.Players[player].Money < 15000) {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -15000);
                FurnitureManager.newFurniture(house.ID, "Оружейный сейф");
                GameLog.Money("server", $"player({Main.Players[player].UUID})", 15000, $"buyFurn({house.ID} | Оружейный сейф)");
            } else if(item.ID == "buy2") {
                if(Main.Players[player].Money < 15000) {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -15000);
                FurnitureManager.newFurniture(house.ID, "Шкаф с одеждой");
                GameLog.Money("server", $"player({Main.Players[player].UUID})", 15000, $"buyFurn({house.ID} | Шкаф с одеждой)");
            } else if(item.ID == "buy3") {
                if(Main.Players[player].Money < 15000) {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -15000);
                FurnitureManager.newFurniture(house.ID, "Шкаф с предметами");
                GameLog.Money("server", $"player({Main.Players[player].UUID})", 15000, $"buyFurn({house.ID} | Шкаф с предметами)");
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Поздравляем с успешной покупкой мебели!", 3000);
            MenuManager.Close(player);
        }

        private static void callback_furniture(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if(house.ID != Main.Players[player].InsideHouseID) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1 || Main.Players[player].InsideHouseID != house.ID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if (!FurnitureManager.HouseFurnitures.ContainsKey(house.ID) || FurnitureManager.HouseFurnitures[house.ID].Count() == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет мебели", 3000);
                MenuManager.Close(player);
                return;
            }
            int id = Convert.ToInt32(data["1"]["Value"].ToString());
            var f = FurnitureManager.HouseFurnitures[house.ID][id];
            if (item.ID == "sellit")
            {
                if(f.IsSet) {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Уберите мебель перед продажей.", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", "server", 7500, $"sellFurn({house.ID} | {f.Name})");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно продали {f.Name} за 7500$", 3000);
                house.DestroyFurniture(f.ID);
                FurnitureManager.HouseFurnitures[house.ID].Remove(id);
                FurnitureManager.FurnituresItems[house.ID].Remove(id);
                MoneySystem.Wallet.Change(player, 7500);
                MenuManager.Close(player);
                return;
            }
            switch (eventName)
            {
                case "button":
                    switch (f.IsSet)
                    {
                        case true:
                            house.DestroyFurniture(f.ID);
                            f.IsSet = false;
                            menu.Items[4].Text = $"Установлено: Нет";
                            menu.Change(player, 4, menu.Items[4]);
                            return;
                        case false:
                            if (player.HasData("IS_EDITING"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить редактирование", 3000);
                                MenuManager.Close(player);
                                return;
                            }
                            player.SetData("IS_EDITING", true);
                            player.SetData("EDIT_ID", f.ID);
                            Trigger.ClientEvent(player, "startEditing", f.Model);
                            MenuManager.Close(player);
                            return;
                    }
                    return;
                case "listChangeleft":
                case "listChangeright":

                    menu.Items[3].Text = $"Тип: {f.Name}";
                    menu.Change(player, 3, menu.Items[3]);

                    var open = (f.IsSet) ? "Да" : "Нет";
                    menu.Items[4].Text = $"Установлено: {open}";
                    menu.Change(player, 4, menu.Items[4]);
                    return;
            }
        }

        public static void OpenRoommatesMenu(Client player)
        {
            Menu menu = new Menu("roommates", false, false);
            menu.Callback = callback_roommates;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Сожители";
            menu.Add(menuItem);

            var house = GetHouse(player, true);
            if (house.Roommates.Count > 0)
            {
                menuItem = new Menu.Item("label", Menu.MenuItem.Card);
                menuItem.Text = "Нажмите на имя игрока, которого хотите выселить";
                menu.Add(menuItem);

                foreach (var p in house.Roommates)
                {
                    menuItem = new Menu.Item(p, Menu.MenuItem.Button);
                    menuItem.Text = $"{p.Replace('_',' ')}";
                    menu.Add(menuItem);
                }
            }
            else
            {
                menuItem = new Menu.Item("label", Menu.MenuItem.Card);
                menuItem.Text = "У Вас никто не подселен в дом";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_roommates(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "back")
            {
                MenuManager.Close(player);
                return;
            }

            var mName = item.ID;
            var roomMate = NAPI.Player.GetPlayerFromName(mName);

            var house = GetHouse(player);
            if (house.Roommates.Contains(mName)) house.Roommates.Remove(mName);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выселили {mName} из своего дома", 3000);
        }

        public static void OpenCarsMenu(Client player)
        {
            Menu menu = new Menu("cars", false, false);
            menu.Callback = callback_cars;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Машины";
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(v, Menu.MenuItem.Button);
                menuItem.Text = $"{VehicleManager.Vehicles[v].Model} - {v}";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_cars(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "close") return;
                    OpenSelectedCarMenu(player, item.ID);
                }
                catch (Exception e) { Log.Write("callback_cars: " + e.Message + e.Message, nLog.Type.Error); }
            });
        }

        public static void OpenSelectedCarMenu(Client player, string number)
        {
            Menu menu = new Menu("selectedcar", false, false);
            menu.Callback = callback_selectedcar;

            var vData = VehicleManager.Vehicles[number];

            var house = GetHouse(player);
            var garage = GarageManager.Garages[house.GarageID];
            var check = garage.CheckCar(false, number);
            var check_pos = (string.IsNullOrEmpty(vData.Position)) ? false : true;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("model", Menu.MenuItem.Card);
            menuItem.Text = vData.Model;
            menu.Add(menuItem);

            var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));

            menuItem = new Menu.Item("repair", Menu.MenuItem.Button);
            menuItem.Text = $"Восстановить {VehicleManager.VehicleRepairPrice[vClass]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("key", Menu.MenuItem.Button);
            menuItem.Text = $"Получить дубликат ключа";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changekey", Menu.MenuItem.Button);
            menuItem.Text = $"Сменить замки";
            menu.Add(menuItem);

            if (check)
            {
                menuItem = new Menu.Item("evac", Menu.MenuItem.Button);
                menuItem.Text = $"Эвакуировать машину";
                menu.Add(menuItem);

                menuItem = new Menu.Item("gps", Menu.MenuItem.Button);
                menuItem.Text = $"Отметить в GPS";
                menu.Add(menuItem);
            }
            else if (check_pos)
            {
                menuItem = new Menu.Item("evac_pos", Menu.MenuItem.Button);
                menuItem.Text = $"Эвакуировать машину";
                menu.Add(menuItem);
            }

            int price = 0;
            if(BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) {
                switch(Main.Accounts[player].VipLvl) {
                case 0: // None
                    price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5);
                    break;
                case 1: // Bronze
                    price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.6);
                    break;
                case 2: // Silver
                    price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.7);
                    break;
                case 3: // Gold
                    price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.8);
                    break;
                case 4: // Platinum
                    price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.9);
                    break;
                default:
                    price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5);
                    break;
                }
            }
            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать ({price}$)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_selectedcar(Client player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "sell":
                    player.SetData("CARSELLGOV", menu.Items[0].Text); 
                    VehicleManager.VehicleData vData = VehicleManager.Vehicles[menu.Items[0].Text];
                    int price = 0;
                    if(BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) {
                        switch(Main.Accounts[player].VipLvl) {
                            case 0: // None
                                price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5);
                                break;
                            case 1: // Bronze
                                price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.6);
                                break;
                            case 2: // Silver
                                price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.7);
                                break;
                            case 3: // Gold
                                price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.8);
                                break;
                            case 4: // Platinum
                                price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.9);
                                break;
                            default:
                                price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5);
                                break;
                        }
                    }
                    Trigger.ClientEvent(player, "openDialog", "CAR_SELL_TOGOV", $"Вы действительно хотите продать государству {vData.Model} ({menu.Items[0].Text}) за ${price}?");
                    MenuManager.Close(player);
                    return;
                case "repair":
                    vData = VehicleManager.Vehicles[menu.Items[0].Text];
                    if (vData.Health > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в восстановлении", 3000);
                        return;
                    }

                    var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));
                    if (!MoneySystem.Wallet.Change(player, -VehicleManager.VehicleRepairPrice[vClass]))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                        return;
                    }
                    vData.Items = new List<nItem>();
                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", VehicleManager.VehicleRepairPrice[vClass], $"carRepair({vData.Model})");
                    vData.Health = 1000;
                    var garage = GarageManager.Garages[GetHouse(player).GarageID];
                    garage.SendVehicleIntoGarage(menu.Items[0].Text);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы восстановили {vData.Model} ({menu.Items[0].Text})", 3000);
                    return;
                case "evac":
                    if (!Main.Players.ContainsKey(player)) return;

                    var number = menu.Items[0].Text;
                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    var check = garage.CheckCar(false, number);

                    if (!check)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта машина стоит в гараже", 3000);
                        return;
                    }
                    if (Main.Players[player].Money < 200)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {200 - Main.Players[player].Money}$)", 3000);
                        return;
                    }

                    var veh = garage.GetOutsideCar(number);
                    if (veh == null) return;
                    VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntitySharedData(veh, "PETROL")) ? VehicleManager.VehicleTank[veh.Class] : NAPI.Data.GetEntitySharedData(veh, "PETROL");
                    NAPI.Entity.DeleteEntity(veh);
                    garage.SendVehicleIntoGarage(number);

                    MoneySystem.Wallet.Change(player, -200);
                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", 200, $"carEvac");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана в гараж", 3000);
                    return;
                case "evac_pos":
                    if (!Main.Players.ContainsKey(player)) return;

                    number = menu.Items[0].Text;
                    if (string.IsNullOrEmpty(VehicleManager.Vehicles[number].Position))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в эвакуации", 3000);
                        return;
                    }

                    VehicleManager.Vehicles[number].Position = null;
                    VehicleManager.Save(number);

                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    garage.SendVehicleIntoGarage(number);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была эвакуирована в гараж", 3000);
                    return;
                case "gps":
                    if (!Main.Players.ContainsKey(player)) return;

                    number = menu.Items[0].Text;
                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    check = garage.CheckCar(false, number);

                    if (!check)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта машина стоит в гараже", 3000);
                        return;
                    }

                    veh = garage.GetOutsideCar(number);
                    if (veh == null) return;

                    Trigger.ClientEvent(player, "createWaypoint", veh.Position.X, veh.Position.Y);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "В GPS было отмечено расположение Вашей машины", 3000);
                    return;
                case "key":
                    if (!Main.Players.ContainsKey(player)) return;

                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    if (garage.Type == -1)
                    {
                        if (player.Position.DistanceTo(garage.Position) > 4)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около гаража", 3000);
                            return;
                        }
                    }
                    else
                    {
                        if (Main.Players[player].InsideGarageID == -1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в гараже", 3000);
                            return;
                        }
                    }

                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }
                    
                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{menu.Items[0].Text}_{VehicleManager.Vehicles[menu.Items[0].Text].KeyNum}"));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от машины с номером {menu.Items[0].Text}", 3000);
                    return;
                case "changekey":
                    if (!Main.Players.ContainsKey(player)) return;

                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    if (garage.Type == -1)
                    {
                        if (player.Position.DistanceTo(garage.Position) > 4)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около гаража", 3000);
                            return;
                        }
                    }
                    else
                    {
                        if (Main.Players[player].InsideGarageID == -1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в гараже", 3000);
                            return;
                        }
                    }
 
                    if (!MoneySystem.Wallet.Change(player, -1000))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Смена замков стоит $1000", 3000);
                        return;
                    }

                    VehicleManager.Vehicles[menu.Items[0].Text].KeyNum++;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сменили замки на машине {menu.Items[0].Text}. Теперь старые ключи не могут быть использованы", 3000);
                    return;
            }
        }
        #endregion

        #region Commands
        public static void InviteToRoom(Client player, Client guest)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас в доме проживает максимальное кол-во игроков", 3000);
                return;
            }

            if (GetHouse(guest) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок уже живет в доме", 3000);
                return;
            }

            guest.SetData("ROOM_INVITER", player);
            guest.TriggerEvent("openDialog", "ROOM_INVITE", $"Игрок ({player.Value}) предложил Вам подселиться к нему");
            
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили игроку ({guest.Value}) подселиться к Вам", 3000);
        }

        public static void acceptRoomInvite(Client player)
        {
            Client owner = player.GetData("ROOM_INVITER");
            if (owner == null || !Main.Players.ContainsKey(owner)) return;

            House house = GetHouse(owner, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет личного дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В доме проживает максимальное кол-во игроков", 3000);
                return;
            }
            
            house.Roommates.Add(player.Name);
            Trigger.ClientEvent(player, "createCheckpoint", 333, 1, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 1.12), 1, NAPI.GlobalDimension, 220, 220, 0);
            Trigger.ClientEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);

            Notify.Send(owner, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) подселился к Вам", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы подселились к игроку ({owner.Value})", 3000);
        }

        [Command("cleargarages")]
        public static void CMD_CreateHouse(Client player)
        {
            if (!Group.CanUseCmd(player, "save")) return;

            var list = new List<int>();
            lock (GarageManager.Garages)
            {
                foreach (var g in GarageManager.Garages)
                {
                    var house = Houses.FirstOrDefault(h => h.GarageID == g.Key);
                    if (house == null) list.Add(g.Key);
                }
            }

            foreach (var id in list)
            {
                GarageManager.Garages.Remove(id);
                MySQL.Query($"DELETE FROM `garages` WHERE `id`={id}");
            }
        }

        [Command("createhouse")]
        public static void CMD_CreateHouse(Client player, int type, int price)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (type < 0 || type >= HouseTypeList.Count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильный тип", 3000);
                return;
            }

            var bankId = MoneySystem.Bank.Create(string.Empty, 2, 0);
            House new_house = new House(GetUID(), string.Empty, type, player.Position - new Vector3(0, 0, 1.12), price, false, 0, bankId, new List<string>());
            DimensionID++;
            new_house.Dimension = DimensionID;
            new_house.Create();
            FurnitureManager.Create(new_house.ID);
            new_house.CreateInterior();

            Houses.Add(new_house);
        }

        [Command("removehouse")]
        public static void CMD_RemoveHouse(Client player, int id)
        {
            if (!Group.CanUseCmd(player, "save")) return;

            House house = Houses.FirstOrDefault(h => h.ID == id);
            if (house == null) return;

            house.Destroy();
            Houses.Remove(house);
            MySQL.Query($"DELETE FROM `houses` WHERE `id`='{house.ID}'");
        }
        [Command("houseis")]
        public static void CMD_HouseIs(Client player)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData("HOUSEID"));
            if (house == null) return;

            NAPI.Chat.SendChatMessageToPlayer(player, $"{player.GetData("HOUSEID")}");
        }
        [Command("housechange")]
        public static void CMD_HouseOwner(Client player, string newOwner)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData("HOUSEID"));
            if (house == null) return;

            house.changeOwner(newOwner);
            SavingHouses();
        }

        [Command("housenewprice")]
        public static void CMD_setHouseNewPrice(Client player, int price)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }

            House house = Houses.FirstOrDefault(h => h.ID == player.GetData("HOUSEID"));
            if (house == null) return;
            house.Price = price;
            house.UpdateLabel();
            house.Save();
        }

        [Command("myguest")]
        public static void CMD_InvitePlayerToHouse(Client player, int id)
        {
            var guest = Main.GetPlayerByID(id);
            if (guest == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не найден", 3000);
                return;
            }
            if (player.Position.DistanceTo(guest.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко", 3000);
                return;
            }
            InvitePlayerToHouse(player, guest);
        }

        public static void InvitePlayerToHouse(Client player, Client guest)
        {
            House house = GetHouse(player);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            guest.SetData("InvitedHouse_ID", house.ID);
            Notify.Send(guest, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) пригласил Вас в свой дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы пригласили игрока ({guest.Value}) в свой дом", 3000);
        }

        [Command("sellhouse")]
        public static void CMD_sellHouse(Client player, int id, int price)
        {
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не найден", 3000);
                return;
            }
            OfferHouseSell(player, target, price);
        }

        public static void OfferHouseSell(Client player, Client target, int price)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от покупателя", 3000);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            if (GetHouse(target, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть дом", 3000);
                return;
            }
            if (price > 1000000000 || price < house.Price / 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком большая/маленькая цена", 3000);
                return;
            }
            if (player.Position.DistanceTo(house.Position) > 30)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от дома", 3000);
                return;
            }

            target.SetData("HOUSE_SELLER", player);
            target.SetData("HOUSE_PRICE", price);
            Trigger.ClientEvent(target, "openDialog", "HOUSE_SELL", $"Игрок ({player.Value}) предложил Вам купить свой дом за ${price}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили игроку ({target.Value}) купить Ваш дом за {price}$", 3000);
        }

        public static void acceptHouseSell(Client player)
        {
            if (!player.HasData("HOUSE_SELLER") || !Main.Players.ContainsKey(player.GetData("HOUSE_SELLER"))) return;
            Client seller = player.GetData("HOUSE_SELLER");

            if (GetHouse(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть дом", 3000);
                return;
            }

            House house = GetHouse(seller, true);
            var price = player.GetData("HOUSE_PRICE");
            if (house == null || house.Owner != seller.Name) return;
            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }
            CheckAndKick(player);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"houseSell({house.ID})");
            seller.TriggerEvent("deleteCheckpoint", 333);
            seller.TriggerEvent("deleteGarageBlip");
            house.SetOwner(player);
            house.PetName = Main.Players[player].PetName;
            house.Save();

            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) купил у Вас дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили дом у игрока ({seller.Value})", 3000);
        }
        #endregion
    }
}
