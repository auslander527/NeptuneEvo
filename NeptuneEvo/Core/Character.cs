using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeptuneEvo.Houses;
using NeptuneEvo.GUI;
using MySql.Data.MySqlClient;
using Redage.SDK;

namespace NeptuneEvo.Core.Character
{
    public class Character : CharacterData
    {
        private static nLog Log = new nLog("Character");
        private static Random Rnd = new Random();
        
        public void Spawn(Client player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        player.SetSharedData("IS_MASK", false);

                        // Logged in state, money, phone init
                        Trigger.ClientEvent(player, "loggedIn");
                        player.SetData("LOGGED_IN", true);

                        Trigger.ClientEvent(player, "UpdateMoney", Money);
                        Trigger.ClientEvent(player, "UpdateBank", MoneySystem.Bank.Accounts[Bank].Balance);
                        Trigger.ClientEvent(player, "initPhone");
                        Jobs.WorkManager.load(player);

                        // Skin, Health, Armor, RemoteID
                        player.SetSkin((Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                        player.Health = (Health > 5) ? Health : 5;
                        player.Armor = Armor;

                        player.SetSharedData("REMOTE_ID", player.Value);

                        Voice.Voice.PlayerJoin(player);

                        player.SetSharedData("voipmode", -1);

                        if (Fractions.Manager.FractionTypes[FractionID] == 1 || AdminLVL > 0) Fractions.GangsCapture.LoadBlips(player);
                        if (WantedLVL != null) Trigger.ClientEvent(player, "setWanted", WantedLVL.Level);
                        
                        player.SetData("RESIST_STAGE", 0);
                        player.SetData("RESIST_TIME", 0);
                        if (AdminLVL > 0) player.SetSharedData("IS_ADMIN", true);

                        Dashboard.sendStats(player);
                        Dashboard.sendItems(player);
                        if(Main.Players[player].LVL == 0) {
                            NAPI.Task.Run(() => { try { Trigger.ClientEvent(player, "disabledmg", true); } catch { } }, 5000);
                        }

                        House house = HouseManager.GetHouse(player);
                        if (house != null)
                        {
                            // House blips & checkpoints
                            house.PetName = Main.Players[player].PetName;

                            Trigger.ClientEvent(player, "changeBlipColor", house.blip, 73);

                            Trigger.ClientEvent(player, "createCheckpoint", 333, 1, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 1.12), 1, NAPI.GlobalDimension, 220, 220, 0);
                            Trigger.ClientEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
                        }

                        if (!Customization.CustomPlayerData.ContainsKey(UUID) || !Customization.CustomPlayerData[UUID].IsCreated)
                        {
                            Trigger.ClientEvent(player, "spawnShow", false);
                            Customization.CreateCharacter(player);
                        }
                        else
                        {
                            try
                            {
                                NAPI.Entity.SetEntityPosition(player, Main.Players[player].SpawnPos);
                                List<bool> prepData = new List<bool>
                                {
                                    true,
                                    (FractionID > 0) ? true : false,
                                    (house != null || HotelID != -1) ? true : false,
                                };
                                Trigger.ClientEvent(player, "spawnShow", JsonConvert.SerializeObject(prepData));
                                Customization.ApplyCharacter(player);
                            }
                            catch { }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write($"EXCEPTION AT \"Spawn.NAPI.Task.Run\":\n" + e.ToString(), nLog.Type.Error);
                    }
                });

                if (Warns > 0 && DateTime.Now > Unwarn)
                {
                    Warns--;

                    if (Warns > 0)
                        Unwarn = DateTime.Now.AddDays(14);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Одно предупреждение было снято. У Вас осталось {Warns}", 3000);
                }

                if (!Dashboard.isopen.ContainsKey(player))
                    Dashboard.isopen.Add(player, false);

                nInventory.Check(UUID);
                if (nInventory.Find(UUID, ItemType.BagWithMoney) != null)
                    nInventory.Remove(player, ItemType.BagWithMoney, 1);
                if (nInventory.Find(UUID, ItemType.BagWithDrill) != null)
                    nInventory.Remove(player, ItemType.BagWithDrill, 1);

                if(FractionID == 15) {
                    Trigger.ClientEvent(player, "enableadvert", true);
                    Fractions.LSNews.onLSNPlayerLoad(player);
                }
                if(AdminLVL > 0)
                {
                    ReportSys.onAdminLoad(player);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Spawn\":\n" + e.ToString());
            }
        }

        public async Task Load(Client player, int uuid)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                    Main.Players.Remove(player);

                DataTable result = await MySQL.QueryReadAsync($"SELECT * FROM `characters` WHERE uuid={uuid}");
                if (result == null || result.Rows.Count == 0) return;

                foreach (DataRow Row in result.Rows)
                {
                    UUID = Convert.ToInt32(Row["uuid"]);
                    FirstName = Convert.ToString(Row["firstname"]);
                    LastName = Convert.ToString(Row["lastname"]);
                    Gender = Convert.ToBoolean(Row["gender"]);
                    Health = Convert.ToInt32(Row["health"]);
                    Armor = Convert.ToInt32(Row["armor"]);
                    LVL = Convert.ToInt32(Row["lvl"]);
                    EXP = Convert.ToInt32(Row["exp"]);
                    Money = Convert.ToInt64(Row["money"]);
                    Bank = Convert.ToInt32(Row["bank"]);
                    WorkID = Convert.ToInt32(Row["work"]);
                    FractionID = Convert.ToInt32(Row["fraction"]);
                    FractionLVL = Convert.ToInt32(Row["fractionlvl"]);
                    ArrestTime = Convert.ToInt32(Row["arrest"]);
                    DemorganTime = Convert.ToInt32(Row["demorgan"]);
                    WantedLVL = JsonConvert.DeserializeObject<WantedLevel>(Row["wanted"].ToString());
                    BizIDs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                    AdminLVL = Convert.ToInt32(Row["adminlvl"]);
                    Licenses = JsonConvert.DeserializeObject<List<bool>>(Row["licenses"].ToString());
                    Unwarn = ((DateTime)Row["unwarn"]);
                    Unmute = Convert.ToInt32(Row["unmute"]);
                    Warns = Convert.ToInt32(Row["warns"]);
                    LastVeh = Convert.ToString(Row["lastveh"]);
                    OnDuty = Convert.ToBoolean(Row["onduty"]);
                    LastHourMin = Convert.ToInt32(Row["lasthour"]);
                    HotelID = Convert.ToInt32(Row["hotel"]);
                    HotelLeft = Convert.ToInt32(Row["hotelleft"]);
                    Contacts = JsonConvert.DeserializeObject<Dictionary<int, string>>(Row["contacts"].ToString());
                    Achievements = JsonConvert.DeserializeObject<List<bool>>(Row["achiev"].ToString());
                    if(Achievements == null) {
                        Achievements = new List<bool>();
                        for(uint i = 0; i != 401; i++) Achievements.Add(false);
                    }
                    Sim = Convert.ToInt32(Row["sim"]);
                    PetName =  Convert.ToString(Row["PetName"]);
                    CreateDate = ((DateTime)Row["createdate"]);

                    SpawnPos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    if (Row["pos"].ToString().Contains("NaN"))
                    {
                        Log.Debug("Detected wrong coordinates!", nLog.Type.Warn);
                        if(LVL <= 1) SpawnPos = new Vector3(3372.995, 5183.807, 0.3402423); // На спавне новичков
                        else SpawnPos = new Vector3(-388.5015, -190.0172, 36.19771); // У мэрии
                    }
                }
                player.Name = FirstName + "_" + LastName;
                Main.Players.Add(player, this);
                CheckAchievements(player);
                GameLog.Connected(player.Name, UUID, player.GetData("RealSocialClub"), player.GetData("RealHWID"), player.Value, player.Address);
                Spawn(player);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Load\":\n" + e.ToString());
            }
        }

        public static void CheckAchievements(Client player) {
            try {
                if(Main.Players[player].Achievements[1] && !Main.Players[player].Achievements[2]) player.SetData("CollectThings", 0);
                else if(Main.Players[player].Achievements[2] && !Main.Players[player].Achievements[4] && !Main.Players[player].Achievements[5]) Trigger.ClientEvent(player, "createWaypoint", 1924.4f, 4922.0f);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CheckAchievements\":\n" + e.ToString());
            }
        }

        public async Task<bool> Save(Client player)
        {
            try
            {
                Customization.SaveCharacter(player);

                Vector3 LPos = (player.IsInVehicle) ? player.Vehicle.Position + new Vector3(0, 0, 0.5) : player.Position;
                string pos = JsonConvert.SerializeObject(LPos);
                try
                {
                    if (InsideHouseID != -1)
                    {
                        House house = HouseManager.Houses.FirstOrDefault(h => h.ID == InsideHouseID);
                        if (house != null)
                            pos = JsonConvert.SerializeObject(house.Position + new Vector3(0, 0, 1.12));
                    }
                    if (InsideGarageID != -1)
                    {
                        Garage garage = GarageManager.Garages[InsideGarageID];
                        pos = JsonConvert.SerializeObject(garage.Position + new Vector3(0, 0, 1.12));
                    }
                    if (ExteriorPos != new Vector3())
                    {
                        Vector3 position = ExteriorPos;
                        pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                    }
                    if (InsideHotelID != -1)
                    {
                        Vector3 position = Houses.Hotel.HotelEnters[InsideHotelID];
                        pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                    }
                    if (TuningShop != -1)
                    {
                        Vector3 position = BusinessManager.BizList[TuningShop].EnterPoint;
                        pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadPos\":\n" + e.ToString()); }

                try
                {
                    if (IsSpawned && !IsAlive)
                    {
                        pos = JsonConvert.SerializeObject(Fractions.Ems.emsCheckpoints[2]);
                        Health = 20;
                        Armor = 0;
                    }
                    else
                    {
                        Health = player.Health;
                        Armor = player.Armor;
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadHP\":\n" + e.ToString()); }

                try
                {
                    var aItem = nInventory.Find(UUID, ItemType.BodyArmor);
                    if (aItem != null && aItem.IsActive)
                        aItem.Data = $"{Armor}";
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadArmorItem\":\n" + e.ToString()); }

                try
                {
                    var all_vehicles = VehicleManager.getAllPlayerVehicles(player.Name);
                    foreach (var number in all_vehicles)
                        VehicleManager.Save(number);
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadVehicles\":\n" + e.ToString()); }

                if (!IsSpawned)
                    pos = JsonConvert.SerializeObject(SpawnPos);

                Main.PlayerSlotsInfo[UUID] = new Tuple<int, int, int, long>(LVL, EXP, FractionID, Money);

                await MySQL.QueryAsync($"UPDATE `characters` SET `pos`='{pos}',`gender`={Gender},`health`={Health},`armor`={Armor},`lvl`={LVL},`exp`={EXP}," +
                    $"`money`={Money},`bank`={Bank},`work`={WorkID},`fraction`={FractionID},`fractionlvl`={FractionLVL},`arrest`={ArrestTime}," +
                    $"`wanted`='{JsonConvert.SerializeObject(WantedLVL)}',`biz`='{JsonConvert.SerializeObject(BizIDs)}',`adminlvl`={AdminLVL}," +
                    $"`licenses`='{JsonConvert.SerializeObject(Licenses)}',`unwarn`='{MySQL.ConvertTime(Unwarn)}',`unmute`='{Unmute}'," +
                    $"`warns`={Warns},`hotel`={HotelID},`hotelleft`={HotelLeft},`lastveh`='{LastVeh}',`onduty`={OnDuty},`lasthour`={LastHourMin}," +
                    $"`demorgan`={DemorganTime},`contacts`='{JsonConvert.SerializeObject(Contacts)}',`achiev`='{JsonConvert.SerializeObject(Achievements)}',`sim`={Sim},`PetName`='{PetName}' WHERE `uuid`={UUID}");

                MoneySystem.Bank.Save(Bank);
                await Log.DebugAsync($"Player [{FirstName}:{LastName}] was saved.");
                return true;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Save\":\n" + e.ToString());
                return false;
            }
        }

        public async Task<int> Create(Client player, string firstName, string lastName)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                {
                    Log.Debug("Main.Players.ContainsKey(player)", nLog.Type.Error);
                    return -1;
                }

                if (firstName.Length < 1 || lastName.Length < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка в длине имени/фамилии", 3000);
                    return -1;
                }
                if (Main.PlayerNames.ContainsValue($"{firstName}_{lastName}"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данное имя уже занято", 3000);
                    return -1;
                }

                UUID = GenerateUUID();

                FirstName = firstName;
                LastName = lastName;

                Bank = MoneySystem.Bank.Create($"{firstName}_{lastName}");

                Main.PlayerBankAccs.Add($"{firstName}_{lastName}", Bank);

                Licenses = new List<bool>() { false, false, false, false, false, false, false, false };

                Achievements = new List<bool>();

                for(uint i = 0; i != 401; i++) Achievements.Add(false);

                SpawnPos = new Vector3(3372.995, 5183.807, 0.3402423);

                Main.PlayerSlotsInfo.Add(UUID, new Tuple<int, int, int, long>(LVL, EXP, FractionID, Money));
                Main.PlayerUUIDs.Add($"{firstName}_{lastName}", UUID);
                Main.PlayerNames.Add(UUID, $"{firstName}_{lastName}");

                await MySQL.QueryAsync($"INSERT INTO `characters`(`uuid`,`firstname`,`lastname`,`gender`,`health`,`armor`,`lvl`,`exp`,`money`,`bank`,`work`,`fraction`,`fractionlvl`,`arrest`,`demorgan`,`wanted`," +
                    $"`biz`,`adminlvl`,`licenses`,`unwarn`,`unmute`,`warns`,`lastveh`,`onduty`,`lasthour`,`hotel`,`hotelleft`,`contacts`,`achiev`,`sim`,`pos`,`createdate`) " +
                    $"VALUES({UUID},'{FirstName}','{LastName}',{Gender},{Health},{Armor},{LVL},{EXP},{Money},{Bank},{WorkID},{FractionID},{FractionLVL},{ArrestTime},{DemorganTime}," +
                    $"'{JsonConvert.SerializeObject(WantedLVL)}','{JsonConvert.SerializeObject(BizIDs)}',{AdminLVL},'{JsonConvert.SerializeObject(Licenses)}','{MySQL.ConvertTime(Unwarn)}'," +
                    $"'{Unmute}',{Warns},'{LastVeh}',{OnDuty},{LastHourMin},{HotelID},{HotelLeft},'{JsonConvert.SerializeObject(Contacts)}','{JsonConvert.SerializeObject(Achievements)}',{Sim}," +
                    $"'{JsonConvert.SerializeObject(SpawnPos)}','{MySQL.ConvertTime(CreateDate)}')");
                NAPI.Task.Run(() => { player.Name = FirstName + "_" + LastName; });
                nInventory.Check(UUID);
                Main.Players.Add(player, this);

                return UUID;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Create\":\n" + e.ToString());
                return -1;
            }
        }

        private int GenerateUUID()
        {
            var result = 333333;
            while (Main.UUIDs.Contains(result))
                result = Rnd.Next(000001, 999999);

            Main.UUIDs.Add(result);
            return result;
        }
        
        public static Dictionary<string, string> toChange = new Dictionary<string, string>();
        private static MySqlCommand nameCommand;

        public Character()
        {
            nameCommand = new MySqlCommand("UPDATE `characters` SET `firstname`=@fn, `lastname`=@ln WHERE `uuid`=@uuid");
        }

        public static async Task changeName(string oldName)
        {
            try
            {
                if (!toChange.ContainsKey(oldName)) return;

                string newName = toChange[oldName];

                //int UUID = Main.PlayerNames.FirstOrDefault(u => u.Value == oldName).Key;
                int Uuid = Main.PlayerUUIDs.GetValueOrDefault(oldName);
                if (Uuid <= 0)
                {
                    await Log.WriteAsync($"Cant'find UUID of player [{oldName}]", nLog.Type.Warn);
                    return;
                }

                string[] split = newName.Split("_");

                Main.PlayerNames[Uuid] = newName;
                Main.PlayerUUIDs.Remove(oldName);
                Main.PlayerUUIDs.Add(newName, Uuid);
                try { 
                    if(Main.PlayerBankAccs.ContainsKey(oldName)) { 
                        int bank = Main.PlayerBankAccs[oldName];
                        Main.PlayerBankAccs.Add(newName, bank);
                        Main.PlayerBankAccs.Remove(oldName);
                    }
                } catch { }

                MySqlCommand cmd = nameCommand;
                cmd.Parameters.AddWithValue("@fn", split[0]);
                cmd.Parameters.AddWithValue("@ln", split[1]);
                cmd.Parameters.AddWithValue("@uuid", Uuid);
                await MySQL.QueryAsync(cmd);

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        VehicleManager.changeOwner(oldName, newName);
                        BusinessManager.changeOwner(oldName, newName);
                        MoneySystem.Bank.changeHolder(oldName, newName);
                        Houses.HouseManager.ChangeOwner(oldName, newName);
                    }
                    catch { }
                });

                await Log.DebugAsync("Nickname has been changed!", nLog.Type.Success);
                toChange.Remove(oldName);
                MoneySystem.Donations.Rename(oldName, newName);
                GameLog.Name(Uuid, oldName, newName);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CHANGENAME\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
    }
}
