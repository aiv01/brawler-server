using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlerServer.Server
{
    public class Room
    {
        public int Id { get; private set; }
        public List<Client> Clients { get; private set; }
        public byte MaxPlayers { get; private set; }

        public Room(byte MaxPlayers)
        {
            Id = Utilities.Utilities.GetRoomId();
            Clients = new List<Client>();
            this.MaxPlayers = MaxPlayers;
        }

        public void AddClient(Client client)
        {
            Clients.Add(client);
        }

        public void RemoveClient(Client client)
        {
            Clients.Remove(client);
        }
    }
}
