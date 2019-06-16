using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using NeptuneEvo.GUI;
using System.Linq;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class CarRoom : Script
    {
        private static nLog Log = new nLog("Carroom");

        public static Vector3 CamPosition = new Vector3(-42.3758, -1101.672, 26.42235); // Позиция камеры
        public static Vector3 CamRotation = new Vector3(0, 0, 1.701622); // Rotation камеры
        public static Vector3 CarSpawnPos = new Vector3(-42.79771, -1095.676, 26.0117); // Место для спавна машины в автосалоне
        public static Vector3 CarSpawnRot = new Vector3(-0.05113003, 0.05638388, 91.65826); // Rotation для спавна машины в автосалоне

        public static void onPlayerDissonnectedHandler(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (NAPI.Data.GetEntityData(player, "CARROOM_CAR") != null)
                {
                    var veh = NAPI.Data.GetEntityData(player, "CARROOM_CAR");
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(veh);
                        }
                        catch (Exception e) { Log.Write("CarRoom entity delete: " + e.Message); }
                    });
                }
                    
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void enterCarroom(Client player, string name)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player)) return;
            Main.Players[player].ExteriorPos = player.Position;
            NAPI.Entity.SetEntityPosition(player, new Vector3(CamPosition.X, CamPosition.Y - 2, CamPosition.Z));
            NAPI.Entity.SetEntityDimension(player, 1);
            player.FreezePosition = true;
            player.SetData("INTERACTIONCHECK", 0);
            Trigger.ClientEvent(player, "carRoom");

            OpenCarromMenu(player, BusinessManager.BizList[player.GetData("CARROOMID")].Type);
        }

        #region Menu
        private static Dictionary<string, Color> carColors = new Dictionary<string, Color>
        {
            { "Черный", new Color(0, 0, 0) },
            { "Белый", new Color(225, 225, 225) },
            { "Красный", new Color(230, 0, 0) },
            { "Оранжевый", new Color(255, 115, 0) },
            { "Желтый", new Color(240, 240, 0) },
            { "Зеленый", new Color(0, 230, 0) },
            { "Голубой", new Color(0, 205, 255) },
            { "Синий", new Color(0, 0, 230) },
            { "Фиолетовый", new Color(190, 60, 165) },
        };

        public static void OpenCarromMenu(Client player, int biztype)
        {
            biztype -= 2;

            var prices = new List<int>();
            Business biz = BusinessManager.BizList[player.GetData("CARROOMID")];
            foreach (var p in biz.Products)
                prices.Add(p.Price);

            Trigger.ClientEvent(player, "openAuto", JsonConvert.SerializeObject(BusinessManager.CarsNames[biztype]), JsonConvert.SerializeObject(prices));
        }

        [RemoteEvent("carroomBuy")]
        public static void RemoteEvent_carroomBuy(Client player, string vName, string color)
        {
            try
            {
                Business biz = BusinessManager.BizList[player.GetData("CARROOMID")];
                NAPI.Entity.SetEntityPosition(player, new Vector3(biz.EnterPoint.X, biz.EnterPoint.Y, biz.EnterPoint.Z + 1.5));
                player.FreezePosition = false;

                Main.Players[player].ExteriorPos = new Vector3();
                Trigger.ClientEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);

                var house = Houses.HouseManager.GetHouse(player, true);
                if (house == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                    return;
                }
                if (house.GarageID == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
                    return;
                }
                var garage = Houses.GarageManager.Garages[house.GarageID];
                if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                    return;
                }

                var prod = biz.Products.FirstOrDefault(p => p.Name == vName);
                if (Main.Players[player].Money < prod.Price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                if (!BusinessManager.takeProd(biz.ID, 1, vName, prod.Price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машин этой модели больше нет на складе", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -prod.Price);
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", prod.Price, $"buyCar({vName})");

                var vNumber = VehicleManager.Create(player.Name, vName, carColors[color], carColors[color]);
                garage.SpawnCar(vNumber);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {vName} с номером {vNumber}", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена в Ваш гараж", 5000);
            }
            catch (Exception e) { Log.Write("CarroomBuy: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("carroomCancel")]
        public static void RemoteEvent_carroomCancel(Client player)
        {
            try
            {
                if (!player.HasData("CARROOMID")) return;
                var enterPoint = BusinessManager.BizList[player.GetData("CARROOMID")].EnterPoint;
                NAPI.Entity.SetEntityPosition(player, new Vector3(enterPoint.X, enterPoint.Y, enterPoint.Z + 1.5));
                Main.Players[player].ExteriorPos = new Vector3();
                player.FreezePosition = false;
                Dimensions.DismissPrivateDimension(player);
                player.ResetData("CARROOMID");
                NAPI.Entity.SetEntityDimension(player, 0);
                Trigger.ClientEvent(player, "destroyCamera");
            }
            catch (Exception e) { Log.Write("carroomCancel: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}
