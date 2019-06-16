using GTANetworkAPI;
using System.Collections.Generic;
using System;

namespace NeptuneEvo.Voice
{
    class RoomController
    {
        public Dictionary<string, Room> Rooms;

        private static RoomController instance;

        private RoomController()
        {
            Rooms = new Dictionary<string, Room>();
        }

        public static RoomController getInstance()
        {
            if (instance == null)
            {
                instance = new RoomController();
            }

            return instance;
        }

        public void CreateRoom(string name)
        {
            if (!Rooms.ContainsKey(name))
            {
                Rooms.Add(name, new Room(name));

                Console.WriteLine("Room " + name + " created");
            }
        }

        public void RemoveRoom(string name)
        {
            if (Rooms.ContainsKey(name))
            {
                Room room = Rooms[name];

                room.OnRemove();
                Rooms.Remove(name);

                Console.WriteLine("Room " + name + " removed");
            }
        }

        public bool HasRoom(string name)
        {
            return Rooms.ContainsKey(name);
        }

        public void OnJoin(string name, Client player)
        {
            if (Rooms.ContainsKey(name))
            {
                Room room = Rooms[name];

                room.OnJoin(player);
            }
        }

        public void OnQuit(string name, Client player)
        {
            if (Rooms.ContainsKey(name))
            {
                Room room = Rooms[name];

                room.OnQuit(player);
            }
        }
    }

}