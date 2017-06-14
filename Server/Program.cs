using System;
using System.Collections.Generic;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using WebSocketSample.RPC;

namespace WebSocketSample.Server
{
    class MainClass
    {
        static void Main()
        {
            IPAddress ipv4 = null;
            foreach (var ipAddress in Dns.GetHostAddresses(""))
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipv4 = ipAddress;
                    break;
                }
            }

            var port = 5678;
            var address = string.Format("ws://{0}:{1}", ipv4.ToString(), port);
            Console.WriteLine(address);

            var gameServer = new GameServer(address);
            gameServer.RunForever();
        }
    }

    public class GameServer
    {
        const string SERVICE_NAME = "/";
        const ConsoleKey EXIT_KEY = ConsoleKey.Q;

        public WebSocketServer WebSocketServer;

        public GameServer(string address)
        {
            WebSocketServer = new WebSocketServer(address);
            WebSocketServer.AddWebSocketService<WebSocketSampleService>(SERVICE_NAME);
        }

        public void RunForever()
        {
            WebSocketServer.Start();
            Console.WriteLine("Game Server started.");

            while (!IsInputtedExitKey())
            {
                Sync();
            }
        }

        bool IsInputtedExitKey()
        {
            if (!Console.KeyAvailable) { return false; }

            switch (Console.ReadKey(true).Key)
            {
                default:
                    Console.WriteLine("Enter " + EXIT_KEY + " to exit the game.");
                    return false;

                case EXIT_KEY:
                    WebSocketServer.Stop();
                    Console.WriteLine("Game Server terminated.");
                    return true;
            }
        }
    }

    public class WebSocketSampleService : WebSocketBehavior
    {
        Dictionary<int, Player> players = new Dictionary<int, Player>();
        static int uidCounter;

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

    class Player
    {
        public readonly int Uid;
        public readonly string Name;
        public Position Position;
        public bool isPositionChanged;

        public Player(int uid, string name, Position position)
        {
            Uid = uid;
            Name = name;
            Position = position;
        }

        public void SetPosition(Position position)
        {
            if (Position.X != position.X || Position.Y != position.Y || Position.Z != position.Z)
            {
                Position = position;
                isPositionChanged = true;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "<Player(uid={0}, name={1}, x={2}, y={3}, z={4})>",
                Uid,
                Name,
                Position.X, Position.Y, Position.Z
            );
        }
    }
}
