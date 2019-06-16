using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace NeptuneEvo.Fractions
{
    class Merryweather : Script
    {
        private static nLog Log = new nLog("Merryweather");

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> Coords = new List<Vector3>
        {
            new Vector3(1571.831, 2240.648, 78.40011), // Колшэйп входа в бункер
            new Vector3(2154.641, 2921.034, -62.82243), // Колшэйп изнутри интерьера для телепорта наверх
            new Vector3(2033.842, 2942.104, -62.82434), // Колшэйп входа на другой этаж
            new Vector3(2155.425, 2921.066, -81.99551), // Колшэйп изнутри этажа, чтобы вернуться назад
        };

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Coords[0], 1f, 2, 0));
                Cols[0].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 82);

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(Coords[1], 1f, 2, 0));
                Cols[1].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[1].SetData("INTERACT", 83);

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(Coords[2], 1f, 2, 0));
                Cols[2].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[2].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[2].SetData("INTERACT", 84);

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(Coords[3], 1f, 2, 0));
                Cols[3].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[3].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[3].SetData("INTERACT", 85);
                
                NAPI.Marker.CreateMarker(1, Coords[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
            } catch(Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_MERRYWEATHER\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void mws_OnEntityEnterColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData("INTERACT"));
            }
            catch (Exception e) { Log.Write("mws_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void mws_OnEntityExitColShape(ColShape shape, Client entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("mws_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Client player, int interact)
        {
            switch (interact)
            {
                case 82:
                case 83:
                case 84:
                case 85:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    if(Main.Players[player].FractionID != 17)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не состоите в Merryweather", 3000);
                        return;
                    }
                    if(interact == 82) NAPI.Entity.SetEntityPosition(player, Coords[1] + new Vector3(0, 0, 1.12));
                    else if(interact == 83) NAPI.Entity.SetEntityPosition(player, Coords[0] + new Vector3(0, 0, 1.12));
                    else if(interact == 84) NAPI.Entity.SetEntityPosition(player, Coords[3] + new Vector3(0, 0, 1.12));
                    else if(interact == 85) NAPI.Entity.SetEntityPosition(player, Coords[2] + new Vector3(0, 0, 1.12));
                    return;
            }
        }
    }
}
