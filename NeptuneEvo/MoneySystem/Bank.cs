using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEvo.Core;
using Redage.SDK;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using NeptuneEvo.GUI;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace NeptuneEvo.MoneySystem
{
    class Bank : Script
    {
        private static nLog Log = new nLog("BankSystem");
        private static Random Rnd = new Random();

        public static Dictionary<int, Data> Accounts = new Dictionary<int, Data>();
        public static ICollection<int> BankAccKeys = Accounts.Keys;

        public enum BankNotifyType
        {
            PaySuccess,
            PayIn,
            PayOut,
            PayError,
            InputError,
        }
        public Bank()
        {
            Log.Write("Loading Bank Accounts...");
            var result = MySQL.QueryRead("SELECT * FROM `money`");
            if (result == null || result.Rows.Count == 0)
            {
                Log.Write("DB return null result.", nLog.Type.Warn);
                return;
            }
            foreach (DataRow Row in result.Rows)
            {
                Data data = new Data();
                data.ID = Convert.ToInt32(Row["id"]);
                data.Type = Convert.ToInt32(Row["type"]);
                data.Holder = Row["holder"].ToString();
                data.Balance = Convert.ToInt64(Row["balance"]);
                Accounts.Add(Convert.ToInt32(Row["id"]), data);
            }
        }

        #region Changing account balance
        public static bool Change(int accountID, long amount, bool notify = true)
        {
            lock (Accounts)
            {
                if (!Accounts.ContainsKey(accountID)) return false;
                if (Accounts[accountID].Balance + amount < 0) return false;
                Accounts[accountID].Balance += amount;
                if (Accounts[accountID].Type == 1 || amount >= 10000) MySQL.Query($"UPDATE `money` SET balance={Accounts[accountID].Balance} WHERE id={accountID}");
                if (Accounts[accountID].Type == 1 && NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder) != null)
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (notify)
                            {
                                if (amount > 0)
                                    BankNotify(NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder), BankNotifyType.PayIn, amount.ToString());
                                else
                                    BankNotify(NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder), BankNotifyType.PayOut, amount.ToString());
                            }
                            NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder).TriggerEvent("UpdateBank", Accounts[accountID].Balance);
                        }
                        catch { }
                    });
                }
                return true;
            }
        }
        #endregion Changing account balance
        #region Transfer money from 1-Acc to 2-Acc
        public static bool Transfer(int firstAccID, int lastAccID, long amount)
        {
            if (firstAccID == 0 || lastAccID == 0)
            {
                Log.Write($"Account ID error [{firstAccID}->{lastAccID}]", nLog.Type.Error);
                return false;
            }
            Data firstAcc = Accounts[firstAccID];
            if (!Accounts.ContainsKey(lastAccID))
            {
                if (firstAcc.Type == 1)
                    BankNotify(NAPI.Player.GetPlayerFromName(firstAcc.Holder), BankNotifyType.InputError, "Такого счета не существует!");
                Log.Write($"Transfer with error. Account does not exist! [{firstAccID.ToString()}->{lastAccID.ToString()}:{amount.ToString()}]", nLog.Type.Warn);
                return false;
            }
            if (!Change(firstAccID, -amount))
            {
                if (firstAcc.Type == 1)
                    BankNotify(NAPI.Player.GetPlayerFromName(firstAcc.Holder), BankNotifyType.PayError, "Недостаточно средств!");
                Log.Write($"Transfer with error. Insufficient funds! [{firstAccID.ToString()}->{lastAccID.ToString()}:{amount.ToString()}]", nLog.Type.Warn);
                return false;
            }
            Change(lastAccID, amount);
            GameLog.Money($"bank({firstAccID})", $"bank({lastAccID})", amount, "bankTransfer");
            return true;
        }
        #endregion Transfer money from 1-Acc to 2-Acc
        #region Save Acc
        public static void Save(int AccID)
        {
            if (!Accounts.ContainsKey(AccID)) return;
            Data acc = Accounts[AccID];
            MySQL.Query($"UPDATE `money` SET `balance`={acc.Balance}, `holder`='{acc.Holder}' WHERE id={AccID}");
        }
        #endregion Save Acc

        public static void BankNotify(Client player, BankNotifyType type, string info)
        {
            switch (type)
            {
                case BankNotifyType.InputError:
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка ввода", 3000);
                    return;
                case BankNotifyType.PayError:
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка списания средств", 3000);
                    return;
                case BankNotifyType.PayIn:
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поступление средств ({info}$)", 3000);
                    return;
                case BankNotifyType.PayOut:
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Списание средств ({info}$)", 3000);
                    return;
            }
        }

        public static int Create(string holder, int type = 1, long balance = 0)
        {
            int id = GenerateUUID();
            Data data = new Data();
            data.ID = id;
            data.Type = type;
            data.Holder = holder;
            data.Balance = balance;
            Accounts.Add(id, data);
            MySQL.Query($"INSERT INTO `money`(`id`, `type`, `holder`, `balance`) VALUES ({id},{type},'{holder}',{balance})");
            Log.Write("Created new Bank Account! ID:" + id.ToString(), nLog.Type.Success);
            return id;
        }
        public static void Remove(int id, string holder)
        {
            if (!Accounts.ContainsKey(id)) return;
            Accounts.Remove(id);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "DELETE FROM `money` WHERE holder=@pn";
            cmd.Parameters.AddWithValue("@pn",holder);
            MySQL.Query(cmd);
            Log.Write("Bank account deleted! ID:" + id, nLog.Type.Warn);
        }
        public static void RemoveByID(int id)
        {
            if (!Accounts.ContainsKey(id)) return;
            Accounts.Remove(id);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "DELETE FROM `money` WHERE id=@pn";
            cmd.Parameters.AddWithValue("@pn",id);
            MySQL.Query(cmd);
            Log.Write("Bank account deleted! ID:" + id, nLog.Type.Warn);
        }
        public static bool isAccExist(int id)
        {
            return Accounts.ContainsKey(id);
        }

        public static Data Get(string holder)
        {
            return Accounts.FirstOrDefault(A => A.Value.Holder == holder).Value;
        }

        public static Data Get(int id)
        {
            return Accounts.FirstOrDefault(A => A.Value.ID == id).Value;
        }

        public static void Update(Client client)
        {
            NAPI.Task.Run(() =>
            {
                Trigger.ClientEvent(client, "UpdateBank", Get(client.Name).Balance);
            });
        }

        private static int GenerateUUID()
        {
            var result = 0;
            while (true)
            {
                result = Rnd.Next(000001, 999999);
                if (!BankAccKeys.Contains(result)) break;
            }
            return result;
        }

        public static void changeHolder(string oldName, string newName)
        {
            List<int> toChange = new List<int>();
            lock (Accounts)
            {
                foreach (KeyValuePair<int, Data> bank in Accounts)
                {
                    if (bank.Value.Holder != oldName) continue;
                    Log.Debug($"The bank was found! [{bank.Key}]");
                    toChange.Add(bank.Key);
                }
                foreach (int id in toChange)
                {
                    Accounts[id].Holder = newName;
                    Save(id);
                }
            }
        }

        internal class Data
        {
            public int ID { get; set; }
            public int Type { get; set; }
            public string Holder { get; set; }
            public long Balance { get; set; }
        }
    }

    class ATM : Script
    {
        private static nLog Log = new nLog("ATMs");

        public static Dictionary<int, ColShape> ATMCols = new Dictionary<int, ColShape>();

        #region ATMs List
        public static List<Vector3> ATMs = new List<Vector3>
        {
            new Vector3(-30.28312, -723.7054, 43.10828),
            new Vector3(-846.4784, -340.7381, 37.56028),
            new Vector3(-30.28312, -723.7054, 43.10828),
            new Vector3(-57.79301, -92.57375, 56.65908),
            new Vector3(-203.8796, -861.4044, 29.14762),
            new Vector3(-301.6998, -830.0975, 31.29726),
            new Vector3(-1315.741, -834.8119, 15.84172),
            new Vector3(-526.7958, -1222.796, 17.33497),
            new Vector3(-165.068, 232.6937, 93.80193),
            new Vector3(147.585, -1035.683, 28.22313),
            new Vector3(-2072.433, -317.1329, 12.19597),
            new Vector3(-2975.008, 380.1415, 13.87914),
            new Vector3(112.6747, -819.3305, 30.21771),
            new Vector3(111.1934, -775.319, 30.31857),
            new Vector3(-3043.924, 594.6759, 6.616974),
            new Vector3(-3241.165, 997.4967, 11.4304),
            new Vector3(-254.3221, -692.4096, 32.49045),
            new Vector3(-256.154, -716.0692, 32.39723),
            new Vector3(-258.849, -723.3128, 32.36183),
            new Vector3(-537.8723, -854.4181, 28.16625),
            new Vector3(-386.8388, 6046.073, 30.38172),
            new Vector3(155.811, 6642.846, 30.48126),
            new Vector3(-2958.9, 487.8209, 14.34391),
            new Vector3(-594.6927, -1161.374, 21.20427),
            new Vector3(-282.9406, 6226.058, 30.37295),
            new Vector3(-3144.312, 1127.521, 19.73535),
            new Vector3(1167.063, -456.2611, 65.6659),
            new Vector3(1138.276, -469.0832, 65.60734),
            new Vector3(-97.33072, 6455.452, 30.34733),
            new Vector3(-821.5346, -1081.945, 10.01243),
            new Vector3(527.2645, -161.3371, 55.95051),
            new Vector3(-1091.597, 2708.577, 17.82036),
            new Vector3(158.4433, 234.1823, 105.5114),
            new Vector3(1171.491, 2702.544, 37.05545),
            new Vector3(1174.94, 2706.804, 36.97408),
            new Vector3(-2294.625, 356.5286, 173.4816),
            new Vector3(-56.88515, -1752.214, 28.30102),
            new Vector3(2564.523, 2584.744, 36.96311),
            new Vector3(2558.747, 350.9788, 107.5015),
            new Vector3(33.25563, -1348.147, 28.37702),
            new Vector3(1822.76, 3683.133, 33.15678),
            new Vector3(1703.047, 4933.534, 40.94364),
            new Vector3(1686.842, 4815.943, 40.88822),
            new Vector3(89.62029, 2.412876, 67.18955),
            new Vector3(-1410.304, -98.57402, 51.31698),
            new Vector3(288.7548, -1282.287, 28.52028),
            new Vector3(-1212.692, -330.7367, 36.66656),
            new Vector3(-1205.556, -325.066, 36.73424),
            new Vector3(-611.844, -704.7563, 30.11593),
            new Vector3(-867.6541, -186.0634, 36.72196),
            new Vector3(289.0122, -1256.787, 28.32075),
            new Vector3(1968.167, 3743.618, 31.22374),
            new Vector3(-1305.292, -706.3788, 24.20243),
            new Vector3(-1570.267, -546.7006, 33.83642),
            new Vector3(1701.183, 6426.415, 31.64404),
            new Vector3(-1430.069, -211.1082, 45.37187),
            new Vector3(-1416.06, -212.0282, 45.38037),
            new Vector3(-1109.778, -1690.661, 3.255033),
            new Vector3(237.3561, 217.8394, 105.1667), // 58 в мэрии
        };
        #endregion ATMs List

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Log.Write("Loading ATMs...");
                for (int i = 0; i < ATMs.Count; i++)
                {
                    if(i != 58) NAPI.Blip.CreateBlip(500, ATMs[i], 0.35f, 27, "ATM", shortRange: true,dimension: 0);
                    ATMCols.Add(i, NAPI.ColShape.CreateCylinderColShape(ATMs[i], 1, 2, 0));
                    ATMCols[i].SetData("NUMBER", i);
                    ATMCols[i].OnEntityEnterColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 13);
                            Jobs.Collector.CollectorEnterATM(e, s);
                        }
                        catch (Exception ex) { Log.Write("ATMCols.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    ATMCols[i].OnEntityExitColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("ATMCols.OnEntityExitrColShape: " + ex.Message, nLog.Type.Error); }
                    };
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static Vector3 GetNearestATM(Client player)
        {
            Vector3 nearesetATM = ATMs[0];
            foreach (var v in ATMs)
            {
                if (v == new Vector3(237.3785, 217.7914, 106.2868)) continue;
                if (player.Position.DistanceTo(v) < player.Position.DistanceTo(nearesetATM))
                    nearesetATM = v;
            }
            return nearesetATM;
        }

        public static void OpenATM(Client player)
        {
            var acc = Main.Players[player];
            if (acc.Bank == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Зарегистрируйте счет в ближайшем отделении банка", 3000);
                return;
            }
            long balance = Bank.Accounts[acc.Bank].Balance;
            Trigger.ClientEvent(player, "setatm", acc.Bank.ToString(), player.Name, balance.ToString(), "");
            Trigger.ClientEvent(player, "openatm");
            return;
        }

        public static void AtmBizGen(Client player)
        {
            var acc = Main.Players[player];
            Log.Debug("Biz count : " + acc.BizIDs.Count);
            if (acc.BizIDs.Count > 0)
            {
                List<string> data = new List<string>();
                foreach (int key in acc.BizIDs)
                {
                    Business biz = BusinessManager.BizList[key];
                    string name = BusinessManager.BusinessTypeNames[biz.Type];
                    data.Add($"{name}");
                }
                Trigger.ClientEvent(player, "atmOpenBiz", JsonConvert.SerializeObject(data), "");
            }
            else
            {
                Trigger.ClientEvent(player, "atmOpen", "[1,0,0]");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет бизнеса!", 3000);
            }
        }

        [RemoteEvent("atmVal")]
        public static void ClientEvent_ATMVAL(Client player, params object[] args)
        {
            try
            {
                if (Admin.IsServerStoping)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Сервер сейчас не может принять это действие", 3000);
                    return;
                }
                var acc = Main.Players[player];
                int type = NAPI.Data.GetEntityData(player, "ATMTYPE");
                string data = Convert.ToString(args[0]);
                int amount;
                if (!Int32.TryParse(data, out amount))
                    return;
                switch (type)
                {
                    case 0:
                        Trigger.ClientEvent(player, "atmClose");
                        if (Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Bank.Change(acc.Bank, +Math.Abs(amount));
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({acc.Bank})", Math.Abs(amount), $"atmIn");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 1:
                        if (Bank.Change(acc.Bank, -Math.Abs(amount)))
                        {
                            Wallet.Change(player, +Math.Abs(amount));
                            GameLog.Money($"bank({acc.Bank})", $"player({Main.Players[player].UUID})", Math.Abs(amount), $"atmOut");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 2:
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null) return;

                        var maxMoney = Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[house.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(house.BankID, +Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({house.BankID})", Math.Abs(amount), $"atmHouse");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        Trigger.ClientEvent(player,
                                "atmOpen", $"[2,'{Bank.Accounts[house.BankID].Balance}/{Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7}$','Сумма внесения наличных']");
                        break;
                    case 3:
                        int bid = NAPI.Data.GetEntityData(player, "ATMBIZ");

                        Business biz = BusinessManager.BizList[acc.BizIDs[bid]];

                        maxMoney = Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[biz.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(biz.BankID, +Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({biz.BankID})", Math.Abs(amount), $"atmBiz");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        Trigger.ClientEvent(player, "atmOpen", $"[2,'{Bank.Accounts[biz.BankID].Balance}/{Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7}$','Сумма зачисления']");
                        break;
                    case 4:
                        if (!Bank.Accounts.ContainsKey(amount) || amount <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        NAPI.Data.SetEntityData(player, "ATM2ACC", amount);
                        Trigger.ClientEvent(player,
                                "atmOpen", "[2,0,'Сумма для перевода']");
                        NAPI.Data.SetEntityData(player, "ATMTYPE", 44);
                        break;
                    case 44:
                        if (Main.Players[player].LVL < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Перевод денег доступен после первого уровня", 3000);
                            return;
                        }
                        if (player.HasData("NEXT_BANK_TRANSFER") && DateTime.Now < player.GetData("NEXT_BANK_TRANSFER"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Следующая транзакция будет возможна в течение минуты", 3000);
                            return;
                        }
                        int bank = NAPI.Data.GetEntityData(player, "ATM2ACC");
                        if (!Bank.Accounts.ContainsKey(bank) || bank <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        if(Bank.Accounts[bank].Type != 1 && Main.Players[player].AdminLVL == 0) {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        if(acc.Bank == bank) {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Операция отменена.", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        Bank.Transfer(acc.Bank, bank, Math.Abs(amount));
                        Trigger.ClientEvent(player, "closeatm");
                        if(Main.Players[player].AdminLVL == 0) player.SetData("NEXT_BANK_TRANSFER", DateTime.Now.AddMinutes(1));
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        [RemoteEvent("atmDP")]
        public static void ClientEvent_ATMDupe(Client client)
        {
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 3)
                {
                    p.SendChatMessage($"!{{#d35400}}[ATM-FLOOD] {client.Name} ({client.Value})");
                }
            }
        }

        [RemoteEvent("atmCB")]
        public static void ClientEvent_ATMCB(Client player, params object[] args)
        {
            try
            {
                var acc = Main.Players[player];
                int type = Convert.ToInt32(args[0]);
                int index = Convert.ToInt32(args[1]);
                if (index == -1)
                {
                    Trigger.ClientEvent(player, "closeatm");
                    return;
                }
                switch (type)
                {
                    case 1:
                        switch (index)
                        {
                            case 0:
                                Trigger.ClientEvent(player,
                                    "atmOpen", "[2,0,'Сумма внесения наличных']");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 1:
                                Trigger.ClientEvent(player,
                                    "atmOpen", "[2,0,'Сумма для снятия']");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 2:
                                if (Houses.HouseManager.GetHouse(player, true) == null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет дома!", 3000);
                                    return;
                                }
                                var house = Houses.HouseManager.GetHouse(player, true);
                                Trigger.ClientEvent(player,
                                    "atmOpen", $"[2,'{Bank.Accounts[house.BankID].Balance}/{Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7}$','Сумма внесения наличных']");
                                Trigger.ClientEvent(player, "setatm", "Дом", $"Дом #{house.ID}", Bank.Accounts[house.BankID].Balance, "");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 3:
                                AtmBizGen(player);
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 4:
                                Trigger.ClientEvent(player,
                                    "atmOpen", "[2,0,'Счет зачисления']");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;

                        }
                        break;
                    case 2:
                        Trigger.ClientEvent(player, "atmOpen", "[1,0,0]");
                        Trigger.ClientEvent(player, "setatm", acc.Bank, player.Name, Bank.Accounts[acc.Bank].Balance, "");
                        break;
                    case 3:
                        if (index == -1)
                        {
                            Trigger.ClientEvent(player, "atmOpen", "[1,0,0]");
                            Trigger.ClientEvent(player, "setatm", acc.Bank, player.Name, Bank.Accounts[acc.Bank].Balance, "");
                            return;
                        }
                        Business biz = BusinessManager.BizList[acc.BizIDs[index]];
                        NAPI.Data.SetEntityData(player, "ATMBIZ", index);
                        Trigger.ClientEvent(player, "atmOpen", $"[2,'{Bank.Accounts[biz.BankID].Balance}/{Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7}$','Сумма зачисления']");
                        Trigger.ClientEvent(player, "setatm",
                            "Бизнес",
                            BusinessManager.BusinessTypeNames[biz.Type],
                            Bank.Accounts[biz.BankID].Balance, "");
                        break;

                }
            }
            catch (Exception e) { Log.Write("atmCB: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("atm")]
        public static void ClientEvent_ATM(Client player, params object[] args)
        {
            try
            {
                if (Admin.IsServerStoping)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Сервер сейчас не может принять это действие", 3000);
                    return;
                }
                int act = Convert.ToInt32(args[0]);
                string data1 = Convert.ToString(args[1]);
                var acc = Main.Players[player];
                int amount;
                if (!Int32.TryParse(data1, out amount))
                    return;
                Log.Debug($"{player.Name} : {data1}");
                switch (act)
                {
                    case 0: //put money
                        if (Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Bank.Change(acc.Bank, amount);
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({acc.Bank})", Math.Abs(amount), $"atmIn");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 1: //take money
                        if (Bank.Change(acc.Bank, -Math.Abs(amount)))
                        {
                            Wallet.Change(player, amount);
                            GameLog.Money($"bank({acc.Bank})", $"player({Main.Players[player].UUID})", Math.Abs(amount), $"atmOut");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 2: //put house tax
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null) return;

                        var maxMoney = Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[house.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(house.BankID, Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({house.BankID})", Math.Abs(amount), $"atmHouse");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        break;
                    case 3: //put biz tax
                        var check = NAPI.Data.GetEntityData(player, "bizcheck");
                        if (check == null) return;
                        if (acc.BizIDs.Count != check)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Возникла ошибка! Попробуйте еще раз.", 3000);
                            return;
                        }
                        int bid = 0;
                        if (!Int32.TryParse(Convert.ToString(args[2]), out bid))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Возникла ошибка! Попробуйте еще раз.", 3000);
                            return;
                        }

                        Business biz = BusinessManager.BizList[acc.BizIDs[bid]];

                        maxMoney = Convert.ToInt32(biz.SellPrice / 100 * 0.01) * 24 * 7;
                        if (Bank.Accounts[biz.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(biz.BankID, Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({biz.BankID})", Math.Abs(amount), $"atmBiz");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        break;
                    case 4: //transfer to
                        int num = 0;
                        if (!Int32.TryParse(Convert.ToString(args[2]), out num))
                            return;
                        Bank.Transfer(acc.Bank, num, +Math.Abs(amount));
                        break;
                }
            }
            catch (Exception e) { Log.Write("atm: " + e.Message, nLog.Type.Error); }
        }
    }
}
