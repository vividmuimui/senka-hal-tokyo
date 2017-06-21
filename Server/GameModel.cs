using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSample.RPC;

namespace WebSocketSample.Server
{
    public class GameModel
    {
        Dictionary<int, Player> players = new Dictionary<int, Player>();
        int uidCounter;

        GameServer server;

        public GameModel(GameServer server)
        {
            this.server = server;
            server.OnUpdate += Sync;
        }

        public void SubscribeServiceEvent(GameService service)
        {
            service.OnPing += OnPing;
            service.OnLogin += OnLogin;
            service.OnPlayerUpdate += OnPlayerUpdate;
        }

        void OnPing(string senderId)
        {
            Console.WriteLine(">> Ping");

            var pingRpc = new Ping(new PingPayload("pong"));
            var pingJson = JsonConvert.SerializeObject(pingRpc);
            server.SendTo(pingJson, senderId);

            Console.WriteLine("<< Pong");
        }

        void OnLogin(string senderId, LoginPayload loginPayload)
        {
            Console.WriteLine(">> Login");

            var player = new Player(uidCounter++, loginPayload.Name, new Position(0f, 0f, 0f));
            players[player.Uid] = player;

            var loginResponseRpc = new LoginResponse(new LoginResponsePayload(player.Uid));
            var loginResponseJson = JsonConvert.SerializeObject(loginResponseRpc);
            server.SendTo(loginResponseJson, senderId);

            Console.WriteLine(player.ToString() + " login.");
        }

        void OnPlayerUpdate(string senderId, PlayerUpdatePayload playerUpdatePayload)
        {
            Console.WriteLine(">> PlayerUpdate");

            Player player;
            if (players.TryGetValue(playerUpdatePayload.Id, out player))
            {
                player.SetPosition(playerUpdatePayload.Position);
            }
        }

        void Sync()
        {
            if (players.Count == 0) return;

            var movedPlayers = new List<RPC.Player>();
            foreach (var player in players.Values)
            {
                if (!player.isPositionChanged) continue;

                var playerRpc = new RPC.Player(player.Uid, player.Position);
                movedPlayers.Add(playerRpc);
                player.isPositionChanged = false;
            }

            if (movedPlayers.Count == 0) return;

            var syncRpc = new Sync(new SyncPayload(movedPlayers));
            var syncJson = JsonConvert.SerializeObject(syncRpc);
            server.Broadcast(syncJson);
        }
    }
}