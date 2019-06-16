using GTANetworkAPI;
using System.Collections.Generic;

namespace NeptuneEvo.Voice
{
    class Room
    {
        public string Name;
        public List<Client> Players;

        public Dictionary<string, object> MetaData { get { return new Dictionary<string, object> { { "name", Name } }; } }

        public Room(string Name)
        {
            this.Name = Name;

            this.Players = new List<Client>();
        }



        public void OnJoin(Client player)
        {
            if (Players.Contains(player))
            {
                var argsMe = new List<object> { MetaData };
                Players.ForEach(_player => argsMe.Add(_player));


                Trigger.ClientEvent(player, "voice.radioConnect", argsMe.ToArray());
                Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioConnect", MetaData, player);

                player.GetData("Voip").RadioRoom = Name;
                Players.Add(player);
            }
        }

        public void OnQuit(Client player)
        {
            if (Players.Contains(player))
            {
                var argsMe = new List<object> { MetaData };
                Players.ForEach(_player => argsMe.Add(_player));

                Trigger.ClientEvent(player, "voice.radioDisconnect", argsMe.ToArray());
                Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioDisconnect", MetaData, player);

                player.GetData("Voip").RadioRoom = "";
                Players.Remove(player);
            }
        }

        public void OnRemove()
        {
            Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioDisconnect", MetaData);
            Players.Clear();
        }
    }
}