using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEvo.GUI
{
    class Docs : Script
    {
        private static nLog Log = new nLog("Docs");
        [RemoteEvent("passport")]
        public static void Event_Passport(Client player, params object[] arguments)
        {
            try
            {
                Client to = (Client)arguments[0];
                Log.Debug(to.Name.ToString());
                Passport(player, to);
            } catch(Exception e)
            {
                Log.Write("EXCEPTION AT \"EVENT_PASSPORT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [RemoteEvent("licenses")]
        public static void Event_Licenses(Client player, params object[] arguments)
        {
            try
            {
                Client to = (Client)arguments[0];
                Licenses(player, to);
            } catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"EVENT_LICENSES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void Passport(Client from, Client to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptPass");
            to.SetData("IS_REQUESTED", true);
            Notify.Send(to, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({from.Value}) хочет показать паспорт. Y/N - принять/отклонить", 3000);
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void Licenses(Client from, Client to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptLics");
            to.SetData("IS_REQUESTED", true);
            Notify.Send(to, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({from.Value}) хочет показать лицензии. Y/N - принять/отклонить", 3000);
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void AcceptPasport(Client player)
        {
            Client from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";
            string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID] : "Безработный";
            List<object> data = new List<object>
                    {
                        acc.UUID,
                        acc.FirstName,
                        acc.LastName,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        fraction,
                        work
                    };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({from.Value}) показал Вам паспорт", 5000);
            Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы показали паспорт игроку ({player.Value})", 5000);
            Log.Debug(json);
            Trigger.ClientEvent(player, "passport", json);
            Trigger.ClientEvent(player, "newPassport", from, acc.UUID);
        }
        public static void AcceptLicenses(Client player)
        {
            Client from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            
            var lic = "";
            for (int i = 0; i < acc.Licenses.Count; i++)
                if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
            if (lic == "") lic = "Отсутствуют";

            List<string> data = new List<string>
                    {
                        acc.FirstName,
                        acc.LastName,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        lic
                    };

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({from.Value}) показал Вам лицензии", 5000);
            Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы показали лицензии игроку ({player.Value})", 5000);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Trigger.ClientEvent(player, "licenses", json);
        }
    }
}
