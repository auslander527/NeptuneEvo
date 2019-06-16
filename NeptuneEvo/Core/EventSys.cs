using GTANetworkAPI;
using Redage.SDK;
using System;
using System.Collections.Generic;

namespace NeptuneEvo.Core
{
    class EventSys : Script
    {
        private class CustomEvent
        {
            public string Name { get; set; }
            public Client Admin { get; set; }
            public Vector3 Position { get; set; }
            public uint Dimension { get; set; }
            public ushort MembersLimit { get; set; }
            public Client Winner { get; set; }
            public uint Reward { get; set; }
            public ColShape Zone { get; set; } = null;
            public byte EventState { get; set; } = 0; // 0 - МП не создано, 1 - Создано, но не началось, 2 - Создано и началось.
            public DateTime Started { get; set; }
            public uint RewardLimit { get; set; } = 0;
            public List<Client> EventMembers = new List<Client>();
            public List<Vehicle> EventVehicles = new List<Vehicle>();
        }
        private static CustomEvent AdminEvent = new CustomEvent(); // Одновременно можно будет создать только одно мероприятие.
        private static nLog Log = new nLog("EventSys");
        private static Config config = new Config("EventSys");

        public static void Init()
        {
            AdminEvent.RewardLimit = config.TryGet<uint>("RewardLimit", 20000);
        }
        
        private void DeleteClientFromEvent(Client player)
        {
            AdminEvent.EventMembers.Remove(player);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (AdminEvent.EventState != 0)
            {
                if (AdminEvent.EventMembers.Contains(player))
                {
                    DeleteClientFromEvent(player);
                    if (AdminEvent.EventState == 2)
                    {
                        if (AdminEvent.EventMembers.Count == 0) CloseAdminEvent(AdminEvent.Admin, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client player, Client killer, uint reason)
        {
            if (AdminEvent.EventState != 0)
            {
                if (AdminEvent.EventMembers.Contains(player))
                {
                    DeleteClientFromEvent(player);
                    if (AdminEvent.EventState == 2)
                    {
                        if (AdminEvent.EventMembers.Count == 0) CloseAdminEvent(AdminEvent.Admin, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColshape(ColShape colshape, Client player)
        {
            if (AdminEvent.EventState == 2)
            { // Проверяет только после начала мп, когда телепорт закрыт
                if (AdminEvent.Zone != null)
                {
                    if (AdminEvent.EventMembers.Contains(player))
                    {
                        if (colshape == AdminEvent.Zone)
                        {
                            player.Health = 0;
                            player.Armor = 0;
                            player.SendChatMessage("Вы покинули территорию мероприятия.");
                        }
                    }
                }
            }
        }

        [Command("createmp", "Используйте: /createmp [Лимит участников] [Радиус зоны] [Название мероприятия]", GreedyArg = true)]
        public void CreateEvent(Client player, ushort members, float radius, string eventname)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "createmp")) return;
                if (AdminEvent.EventState == 0)
                {
                    if (eventname.Length < 50)
                    {
                        if (radius >= 10) AdminEvent.Zone = NAPI.ColShape.CreateSphereColShape(player.Position, radius, player.Dimension);
                        AdminEvent.EventState = 1;
                        AdminEvent.EventMembers = new List<Client>();
                        AdminEvent.EventVehicles = new List<Vehicle>();
                        if (members >= NAPI.Server.GetMaxPlayers()) members = 0;
                        AdminEvent.Started = DateTime.Now;
                        AdminEvent.MembersLimit = members;
                        AdminEvent.Name = eventname;
                        AdminEvent.Winner = null;
                        AdminEvent.Position = player.Position;
                        AdminEvent.Dimension = player.Dimension;
                        AdminEvent.Admin = player;
                        AddAdminEventLog();
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Уважаемые игроки, скоро начнётся мероприятие '" + eventname + "'!");
                        if (members != 0) NAPI.Chat.SendChatMessageToAll("!{#A87C33}На данном мероприятии установлен лимит участников: " + members + ".");
                        else NAPI.Chat.SendChatMessageToAll("!{#A87C33}На данном мероприятии не установлен лимит участников.");
                        if (AdminEvent.Zone != null) NAPI.Chat.SendChatMessageToAll("!{#A87C33}Мероприятие действует в зоне " + radius + "м от точки телепорта.");
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Чтобы телепортироваться на мероприятие, введите команду /mp");
                    }
                    else player.SendChatMessage("Слишком длинное название мероприятия, придумайте покороче.");
                }
                else player.SendChatMessage("Одно мероприятие уже создано, нельзя создать новое, пока старое активно.");
            }
        }

        [Command("startmp")]
        public void StartEvent(Client player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "startmp")) return;
                if (AdminEvent.EventState == 1)
                {
                    if (AdminEvent.EventMembers.Count >= 1)
                    {
                        AdminEvent.EventState = 2;
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Мероприятие '" + AdminEvent.Name + "' началось, телепорт закрыт!");
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Игроков на мероприятии: " + AdminEvent.EventMembers.Count + ".");
                    }
                    else player.SendChatMessage("Невозможно запустить мероприятие без участников.");
                }
                else player.SendChatMessage("Мероприятие либо не создано, либо уже запущено.");
            }
        }

        [Command("stopmp", "Используйте: /stopmp [ID игрока] [Награда]")]
        public void MPReward(Client player, int pid, uint reward)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "stopmp")) return;
                if (AdminEvent.EventState == 2)
                {
                    if (reward <= AdminEvent.RewardLimit)
                    {
                        if (AdminEvent.Winner == null)
                        {
                            Client target = Main.GetPlayerByID(pid);
                            if (target != null)
                            {
                                if (AdminEvent.EventMembers.Contains(target) || AdminEvent.Admin == target) CloseAdminEvent(target, reward);
                                else player.SendChatMessage("Данный игрок был найден, но он не участник мероприятия.");
                            }
                            else player.SendChatMessage("Игрока с таким ID не было найдено.");
                        }
                        else player.SendChatMessage("Победитель уже был назначен.");
                    }
                    else player.SendChatMessage("Награда не может превышать выставленный лимит: " + AdminEvent.RewardLimit + ".");
                }
                else player.SendChatMessage("Мероприятие либо не создано, либо еще не запущено.");
            }
        }

        [Command("mpveh", "Используйте: /mpveh [Название модели] [Цвет] [Цвет] [Количество машин]")]
        public void CreateMPVehs(Client player, string model, byte c1, byte c2, byte count)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpveh")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (count >= 1 && count <= 10)
                    {
                        VehicleHash vehHash = NAPI.Util.VehicleNameToModel(model);
                        if (vehHash != 0)
                        {
                            for (byte i = 0; i != count; i++)
                            {
                                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, new Vector3((player.Position.X + (4 * i)), player.Position.Y, player.Position.Z), player.Rotation.Z, c1, c2, "EVENTCAR");
                                vehicle.Dimension = player.Dimension;
                                VehicleStreaming.SetEngineState(vehicle, true);
                                AdminEvent.EventVehicles.Add(vehicle);
                            }
                            AdminEvent.Admin = player;
                        }
                        else player.SendChatMessage("Машины с таким названием не найдено в базе.");
                    }
                    else player.SendChatMessage("За один раз можно создать от 1 до 10 машин.");
                }
                else player.SendChatMessage("Создать транспорт можно только после создания и до начала мероприятия.");
            }
        }

        [Command("mpreward", "Используйте: /mpreward [Новый лимит]")]
        public void SetMPReward(Client player, uint newreward)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].AdminLVL >= 6)
                {
                    if (newreward <= 999999)
                    {
                        AdminEvent.RewardLimit = newreward;
                        try
                        {
                            MySQL.Query($"UPDATE `eventcfg` SET `RewardLimit`={newreward}");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы установили лимит на " + newreward, 3000);
                        }
                        catch (Exception e)
                        {
                            Log.Write("EXCEPTION AT \"SetMPReward\":\n" + e.ToString(), nLog.Type.Error);
                        }
                    }
                    else player.SendChatMessage("Вы ввели слишком большой лимит. Максимально возможный лимит: 999999");
                }
            }
        }

        [Command("mp")]
        public void TpToMp(Client player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].DemorganTime == 0 && Main.Players[player].ArrestTime == 0 && player.HasData("CUFFED") && player.GetData("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData("InDeath") == false)
                {
                    if (AdminEvent.EventState == 1)
                    {
                        if (!AdminEvent.EventMembers.Contains(player))
                        {
                            if (AdminEvent.MembersLimit == 0 || AdminEvent.EventMembers.Count < AdminEvent.MembersLimit)
                            {
                                AdminEvent.EventMembers.Add(player);
                                player.Position = AdminEvent.Position;
                                player.Dimension = AdminEvent.Dimension;
                                player.SendChatMessage("Вы были телепортированы на мероприятие '" + AdminEvent.Name + "'.");
                            }
                            else player.SendChatMessage("К сожалению, список участников полон.");
                        }
                        else player.SendChatMessage("Вы уже занесены в список участников.");
                    }
                    else player.SendChatMessage("Телепорт закрыт.");
                }
                else player.SendChatMessage("Телепорт для Вас недоступен.");
            }
        }

        [Command("mpkick", "Используйте: /mpkick [ID игрока]")]
        public void MPKick(Client player, int pid)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpkick")) return;
                if (AdminEvent.EventState == 1)
                {
                    Client target = Main.GetPlayerByID(pid);
                    if (target != null)
                    {
                        if (AdminEvent.EventMembers.Contains(target))
                        {
                            AdminEvent.Admin = player;
                            target.Health = 0;
                            target.Armor = 0;
                            player.SendChatMessage("Вы выгнали " + target.Name + " с мероприятия.");
                        }
                        else player.SendChatMessage("Игрок с данным ID был найден, но он не участник мероприятия.");
                    }
                    else player.SendChatMessage("Игрока с таким ID не было найдено.");
                }
                else player.SendChatMessage("Выгнать игрока можно только после создания и до начала мероприятия.");
            }
        }

        [Command("mphp", "Используйте: /mphp [Количество HP]")]
        public void MPHeal(Client player, byte health)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mphp")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (health >= 1 && health <= 100)
                    {
                        AdminEvent.Admin = player;
                        foreach (Client target in AdminEvent.EventMembers)
                        {
                            NAPI.Player.SetPlayerHealth(target, health);
                        }
                        player.SendChatMessage("Вы успешно установили всем участником МП " + health + " HP.");
                    }
                    else player.SendChatMessage("Количество HP, которое можно выставить, находится в диапозоне от 1 до 100.");
                }
                else player.SendChatMessage("Выдать HP игрокам можно только до начала мероприятия.");
            }
        }

        [Command("mpar", "Используйте: /mpar [Количество Armor]")]
        public void MPArmor(Client player, byte armor)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpar")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (armor >= 0 && armor <= 100)
                    {
                        AdminEvent.Admin = player;
                        foreach (Client target in AdminEvent.EventMembers)
                        {
                            NAPI.Player.SetPlayerArmor(target, armor);
                        }
                        player.SendChatMessage("Вы успешно установили всем участником МП " + armor + " брони.");
                    }
                    else player.SendChatMessage("Количество Armor, которое можно выставить, находится в диапозоне от 0 до 100.");
                }
                else player.SendChatMessage("Выдать Armor игрокам можно только до начала мероприятия.");
            }
        }

        [Command("mpplayers")]
        public void MpPlayerList(Client player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpplayers")) return;
                if (AdminEvent.EventState != 0)
                {
                    short memcount = Convert.ToInt16(AdminEvent.EventMembers.Count);
                    if (memcount > 0)
                    {
                        if (memcount <= 15)
                        {
                            foreach (Client target in AdminEvent.EventMembers)
                            {
                                player.SendChatMessage("ID: " + target.Value + " | Имя: " + target.Name);
                            }
                            player.SendChatMessage("Игроков на мероприятии: " + memcount);
                        }
                        else player.SendChatMessage("Игроков на мероприятии: " + memcount);
                    }
                    else player.SendChatMessage("Игроков на мероприятии не обнаружено.");
                }
                else player.SendChatMessage("Мероприятие еще не создано.");
            }
        }
        
        private void AddAdminEventLog()
        {
            try
            {
                GameLog.EventLogAdd(AdminEvent.Admin.Name, AdminEvent.Name, AdminEvent.MembersLimit, MySQL.ConvertTime(AdminEvent.Started));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"AddAdminEventLog\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void UpdateLastAdminEventLog()
        {
            try
            {
                GameLog.EventLogUpdate(AdminEvent.Admin.Name,AdminEvent.EventMembers.Count,AdminEvent.Winner.Name,AdminEvent.Reward,MySQL.ConvertTime(DateTime.Now),AdminEvent.RewardLimit, AdminEvent.MembersLimit, AdminEvent.Name);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"UpdateLastAdminEventLog\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void CloseAdminEvent(Client winner, uint reward)
        {
            if (AdminEvent.Zone != null)
            {
                AdminEvent.Zone.Delete();
                AdminEvent.Zone = null;
            }
            if (AdminEvent.EventVehicles.Count != 0)
            {
                foreach (Vehicle vehicle in AdminEvent.EventVehicles)
                {
                    vehicle.Delete();
                }
            }
            AdminEvent.Winner = winner;
            AdminEvent.Reward = reward;
            AdminEvent.EventState = 0;
            UpdateLastAdminEventLog();
            NAPI.Chat.SendChatMessageToAll("!{#A87C33}Мероприятие '" + AdminEvent.Name + "' закончилось, спасибо за участие!");
            if (winner != AdminEvent.Admin)
            {
                if (reward != 0)
                {
                    NAPI.Chat.SendChatMessageToAll("!{#A87C33}Победитель " + winner.Name + " получил приз в размере " + reward + "$.");
                    MoneySystem.Wallet.Change(winner, (int)reward);
                }
                else NAPI.Chat.SendChatMessageToAll("!{#A87C33}Победитель: " + winner.Name + ".");
            }
        }
    }
}
