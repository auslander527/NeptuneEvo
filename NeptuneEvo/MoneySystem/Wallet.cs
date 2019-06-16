using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.MoneySystem
{
    class Wallet : Script
    {
        private static nLog Log = new nLog("Wallet");

        public static bool Change(Client player, int Amount)
        {
            if (!Main.Players.ContainsKey(player)) return false;
            if (Main.Players[player] == null) return false;
            int temp = (int)Main.Players[player].Money + Amount;
            if (temp < 0) return false;
            Main.Players[player].Money = temp;
            Trigger.ClientEvent(player, "UpdateMoney", temp, Convert.ToString(Amount));
            MySQL.Query($"UPDATE characters SET money={Main.Players[player].Money} WHERE uuid={Main.Players[player].UUID}");
            //MoneyLog.Write("Wallet", player.Name, Amount);
            return true;
        }
        public static void Set(Client player, long Amount)
        {
            var data = Main.Players[player];
            if (data == null) return;
            data.Money = Amount;
            Trigger.ClientEvent(player, "UpdateMoney", data.Money);
        }
    }
}
