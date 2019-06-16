using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEvo.Core;
using Redage.SDK;
using System.Linq;
using NeptuneEvo.GUI;

namespace NeptuneEvo.MoneySystem
{
    class Casino : Script
    {
        private static nLog Log = new nLog("Casino");
        private static Config config = new Config("Casino");

        private static readonly Random random = new Random();
        private static readonly byte[] webnums = new byte[37] { 0, 14, 31, 2, 33, 18, 27, 6, 21, 10, 19, 23, 4, 25, 12, 35, 16, 29, 8, 34, 13, 32, 9, 20, 17, 30, 1, 26, 5, 7, 22, 11, 36, 15, 28, 3, 24 }; // номера ячейки для каждого числа 0, 1, 2 и т.д.
        private static int[] rednums = new int[18] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 }; // Все красные числа на рулетке
        private static int WinNum = -1; // Изначально не имеем заданного выигрышного номера
        private static string CasinoT = null; // Таймер казино будет активен только тогда, когда в него будет играть ХОТЯ БЫ 1 человек, во всех других случаях таймер неактивен.
        private static byte CasinoState = 0; // Стартует казино с первой фазы, о самих фазах в самом таймере.
        private static byte CTime = 45; // 45 секунд работает ожидание до старта рулетки + 10 в самом таймере накидывается на прокрутку самого колеса
        private static Dictionary<Client, (ushort, ushort, ushort)> PlayersList = new Dictionary<Client, (ushort, ushort, ushort)>();
        //RED. ZERO, BLACK
        private static long minBalance = config.TryGet<long>("minBalance", 15000);
        private static float colRadius = config.TryGet<float>("ShapeRadius", 2);
        private static Vector3 colPosition = new Vector3
        (
            927.6365,
            44.86219,
            80.9
        );
        private static Vector3 blipPosition = new Vector3
        (
            927.6365,
            44.86219,
            80.9
        );
        private static string blipName = config.TryGet<string>("BlipName", "Casino");
        private static byte blipColor = config.TryGet<byte>("BlipColor", 0);
        private static uint blipID = config.TryGet<uint>("BlipID", 605);

        private static ColShape colShape;
        private static Blip blip;

        #region Events
        public static void OnResourceStart()
        {
            Console.WriteLine(colPosition);
            Console.WriteLine(blipPosition);

            colShape = NAPI.ColShape.CreateCylinderColShape(colPosition, colRadius, 2);
            colShape.OnEntityEnterColShape += (ColShape shape, Client client) =>
            {
                NAPI.Data.SetEntityData(client, "INTERACTIONCHECK", 70);
            };
            colShape.OnEntityExitColShape += (ColShape shape, Client client) =>
            {
                NAPI.Data.SetEntityData(client, "INTERACTIONCHECK", 0);
            };
            colShape.Position = colPosition;

            Console.WriteLine(colShape.Position);
            blip = NAPI.Blip.CreateBlip(blipID, blipPosition, 1, blipColor, blipName, 255, 0, true);
            Console.WriteLine(blip.Position);
            NAPI.Marker.CreateMarker(21, colPosition, new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));
            NAPI.TextLabel.CreateTextLabel("~y~Нажмите E чтобы начать играть", colPosition + new Vector3(0, 0, 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client player, Client killer, uint reason)
        {
            if (PlayersList.ContainsKey(player))
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "rouletecfg", 5, 0, 0); // Обычно CaEnd работает после того как клиент его закроет изнутри, но при смерти нужно закрыть со стороны сервера
            }
        }
        
        [RemoteEvent("PlacedBet")]
        public static void PlacedBet(Client player, ushort red, ushort zero, ushort black)
        {
            try
            {// Тут только добавляем к переменным действующие ставки, снимать со счёта будем в тот момент, когда ставки закроются, в CasinoState 1
                if(CasinoState == 0)
                {
                    if (red + zero + black <= 50000)
                    { // Такая же проверка стоит и на клиенте, на 1 игру ставка максимум 50000 за одну игру
                        long mymoney = Main.Players[player].Money - red - zero - black;
                        if (mymoney >= 0)
                        { // Такая же проверка стоит и на клиенте, но лучше пусть 2 раза перепроверит, что у игрока достаточно денег в банке для игры
                            PlayersList[player] = (red, zero, black);
                        }
                    }
                }
            } catch(Exception e)
            {
                Log.Write("PlacedBet: " + e.ToString(), nLog.Type.Error);
            }
        }

        [RemoteEvent("End")]
        public static void CaEnd(Client player, byte type)
        { // Либо закрыл казино сам, либо PlayerDeath, либо PlayerDisconnected
            if (PlayersList.ContainsKey(player))
            {
                player.Dimension = 0;
                if (CasinoState >= 1)
                { // Если ставки были закрыты
                    ushort cred = PlayersList[player].Item1;
                    ushort czero = PlayersList[player].Item2;
                    ushort cblack = PlayersList[player].Item3;
                    if (cred >= 1 || czero >= 1 || cblack >= 1)
                    { 
                        if (type >= 1)
                        {
                            Main.Players[player].Money += cred + czero + cblack;
                            GameLog.Money("casino", $"player({Main.Players[player].UUID})", (cred + czero + cblack), $"type{type}");
                        }
                    }
                }
                GameLog.CasinoEnd(player.Name, Main.Players[player].UUID, CasinoState, type);
                PlayersList.Remove(player);
                if (PlayersList.Count == 0 && CasinoT != null)
                {
                    Timers.Stop(CasinoT);
                    CTime = 45;
                    CasinoState = 0;
                }
            }
        }
        #endregion

        public static void Interact(Client client)
        {
            if (!PlayersList.ContainsKey(client))
            {
                if (Main.Players[client].Money < minBalance)
                {
                    Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, $"Для игры необходимо иметь баланс больше {minBalance}$", 4000);
                    return;
                }

                if (PlayersList.Count == 0) CasinoT = Timers.StartTask("CasinoT", 5000, CasinoTick); // Если это первый игрок, то запускаем таймер казино с задержкой в 5 секунд
                client.Dimension = (uint)(client.Value + 1);
                PlayersList.Add(client, (0, 0, 0)); // Добавляем игрока в лист игроков
                                                    // Заменить переменную на ту, что держит кол-во денег в банке у player, Обновляем в UI количество реальных денег со счёта
                Trigger.ClientEvent(client, "startroulete", CTime, CasinoState, Main.Players[client].Money);
            }
        }
        public static void Disconnect(Client client, DisconnectionType type)
        {
            if (PlayersList.ContainsKey(client)) CaEnd(client, (byte)type);
        }

        private static void CasinoTick()
        { // Один таймер для всего, чтобы у всех была 1 игра, а не у каждого своя.
            if (PlayersList.Count == 0) {
                Timers.Stop(CasinoT);
                CTime = 45;
                CasinoState = 0;
            } else {
                CTime -= 5;
                if (CTime == 5)
                {
                    if (CasinoState == 0)
                    { // Первая позиция, в которой идёт отсчёт до момента кручения рулетки, ставки доступны.
                        CasinoState = 1;
                        foreach (Client target in PlayersList.Keys)
                        {
                            Trigger.ClientEvent(target, "rouletecfg", 1, 0, -1);
                        }
                    }
                }
                else if (CTime == 0)
                {
                    if (CasinoState == 1)
                    { // Вторая позиция, в которой идёт отсчёт до момента кручения рулетки, ставки закрыты.
                        CasinoState = 2;
                        CTime = 10;
                        WinNum = random.Next(0, 37); // всего цифр на циферблате 37 (0 + 36), 37 цифр прокрутить это сделать ровно 1 оборот
                        int num = random.Next(1, 11); // от 1 до 10 кругов будет происходить оборот + до нужной цифры
                        lock(PlayersList)
                        {
                            foreach (Client target in PlayersList.Keys)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        NAPI.ClientEvent.TriggerClientEvent(target, "rouletecfg", 0, ((num * 37) + webnums[WinNum]), 0);
                                        // Забираем со счёта все активные ставки
                                        ushort cred = PlayersList[target].Item1;
                                        ushort czero = PlayersList[target].Item2;
                                        ushort cblack = PlayersList[target].Item3;
                                        Wallet.Change(target, -(cred + czero + cblack));

                                        GameLog.CasinoPlacedBet(target.Name, Main.Players[target].UUID, cred, czero, cblack);
                                        GameLog.Money($"player({Main.Players[target].UUID})", "casino", (cred + czero + cblack), "placedBet");
                                    } catch(Exception e)
                                    {
                                        Log.Write("CasinoTick: " + e.ToString(), nLog.Type.Error);
                                    }
                                });
                            }
                        }
                    }
                    else if (CasinoState == 2)
                    { // Третья позиция, в которой рулетка крутится, ставки открываются после того, как рулетка докрутится
                        if (WinNum == 0) CasinoWinLoseUpdate(0);
                        else
                        {
                            for (byte i = 0; i != 18; i++)
                            {
                                if (rednums[i] == WinNum)
                                { // RED
                                    CasinoWinLoseUpdate(1);
                                    break;
                                }
                                else if (i == 17) CasinoWinLoseUpdate(2); // BLACK
                            }
                        }
                        CTime = 45;
                        CasinoState = 0;
                    }
                }
                SendTimeAndState();
            }
        }

        private static void CasinoWinLoseUpdate(byte index)
        { // Выдаем деньги победителям
            int wonbet;
            lock(PlayersList)
            {
                foreach (Client target in PlayersList.Keys)
                {
                    wonbet = 0;
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (index == 0)
                            {
                                wonbet = PlayersList[target].Item2;
                                if (wonbet >= 1)
                                {
                                    wonbet = wonbet * 14;
                                    Wallet.Change(target, wonbet);
                                    GameLog.Money("casino", $"player({Main.Players[target].UUID})", wonbet, "winZero");
                                }
                            }
                            else if (index == 1)
                            {
                                wonbet = PlayersList[target].Item1;
                                if (wonbet >= 1)
                                {
                                    wonbet = wonbet * 2;
                                    Wallet.Change(target, wonbet);
                                    GameLog.Money("casino", $"player({Main.Players[target].UUID})", wonbet, "winRed");
                                }
                            }
                            else if (index == 2)
                            {
                                wonbet = PlayersList[target].Item3;
                                if (wonbet >= 1)
                                {
                                    wonbet = wonbet * 2;
                                    Wallet.Change(target, wonbet);
                                    GameLog.Money("casino", $"player({Main.Players[target].UUID})", wonbet, "winBlack");
                                }
                            }
                            // Отсылаем всем игрокам их новые суммы в банке и данные об выигрыше в UI, если он есть.
                            NAPI.ClientEvent.TriggerClientEvent(target, "rouletecfg", 4, wonbet, Main.Players[target].Money);
                            // Аннулируем все активные ставки по серверу
                            PlayersList[target] = (0, 0, 0);

                            GameLog.CasinoWinLose(target.Name, Main.Players[target].UUID, wonbet);
                        }
                        catch(Exception e)
                        {
                            Log.Write("CasinoWinLoseUpdate: " + e.ToString(), nLog.Type.Error);
                        }
                    });
                }
            }
        }

        private static void SendTimeAndState()
        { // Каждые 5 секунд обновляем у игроков время и state в их UI 
            foreach (Client target in PlayersList.Keys)
            {
                //NAPI.ClientEvent.TriggerClientEvent(target, "rouletecfg", 2, CTime, CasinoState);
                Trigger.ClientEvent(target, "rouletecfg", 2, CTime, CasinoState);
            }
        }

        private static int CheckBank(string nickname)
        {
            //Не эффективно, требуется рефактор
            List<Bank.Data> all = Bank.Accounts.Values.ToList();

            Bank.Data acc = all.FirstOrDefault(d => d.Type == 4 && d.Holder == nickname);

            if (acc != null) return acc.ID;
            
            return 0;
        }
    }
}
