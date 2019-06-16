using MySql.Data.MySqlClient;
using Redage.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NeptuneEvo.Core
{
    public class GameLog
    {

        private static Thread thread;
        private static nLog Log = new nLog("GameLog");
        private static Queue<string> queue = new Queue<string>();
        private static Dictionary<int, DateTime> OnlineQueue = new Dictionary<int, DateTime>();
        
        private static Config config = new Config("MySQL");

        private static string DB = config.TryGet<string>("DataBase", "") + "logs";

        private static string insert = "insert into " + DB + ".{0}({1}) values ({2})";
        
        public static void Votes(uint ElectionId, string Login, string VoteFor)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "votelog", "`election`,`login`,`votefor`,`time`", $"'{ElectionId}','{Login}','{VoteFor}','{DateTime.Now.ToString("s")}'"));
        }
        public static void Stock(int Frac, int Uuid, string Type, int Amount, bool In)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "stocklog", "`time`,`frac`,`uuid`,`type`,`amount`,`in`", $"'{DateTime.Now.ToString("s")}',{Frac},{Uuid},'{Type}',{Amount},{In}"));
        }
        public static void Admin(string Admin, string Action, string Player)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "adminlog", "`time`,`admin`,`action`,`player`", $"'{DateTime.Now.ToString("s")}','{Admin}','{Action}','{Player}'"));
        }
        /// <summary>
        /// Формат для From и To:
        /// Для игрока - player(UUID).
        /// Для бизнеса - biz(ID).
        /// Для банка - bank(UUID).
        /// Для сервисов и услуг - произвольно.
        /// Пример: Money("donate","player(1)",100500)
        /// </summary>
        public static void Money(string From, string To, long Amount, string Comment)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "moneylog", "`time`,`from`,`to`,`amount`,`comment`", $"'{DateTime.Now.ToString("s")}','{From}','{To}',{Amount.ToString()},'{Comment}'"));
        }
        public static void Items(string From, string To, int Type, int Amount, string Data)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "itemslog", "`time`,`from`,`to`,`type`,`amount`,`data`", $"'{DateTime.Now.ToString("s")}','{From}','{To}',{Type},{Amount},'{Data}'"));
        }
        public static void Name(int Uuid, string Old, string New)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "namelog", "`time`,`uuid`,`old`,`new`", $"'{DateTime.Now.ToString("s")}',{Uuid},'{Old}','{New}'"));
        }
        /// <summary>
        /// Лог банов
        /// </summary>
        /// <param name="Admin">UUID админа</param>
        /// <param name="Player">UUID игрока</param>
        public static void Ban(int Admin, int Player, DateTime Until, string Reason, bool isHard)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "banlog", "`time`,`admin`,`player`,`until`,`reason`,`ishard`", $"'{DateTime.Now.ToString("s")}',{Admin},{Player},'{Until.ToString("s")}','{Reason}',{isHard}"));
        }
        public static void Ticket(int player, int target, int sum, string reason, string pnick, string tnick)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "ticketlog", "`time`,`player`,`target`,`sum`,`reason`,`pnick`,`tnick`", $"'{DateTime.Now.ToString("s")}',{player},{target},{sum},'{reason}','{pnick}','{tnick}'"));
        }
        public static void Arrest(int player, int target, string reason, int stars, string pnick, string tnick)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "arrestlog", "`time`,`player`,`target`,`reason`,`stars`,`pnick`,`tnick`", $"'{DateTime.Now.ToString("s")}',{player},{target},'{reason}',{stars},'{pnick}','{tnick}'"));
        }
        public static void Connected(string Name, int Uuid, string SClub, string Hwid, int Id, string ip)
        {
            if (thread == null || OnlineQueue.ContainsKey(Uuid)) return;
            DateTime now = DateTime.Now;
            if(ip.Equals("80.235.53.64")) ip = "31.13.190.88";
            queue.Enqueue(string.Format(
                insert, "connlog", "`in`,`out`,`uuid`,`sclub`,`hwid`,`ip`", $"'{now.ToString("s")}',null,'{Uuid}','{SClub}','{Hwid}','{ip}'"));
            queue.Enqueue(string.Format(
                insert, "idlog", "`in`,`out`,`uuid`,`id`,`name`", $"'{now.ToString("s")}',null,'{Uuid}','{Id}','{Name}'"));
            OnlineQueue.Add(Uuid, now);
        }
        public static void Disconnected(int Uuid)
        {
            if (thread == null || !OnlineQueue.ContainsKey(Uuid)) return;
            DateTime conn = OnlineQueue[Uuid];
            if (conn == null) return;
            OnlineQueue.Remove(Uuid);
            queue.Enqueue($"update {DB}.connlog set `out`='{DateTime.Now.ToString("s")}' WHERE `in`='{conn.ToString("s")}' and `uuid`={Uuid}");
            //queue.Enqueue($"update masklog set `out`='{DateTime.Now.ToString("s")}' WHERE `in`='{conn.ToString("s")}' and `uuid`={Uuid}");
        }
        public static void CharacterDelete(string name, int uuid, string account)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "deletelog", "`time`,`uuid`,`name`,`account`", $"'{DateTime.Now.ToString("s")}',{uuid},'{name}','{account}'"));
        }
        public static void EventLogAdd(string AdmName, string EventName, ushort MembersLimit, string Started)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "eventslog", "`AdminStarted`,`EventName`,`MembersLimit`,`Started`", $"'{AdmName}','{EventName}','{MembersLimit}','{Started}'"));
        }
        public static void EventLogUpdate(string AdmName, int MembCount, string WinName, uint Reward, string Time, uint RewardLimit, ushort MemLimit, string EvName)
        {
            if (thread == null) return;
            queue.Enqueue($"update {DB}.eventslog set `AdminClosed`='{AdmName}',`Members`={MembCount},`Winner`='{WinName},`Reward`={Reward},`Ended`='{Time}',`RewardLimit`={RewardLimit} WHERE `Winner`='Undefined' AND `MembersLimit`={MemLimit} AND `EventName`='{EvName}'");
        }
        public static void CasinoPlacedBet(string name, int uuid, ushort red, ushort zero, ushort black)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "casinobetlog", "`time`,`name`,`uuid`,`red`,`zero`,`black`",
                $"'{DateTime.Now.ToString("s")}','{name}',{uuid},'{red}','{zero}','{black}'"));
        }
        public static void CasinoEnd(string name, int uuid, byte casino, byte disctype)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "casinoendlog", "`time`,`name`,`uuid`,`state`,`type`",
                $"'{DateTime.Now.ToString("s")}','{name}',{uuid},{casino},{disctype}"));
        }
        public static void CasinoWinLose(string name, int uuid, int wonbet)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "casinowinloselog", "`time`,`name`,`uuid`,`wonbet`",
                $"'{DateTime.Now.ToString("s")}','{name}',{uuid},{wonbet}"));
        }
        #region Логика потока
        public static void Start()
        {
            thread = new Thread(Worker);
            thread.IsBackground = true;
            thread.Start();
        }
        private static void Worker()
        {
            string CMD = "";
            try
            {
                Log.Debug("Worker started");
                while (true)
                {
                    if (queue.Count < 1) continue;
                    else
                        MySQL.Query(queue.Dequeue());
                }
            }
            catch (Exception e)
            {
                Log.Write($"{e.ToString()}\n{CMD}", nLog.Type.Error);
            }
        }
        public static void Stop()
        {
            thread.Join();
        }
        #endregion
    }
}
