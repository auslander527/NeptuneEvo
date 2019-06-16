using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;

namespace NeptuneEvo.Fractions
{
    class GangsCapture : Script
    {
        private static nLog Log = new nLog("GangCapture");
        private static Config config = new Config("GangCapture");
        public static Dictionary<int, GangPoint> gangPoints = new Dictionary<int, GangPoint>();
        public static bool captureIsGoing = false;
        public static bool captureStarting = false;
        private static string captureTimer;
        private static string toStartCaptureTimer;
        private static int attackersFracID = -1;
        private static int timerCount = 0;
        private static int timerExitCountDef = 0;
        private static int timerExitCountAt = 0;
        private static int attackersSt = 0;
        private static int defendersSt = 0;
        private static bool defendersWas = false;
        private static bool attackersWas = false;
        private static bool smbTryCapture = false;
        public static Dictionary<int, int> gangPointsColor = new Dictionary<int, int>
        {
            { 1, 52 }, // families
            { 2, 58 }, // ballas
            { 3, 70 }, // vagos
            { 4, 77 }, // marabunta
            { 5, 59 }, // blood street
        };
        private static Dictionary<int, string> pictureNotif = new Dictionary<int, string>
        {
            { 1, "CHAR_MP_FAM_BOSS" }, // families
            { 2, "CHAR_MP_GERALD" }, // ballas
            { 3, "CHAR_ORTEGA" }, // vagos
            { 4, "CHAR_MP_ROBERTO" }, // marabunta
            { 5, "CHAR_MP_SNITCH" }, // blood street
        };
        private static Dictionary<int, DateTime> nextCaptDate = new Dictionary<int, DateTime>
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
        };
        private static Dictionary<int, DateTime> protectDate = new Dictionary<int, DateTime>
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
        };
        public static List<Vector3> gangZones = new List<Vector3>()
        {
            // left side
            new Vector3(-200.8397, -1431.556, 30.18104),
            new Vector3(-100.8397, -1431.556, 30.18104),
            new Vector3(-0.8397, -1431.556, 30.18104),
            new Vector3(99.1603, -1431.556, 30.18104),

            new Vector3(-200.8397, -1531.556, 30.18104),
            new Vector3(-100.8397, -1531.556, 30.18104),
            new Vector3(-0.8397, -1531.556, 30.18104),
            new Vector3(99.1603, -1531.556, 30.18104),
            new Vector3(199.1603, -1531.556, 30.18104),
            new Vector3(299.1603, -1531.556, 30.18104),
            new Vector3(399.1603, -1531.556, 30.18104),
            new Vector3(499.1603, -1531.556, 30.18104),

            new Vector3(-200.8397, -1631.556, 30.18104),
            new Vector3(-100.8397, -1631.556, 30.18104),
            new Vector3(-0.8397, -1631.556, 30.18104),
            new Vector3(99.1603, -1631.556, 30.18104),
            new Vector3(199.1603, -1631.556, 30.18104),
            new Vector3(299.1603, -1631.556, 30.18104),
            new Vector3(399.1603, -1631.556, 30.18104),
            new Vector3(499.1603, -1631.556, 30.18104),
            new Vector3(599.1603, -1631.556, 30.18104),

            new Vector3(-100.8397, -1731.556, 30.18104),
            new Vector3(-0.8397, -1731.556, 30.18104),
            new Vector3(99.1603, -1731.556, 30.18104),
            new Vector3(199.1603, -1731.556, 30.18104),
            new Vector3(299.1603, -1731.556, 30.18104),
            new Vector3(399.1603, -1731.556, 30.18104),
            new Vector3(499.1603, -1731.556, 30.18104),
            new Vector3(599.1603, -1731.556, 30.18104),

            new Vector3(-0.8397, -1831.556, 30.18104),
            new Vector3(99.1603, -1831.556, 30.18104),
            new Vector3(199.1603, -1831.556, 30.18104),
            new Vector3(299.1603, -1831.556, 30.18104),
            new Vector3(399.1603, -1831.556, 30.18104),
            new Vector3(499.1603, -1831.556, 30.18104),
            new Vector3(599.1603, -1831.556, 30.18104),

            new Vector3(99.1603, -1931.556, 30.18104),
            new Vector3(199.1603, -1931.556, 30.18104),
            new Vector3(299.1603, -1931.556, 30.18104),
            new Vector3(399.1603, -1931.556, 30.18104),
            new Vector3(499.1603, -1931.556, 30.18104),
            new Vector3(599.1603, -1931.556, 30.18104),

            new Vector3(199.1603, -2031.556, 30.18104),
            new Vector3(299.1603, -2031.556, 30.18104),
            new Vector3(399.1603, -2031.556, 30.18104),
            new Vector3(499.1603, -2031.556, 30.18104),

            new Vector3(299.1603, -2131.556, 30.18104),
            new Vector3(399.1603, -2131.556, 30.18104),

            //right side
            new Vector3(768.8984, -2401.556, 28.17772),
            new Vector3(868.8984, -2401.556, 28.17772),
            new Vector3(968.8984, -2401.556, 28.17772),
            new Vector3(1068.8984, -2401.556, 28.17772),

            new Vector3(768.8984, -2301.556, 28.17772),
            new Vector3(868.8984, -2301.556, 28.17772),
            new Vector3(968.8984, -2301.556, 28.17772),
            new Vector3(1068.8984, -2301.556, 28.17772),

            new Vector3(768.8984, -2201.556, 28.17772),
            new Vector3(868.8984, -2201.556, 28.17772),
            new Vector3(968.8984, -2201.556, 28.17772),
            new Vector3(1068.8984, -2201.556, 28.17772),

            new Vector3(768.8984, -2101.556, 28.17772),
            new Vector3(868.8984, -2101.556, 28.17772),
            new Vector3(968.8984, -2101.556, 28.17772),
            new Vector3(1068.8984, -2101.556, 28.17772),

            new Vector3(768.8984, -2001.556, 28.17772),
            new Vector3(868.8984, -2001.556, 28.17772),
            new Vector3(968.8984, -2001.556, 28.17772),
            new Vector3(1068.8984, -2001.556, 28.17772),

            new Vector3(768.8984, -1901.556, 28.17772),
            new Vector3(868.8984, -1901.556, 28.17772),
            new Vector3(968.8984, -1901.556, 28.17772),
            new Vector3(1068.8984, -1901.556, 28.17772),

            new Vector3(768.8984, -1801.556, 28.17772),
            new Vector3(868.8984, -1801.556, 28.17772),
            new Vector3(968.8984, -1801.556, 28.17772),
            new Vector3(1168.8984, -1801.556, 28.17772),
            new Vector3(1268.8984, -1801.556, 28.17772),

            new Vector3(768.8984, -1701.556, 28.17772),
            new Vector3(868.8984, -1701.556, 28.17772),
            new Vector3(968.8984, -1701.556, 28.17772),
            new Vector3(1168.8984, -1701.556, 28.17772),
            new Vector3(1268.8984, -1701.556, 28.17772),
            new Vector3(1368.8984, -1701.556, 28.17772),

            new Vector3(768.8984, -1601.556, 28.17772),
            new Vector3(868.8984, -1601.556, 28.17772),
            new Vector3(1168.8984, -1601.556, 28.17772),
            new Vector3(1268.8984, -1601.556, 28.17772),
            new Vector3(1368.8984, -1601.556, 28.17772),
            
            new Vector3(1268.8984, -1501.556, 28.17772),
            new Vector3(1368.8984, -1501.556, 28.17772),
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM gangspoints");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    var data = new GangPoint();
                    data.ID = Convert.ToInt32(Row["id"]);
                    data.GangOwner = Convert.ToInt32(Row["gangid"]);
                    data.IsCapture = false;

                    if (data.ID >= gangZones.Count) break;
                    gangPoints.Add(data.ID, data);
                }
                foreach (var gangpoint in gangPoints.Values)
                {
                    var colShape = NAPI.ColShape.Create2DColShape(gangZones[gangpoint.ID].X, gangZones[gangpoint.ID].Y, 100, 100, 0);
                    colShape.OnEntityEnterColShape += onPlayerEnterGangPoint;
                    colShape.OnEntityExitColShape += onPlayerExitGangPoint;
                    colShape.SetData("ID", gangpoint.ID);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_CAPTURE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        /*[Command("test")]
        public static void test(Client player, int index, int style, int color)
        {
            NAPI.Player.SetPlayerFaceFeature(player, index, style);
            var headoverlay = new HeadOverlay();
            headoverlay.Index = (byte)index;
            headoverlay.Opacity = 1.0f;
            headoverlay.Color = (byte)color;
            headoverlay.SecondaryColor = 100;
            NAPI.Player.SetPlayerHeadOverlay(player, index, headoverlay);
        }*/

        public static void CMD_startCapture(Client player)
        {
            if (!Manager.canUseCommand(player, "capture")) return;
            if (player.GetData("GANGPOINT") == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не находитесь ни на одном из регионов", 3000);
                return;
            }
            GangPoint region = gangPoints[player.GetData("GANGPOINT")];
            if (region.GangOwner == Main.Players[player].FractionID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете напасть на свою территорию", 3000);
                return;
            }
            if (DateTime.Now.Hour < 13 || DateTime.Now.Hour > 23)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы можете напасть только с 13:00 до 23:00", 3000);
                return;
            }
            if (DateTime.Now < nextCaptDate[Main.Players[player].FractionID])
            {
                DateTime g = new DateTime((nextCaptDate[Main.Players[player].FractionID] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сможете начать захват только через {min}:{sec}", 3000);
                return;
            }
            if (DateTime.Now < protectDate[region.GangOwner])
            {
                DateTime g = new DateTime((protectDate[region.GangOwner] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сможете начать захват территории этой банды только через {min}:{sec}", 3000);
                return;
            }
            if (Manager.countOfFractionMembers(region.GangOwner) < 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточный онлайн в банде противников", 3000);
                return;
            }
            if (smbTryCapture) return;
            smbTryCapture = true;
            if (captureStarting || captureIsGoing)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Захват территории уже идёт", 3000);
                smbTryCapture = false;
                return;
            }

            timerCount = 0;
            timerExitCountDef = 0;
            timerExitCountAt = 0;
            region.IsCapture = true;
            attackersFracID = Main.Players[player].FractionID;

            //toStartCaptureTimer = Main.StartT(900000, 9999999, (o) => timerStartCapture(region), "CAPTURESTART_TIMER");
            toStartCaptureTimer = Timers.StartOnce(900000, () => timerStartCapture(region));
            Main.ClientEventToAll("setZoneFlash", region.ID, true, gangPointsColor[region.GangOwner]);

            captureStarting = true;
            smbTryCapture = false;

            Manager.sendFractionMessage(region.GangOwner, $"Ахтунг! Сбор в течении 15 минут! {Manager.getName(attackersFracID)} решили отхватить нашу территорию");
            Manager.sendFractionMessage(attackersFracID, "Стреляй! Отжимай! Примерно через 15 минут подлетят противники");
        }

        private static void timerStartCapture(GangPoint region)
        {
            var attackers = 0;
            var defenders = 0;

            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || !p.HasData("GANGPOINT") || p.GetData("GANGPOINT") != region.ID) continue;
                if (Main.Players[p].FractionID == region.GangOwner) defenders++;
                else if (Main.Players[p].FractionID == attackersFracID) attackers++;
            }
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || !p.HasData("GANGPOINT") || p.GetData("GANGPOINT") != region.ID) continue;
                if (Main.Players[p].FractionID == region.GangOwner || Main.Players[p].FractionID == attackersFracID)
                {
                    Trigger.ClientEvent(p, "sendCaptureInformation", attackers, defenders, 0, 0);
                    Trigger.ClientEvent(p, "captureHud", true);
                }
            }

            captureIsGoing = true;
            captureStarting = false;
            //captureTimer = Main.StartT(1000, 1000, (o) => timerUpdate(region, region.ID), "CAPTUREUPDATE_TIMER");
            captureTimer = Timers.Start(1000, () => timerUpdate(region, region.ID));
            //Main.StopT(toStartCaptureTimer, "toStartCaptureTimer_gangcapture");

            Manager.sendFractionMessage(region.GangOwner, $"Ахтунг! На нас напали! {Manager.getName(attackersFracID)} решили отхватить нашу территорию");
            Manager.sendFractionMessage(attackersFracID, "Стреляй! Отжимай! Вы начали войну за территорию");
        }

        private static void timerUpdate(GangPoint region, int id)
        {
            try
            {
                var attackers = 0;
                var defenders = 0;

                var allplayers = Main.Players.Keys.ToList();
                foreach (var p in allplayers)
                {
                    if (!Main.Players.ContainsKey(p) || !p.HasData("GANGPOINT") || p.GetData("GANGPOINT") != region.ID) continue;
                    if (Main.Players[p].FractionID == region.GangOwner) defenders++;
                    else if (Main.Players[p].FractionID == attackersFracID) attackers++;
                }

                attackersSt = attackers;
                defendersSt = defenders;
                if (!defendersWas && defenders != 0)
                    defendersWas = true;
                if (!attackersWas && defendersWas)
                    attackersWas = true;

                if (defenders != 0) timerExitCountDef = 0;
                if (attackers != 0) timerExitCountAt = 0;

                if (defendersWas && defenders == 0)
                {
                    timerExitCountDef++;
                    if (timerExitCountDef >= 60)
                    {
                        endCapture(region, 0, 1);
                        return;
                    }
                }
                if (attackersWas && attackers == 0)
                {
                    timerExitCountAt++;
                    if (timerExitCountAt >= 60)
                    {
                        endCapture(region, 1, 0);
                        return;
                    }
                }

                if (timerCount >= 480 && !defendersWas)
                    endCapture(region, defenders, attackers);

                timerCount++;
                foreach (var p in allplayers)
                {
                    if (!Main.Players.ContainsKey(p) || !p.HasData("GANGPOINT") || p.GetData("GANGPOINT") != region.ID) continue;
                    if (Main.Players[p].FractionID == region.GangOwner || Main.Players[p].FractionID == attackersFracID)
                    {
                        int minutes = timerCount / 60;
                        int seconds = timerCount % 60;
                        Trigger.ClientEvent(p, "sendCaptureInformation", attackers, defenders, minutes, seconds);
                    }
                }
            }
            catch (Exception e) { Log.Write("GangCapture: " + e.Message, nLog.Type.Error); }
        }

        private static void endCapture(GangPoint region, int defenders, int attackers)
        {
            //Main.StopT(captureTimer, "endCapture_gangcapture");
            Timers.Stop(captureTimer);
            NAPI.Task.Run(() => Main.ClientEventToAll("captureHud", false));
            protectDate[region.GangOwner] = DateTime.Now.AddMinutes(20);
            protectDate[attackersFracID] = DateTime.Now.AddMinutes(20);
            if (attackers <= defenders)
            {
                Manager.sendFractionMessage(region.GangOwner, $"Обсосы сбежали! Вы дали им под хвост! Вы отстояли территорию");
                Manager.sendFractionMessage(attackersFracID, "Вы лохонулись! Враги были сильнее! Вы не смогли захватить территорию");
                foreach (var m in Manager.Members.Keys)
                {
                    if (Main.Players[m].FractionID == region.GangOwner)
                    {
                        MoneySystem.Wallet.Change(m, 300);
                        GameLog.Money($"server", $"player({Main.Players[m].UUID})", 300, $"winCapture");
                    }
                }
            }
            else if (attackers > defenders)
            {
                Manager.sendFractionMessage(region.GangOwner, $"Вы прошляпили территорию");
                Manager.sendFractionMessage(attackersFracID, "Шугнули их как детей! Вы захватили территорию");
                region.GangOwner = attackersFracID;
                foreach (var m in Manager.Members.Keys)
                {
                    if (Main.Players[m].FractionID == attackersFracID)
                    {
                        MoneySystem.Wallet.Change(m, 300);
                        GameLog.Money($"server", $"player({Main.Players[m].UUID})", 300, $"winCapture");
                    }
                }
            }
            DateTime nextcapt = DateTime.Now.AddMinutes(config.TryGet<double>("nextcapt", 120));
            nextCaptDate[attackersFracID] = nextcapt;
            region.IsCapture = false;
            captureIsGoing = false;
            NAPI.Task.Run(() =>
            {
                Main.ClientEventToAll("setZoneFlash", region.ID, false);
                Main.ClientEventToAll("setZoneColor", region.ID, gangPointsColor[region.GangOwner]);
            });
        }

        private static void onPlayerEnterGangPoint(ColShape shape, Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID >= 1 && Main.Players[player].FractionID <= 5)
                {
                    //Log.Write($"Gangsta {player.Name} entered gangPoint");
                    player.SetData("GANGPOINT", (int)shape.GetData("ID"));
                    GangPoint region = gangPoints[(int)shape.GetData("ID")];
                    if (region.IsCapture && captureIsGoing && (Main.Players[player].FractionID == attackersFracID || Main.Players[player].FractionID == region.GangOwner))
                    {
                        int minutes = timerCount / 60;
                        int seconds = timerCount % 60;
                        Trigger.ClientEvent(player, "sendCaptureInformation", attackersSt, defendersSt, minutes, seconds);
                        Trigger.ClientEvent(player, "captureHud", true);
                    }
                }
            }
            catch (Exception ex) { Log.Write("onPlayerEnterGangPoint: " + ex.Message, nLog.Type.Error); }
        }

        private static void onPlayerExitGangPoint(ColShape shape, Client player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID >= 1 && Main.Players[player].FractionID <= 5)
                {
                    //Log.Write($"Gangsta {player.Name} exited gangPoint");
                    if (shape.GetData("ID") == player.GetData("GANGPOINT"))
                        player.SetData("GANGPOINT", -1);

                    GangPoint region = gangPoints[(int)shape.GetData("ID")];
                    if (region.IsCapture && (Main.Players[player].FractionID == attackersFracID || Main.Players[player].FractionID == region.GangOwner))
                        Trigger.ClientEvent(player, "captureHud", false);
                }
            }
            catch (Exception ex) { Log.Write("onPlayerExitGangPoint: " + ex.Message, nLog.Type.Error); }
        }

        public static void SavingRegions()
        {
            foreach (var region in gangPoints.Values)
                MySQL.Query($"UPDATE gangspoints SET gangid={region.GangOwner} WHERE id={region.ID}");
            Log.Write("Gang Regions has been saved to DB", nLog.Type.Success);
        }

        public static void LoadBlips(Client player)
        {
            var colors = new List<int>();
            foreach (var g in gangPoints.Values)
                colors.Add(gangPointsColor[g.GangOwner]);

            Trigger.ClientEvent(player, "loadCaptureBlips", Newtonsoft.Json.JsonConvert.SerializeObject(colors));

            if (captureIsGoing || captureStarting) Trigger.ClientEvent(player, "setZoneFlash", gangPoints.FirstOrDefault(g => g.Value.IsCapture == true).Value.ID, true);
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            try
            {
                SavingRegions();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }

        public class GangPoint
        {
            public int ID { get; set; }
            public int GangOwner { get; set; }
            public bool IsCapture { get; set; }
        }
    }
}
