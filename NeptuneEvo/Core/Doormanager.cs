using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class Doormanager : Script
    {
        private static nLog Log = new nLog("Doormanager");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                //X:266,3624 Y:217,5697 Z:110,4328
                RegisterDoor(-1246222793, new Vector3(0, 0, 0)); // pacific standart staff door
                SetDoorLocked(0, true, 0);

                RegisterDoor(1956494919, new Vector3(0, 0, 0)); // pacific standart staff door
                SetDoorLocked(1, true, 0);

                RegisterDoor(961976194, new Vector3(255.2283, 223.976, 102.3932)); // safe door 
                SetDoorLocked(2, true, 0);

                RegisterDoor(110411286, new Vector3(232.6054, 214.1584, 106.4049)); // pacific standart main door 1
                SetDoorLocked(3, true, 0);

                RegisterDoor(110411286, new Vector3(231.5123, 216.5177, 106.4049)); // pacific standart main door 2
                SetDoorLocked(4, true, 0);

                RegisterDoor(631614199, new Vector3(461.8065, -997.6583, 25.06443)); // police prison door
                SetDoorLocked(5, true, 0);

                RegisterDoor(1335309163, new Vector3(260.6518, 203.2292, 106.4328)); // pacific exit door
                SetDoorLocked(6, true, 0);

                RegisterDoor(1335309163, new Vector3(258.2093, 204.119, 106.4328)); // pacific exit door 2
                SetDoorLocked(7, true, 0);

                NAPI.World.DeleteWorldProp(-1148826190, new Vector3(82.38156, -1390.476, 29.52609), 30f);
                NAPI.World.DeleteWorldProp(868499217, new Vector3(82.38156, -1390.752, 29.52609), 30f);
                NAPI.World.DeleteWorldProp(-8873588, new Vector3(842.7685, -1024.539, 28.34478), 30f);
                NAPI.World.DeleteWorldProp(97297972, new Vector3(845.3694, -1024.539, 28.34478), 30f);
                NAPI.World.DeleteWorldProp(-8873588, new Vector3(-662.6415, -944.3256, 21.97915), 30f);
                NAPI.World.DeleteWorldProp(97297972, new Vector3(-665.2424, -944.3256, 21.97915), 30f);
                NAPI.World.DeleteWorldProp(-8873588, new Vector3(810.5769, -2148.27, 29.76892), 30f);
                NAPI.World.DeleteWorldProp(97297972, new Vector3(813.1779, -2148.27, 29.76892), 30f);
                NAPI.World.DeleteWorldProp(-8873588, new Vector3(18.572, -1115.495, 29.94694), 30f);
                NAPI.World.DeleteWorldProp(97297972, new Vector3(16.12787, -1114.606, 29.94694), 30f);
                NAPI.World.DeleteWorldProp(-8873588, new Vector3(243.8379, -46.52324, 70.09098), 30f);
                NAPI.World.DeleteWorldProp(97297972, new Vector3(244.7275, -44.07911, 70.09098), 30f);
                NAPI.World.DeleteWorldProp(-1922281023, new Vector3(-715.6154, -157.2561, 37.67493), 30f);
                NAPI.World.DeleteWorldProp(-1922281023, new Vector3(-716.6755, -155.42, 37.67493), 30f);
                NAPI.World.DeleteWorldProp(-1922281023, new Vector3(-1456.201, -233.3682, 50.05648), 30f);
                NAPI.World.DeleteWorldProp(-1922281023, new Vector3(-1454.782, -231.7927, 50.05649), 30f);
                NAPI.World.DeleteWorldProp(-1922281023, new Vector3(-156.439, -304.4294, 39.99308), 30f);
                NAPI.World.DeleteWorldProp(-1922281023, new Vector3(-157.1293, -306.4341, 39.99308), 30f);
                NAPI.World.DeleteWorldProp(1780022985, new Vector3(-1201.435, -776.8566, 17.99184), 30f);
                NAPI.World.DeleteWorldProp(1780022985, new Vector3(127.8201, -211.8274, 55.22751), 30f);
                NAPI.World.DeleteWorldProp(1780022985, new Vector3(617.2458, 2751.022, 42.75777), 30f);
                NAPI.World.DeleteWorldProp(1780022985, new Vector3(-3167.75, 1055.536, 21.53288), 30f);
                NAPI.World.DeleteWorldProp(145369505, new Vector3(-822.4442, -188.3924, 37.81895), 30f);
                NAPI.World.DeleteWorldProp(-1663512092, new Vector3(-823.2001, -187.0831, 37.81895), 30f);
                NAPI.World.DeleteWorldProp(-1844444717, new Vector3(-29.86917, -148.1571, 57.22648), 30f);
                NAPI.World.DeleteWorldProp(-1844444717, new Vector3(1932.952, 3725.154, 32.9944), 30f);
                NAPI.World.DeleteWorldProp(1417577297, new Vector3(-59.89302, -1092.952, 26.88362), 30f);
                NAPI.World.DeleteWorldProp(2059227086, new Vector3(-39.13366, -1108.218, 26.7198), 30f);
                NAPI.World.DeleteWorldProp(1417577297, new Vector3(-60.54582, -1094.749, 26.88872), 30f);
                NAPI.World.DeleteWorldProp(2059227086, new Vector3(-59.89302, -1092.952, 26.88362), 30f);
                NAPI.World.DeleteWorldProp(1765048490, new Vector3(1855.685, 3683.93, 34.59282), 30f);
                NAPI.World.DeleteWorldProp(543652229, new Vector3(321.8085, 178.3599, 103.6782), 30f);
                NAPI.World.DeleteWorldProp(868499217, new Vector3(-818.7643, -1079.545, 11.47806), 30f);
                NAPI.World.DeleteWorldProp(3146141106, new Vector3(-816.7932, -1078.406, 11.47806), 30f);
                NAPI.World.DeleteWorldProp(543652229, new Vector3(-1155.454, -1424.008, 5.046147), 30f);
                NAPI.World.DeleteWorldProp(543652229, new Vector3(1321.286, -1650.597, 52.36629), 30f);

                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_247door_r"), new Vector3(1732.362, 6410.917, 35.18717), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_247door"), new Vector3(1730.032, 6412.072, 35.18717), 30f);

                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_gasdoor_r"), new Vector3(1698.172, 4928.146, 42.21359), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_gasdoor"), new Vector3(1699.661, 4930.278, 42.21359), 30f);
                // X:-59,89302 Y:-1092,952 Z:26,88362
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static List<Door> allDoors = new List<Door>();
        public static int RegisterDoor(int model, Vector3 Position)
        {
            allDoors.Add(new Door(model, Position));
            var col = NAPI.ColShape.CreateCylinderColShape(Position, 5, 5, 0);
            col.SetData("DoorID", allDoors.Count - 1);
            col.OnEntityEnterColShape += Door_onEntityEnterColShape;
            return allDoors.Count - 1;
        }

        private static void Door_onEntityEnterColShape(ColShape shape, Client entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Player) return;
                var door = allDoors[shape.GetData("DoorID")];
                Trigger.ClientEvent(entity, "setDoorLocked", door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, door.Angle);
            }
            catch (Exception e) { Log.Write("Door_onEntityEnterColshape: " + e.ToString(), nLog.Type.Error); }
        }

        public static void SetDoorLocked(int id, bool locked, float angle)
        {
            if (allDoors.Count < id + 1) return;
            allDoors[id].Locked = locked;
            allDoors[id].Angle = angle;
            Main.ClientEventToAll("setDoorLocked", allDoors[id].Model, allDoors[id].Position.X, allDoors[id].Position.Y, allDoors[id].Position.Z, allDoors[id].Locked, allDoors[id].Angle);
        }

        public static bool GetDoorLocked(int id)
        {
            if (allDoors.Count < id + 1) return false;
            return allDoors[id].Locked;
        }

        internal class Door
        {
            public Door(int model, Vector3 position)
            {
                Model = model;
                Position = position;
                Locked = false;
                Angle = 50.0f;
            }
            
            public int Model { get; set; }
            public Vector3 Position { get; set; }
            public bool Locked { get; set; }
            public float Angle { get; set; }
        }
    }
}
