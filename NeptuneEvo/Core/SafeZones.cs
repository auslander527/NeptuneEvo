using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class SafeZones : Script
    {
        private static nLog Log = new nLog("SafeZones");
        public static void CreateSafeZone(Vector3 position, int height, int width)
        {
            var colShape = NAPI.ColShape.Create2DColShape(position.X, position.Y, height, width, 0);
            colShape.OnEntityEnterColShape += (shape, player) =>
            {
                try
                {
                    Trigger.ClientEvent(player, "safeZone", true);
                }
                catch (Exception e) { Log.Write($"SafeZoneEnter: {e.Message}", nLog.Type.Error); }
                
            };
            colShape.OnEntityExitColShape += (shape, player) =>
            {
                try
                {
                    Trigger.ClientEvent(player, "safeZone", false);
                }
                catch (Exception e) { Log.Write($"SafeZoneExit: {e.Message}", nLog.Type.Error); }
            };
        }

        [ServerEvent(Event.ResourceStart)]
        public void Event_onResourceStart()
        {
            CreateSafeZone(new Vector3(240.7599, -1379.576, 32.74176), 70, 70); // ems safe zone
            //CreateSafeZone(new Vector3(-712.2147, -1298.926, 4.101922), 70, 70); // driving school safe zone
        }
    }
}
