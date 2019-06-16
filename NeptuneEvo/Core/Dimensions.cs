using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class Dimensions : Script
    {
        private static nLog Log = new nLog("Dimensions");

        private static Dictionary<int, NetHandle> DimensionsInUse = new Dictionary<int, NetHandle>();
        private static ICollection<int> Keys = DimensionsInUse.Keys;

        public static uint RequestPrivateDimension(Client requester)
        {
            int firstUnusedDim = 10000;

            lock (DimensionsInUse)
            {
                while (DimensionsInUse.ContainsKey(--firstUnusedDim))
                {
                }
                DimensionsInUse.Add(firstUnusedDim, requester.Handle);
            }
            Log.Debug($"Dimension {firstUnusedDim.ToString()} is registered for {requester.Name}.");
            return (uint)firstUnusedDim;
        }
        public static void DismissPrivateDimension(Client requester)
        {
            try
            {
                foreach (KeyValuePair<int, NetHandle> dim in DimensionsInUse)
                {
                    if (dim.Value == requester.Handle)
                        DimensionsInUse.Remove(dim.Key);
                    break;
                }
            }
            catch (Exception e) { Log.Write("DismissPrivateDimension: " + e.Message, nLog.Type.Error); }
        }
        public static uint GetPlayerDimension(Client player)
        {
            foreach (var key in Keys)
                if (DimensionsInUse[key] == player.Handle) return (uint)key;
            return 0;
        }
    }
}
