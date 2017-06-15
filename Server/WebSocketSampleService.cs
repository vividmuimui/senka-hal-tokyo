using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using WebSocketSample.RPC;

namespace WebSocketSample.Server
{
    public class WebSocketSampleService : WebSocketBehavior
    {
        Dictionary<int, Player> players = new Dictionary<int, Player>();
        static int uidCounter;

        public WebSocketSampleService(GameServer gameServer)
        {
            gameServer.OnUpdate += Sync;
        }

        protected override void OnOpen()
        {
            Console.WriteLine("WebSocket opened.");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("WebSocket Close.");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("WebSocket Message: " + e.Data);

            var header = JsonConvert.DeserializeObject<Header>(e.Data);
            Console.WriteLine("Header: " + header.Method);

            switch (header.Method)
            {
                case "ping":
                    {
                        OnPing();
                        break;
                    }
                case "login":
                    {
                        var loginPayload = JsonConvert.DeserializeObject<Login>(e.Data).Payload;
                        OnLogin(loginPayload);
                        break;
                    }
                case "player_update":
                    {
                        var playerUpdatePayload = JsonConvert.DeserializeObject<PlayerUpdate>(e.Data).Payload;
                        OnPlayerUpdate(playerUpdatePayload);
                        break;
                    }
            }
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("WebSocket Error: " + e);
        }

        void SendTo(string message)
        {
            Sessions.SendTo(message, ID);
            Console.WriteLine("<< SendTo: " + ID + " " + message);
        }

        void Broadcast(string message)
        {
            Sessions.Broadcast(message);
            Console.WriteLine("<< Broeadcast: " + message);
        }

        public void OnPing()
        {
            Console.WriteLine(">> Ping");

            var pingRpc = new Ping(new PingPayload("pong"));
            var pingJson = JsonConvert.SerializeObject(pingRpc);
            SendTo(pingJson);

            Console.WriteLine("<< Pong");
        }

        public void OnLogin(LoginPayload loginPayload)
        {
            Console.WriteLine(">> Login");

            var player = new Player(uidCounter++, loginPayload.Name, new Position(0f, 0f, 0f));
            players[player.Uid] = player;

            var loginResponseRpc = new LoginResponse(new LoginResponsePayload(player.Uid));
            var loginResponseJson = JsonConvert.SerializeObject(loginResponseRpc);
            SendTo(loginResponseJson);

            Console.WriteLine(player.ToString() + " login.");
        }

        public void OnPlayerUpdate(PlayerUpdatePayload playerUpdatePayload)
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
            Broadcast(syncJson);
        }
    }
}