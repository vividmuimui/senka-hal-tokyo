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
        public static void Main(string[] args)
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

            var gameServer = GameServer.GetInstance(address);
            gameServer.RunForever();
        }
    }

    public class GameServer
    {
        const string DEFAULT_ADDRESS = "ws//localhost:5678";
        const string SERVICE_NAME = "/";
        const ConsoleKey EXIT_KEY = ConsoleKey.Q;

        static GameServer instance;

        public WebSocketServer WebSocketServer;

        Dictionary<int, Player> players = new Dictionary<int, Player>();
        UIDGenerator uidGenerator = new UIDGenerator();

        static public GameServer GetInstance(string address = DEFAULT_ADDRESS)
        {
            if (instance == null)
            {
                instance = new GameServer(address);
            }
            return instance;
        }

        GameServer(string address)
        {
            WebSocketServer = new WebSocketServer(address);
            WebSocketServer.AddWebSocketService<WebSocketSampleService>(SERVICE_NAME);
        }

        public void RunForever()
        {
            WebSocketServer.Start();
            Console.WriteLine("Game Server started.");

            try
            {
                while (true)
                {
                    PollKey();
                    Sync();
                }
            }
            catch (GameExit ex)
            {
            }
            catch (Exception ex)
            {
            }

            WebSocketServer.Stop();
            Console.WriteLine("Game Server terminated.");
        }

        void PollKey()
        {
            if (!Console.KeyAvailable) return;

            switch (Console.ReadKey(true).Key)
            {
                case EXIT_KEY:
                    throw new GameExit();

                default:
                    Console.WriteLine("Enter " + EXIT_KEY + " to exit the game.");
                    break;
            }
        }

        void Sync()
        {
            if (players.Count == 0) return;

            var movedPlayers = new List<RPC.Player>();
            foreach (var kv in players)
            {
                var player = kv.Value;
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

        public void Ping(string senderId, MessageEventArgs e)
        {
            Console.WriteLine(">> Ping");

            var pingRpc = new Ping(new PingPayload("pong"));
            var pingJson = JsonConvert.SerializeObject(pingRpc);
            SendTo(senderId, pingJson);

            Console.WriteLine("<< Pong");
        }

        public void Login(string senderId, MessageEventArgs e)
        {
            Console.WriteLine(">> Login");

            var login = JsonConvert.DeserializeObject<Login>(e.Data);

            var player = new Player(uidGenerator.Generate(), login.Payload.Name, new Position(0f, 0f, 0f));
            players[player.Uid] = player;

            var loginResponseRpc = new LoginResponse(new LoginResponsePayload(player.Uid));
            var loginResponseJson = JsonConvert.SerializeObject(loginResponseRpc);
            SendTo(senderId, loginResponseJson);

            Console.WriteLine(player.ToString() + " login.");
        }

        public void PlayerUpdate(string senderId, MessageEventArgs e)
        {
            Console.WriteLine(">> PlayerUpdate");

            var playerUpdate = JsonConvert.DeserializeObject<PlayerUpdate>(e.Data);

            Player player;
            if (players.TryGetValue(playerUpdate.Payload.Id, out player))
            {
                player.SetPosition(playerUpdate.Payload.Position);
            }
        }

        void SendTo(string id, string message)
        {
            WebSocketServer.WebSocketServices[SERVICE_NAME].Sessions.SendTo(message, id);

            Console.WriteLine("<< SendTo: " + id + " " + message);
        }

        void Broadcast(string message)
        {
            WebSocketServer.WebSocketServices[SERVICE_NAME].Sessions.Broadcast(message);

            Console.WriteLine("<< Broeadcast: " + message);
        }
    }

    class UIDGenerator
    {
        int counter = 0;

        public int Generate()
        {
            counter++;
            return counter;
        }
    }

    public class WebSocketSampleService : WebSocketBehavior
    {
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

            try
            {
                DispatchMethod(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("WebSocket Error: " + e);
        }

        // メソッドディスパッチ
        void DispatchMethod(MessageEventArgs e)
        {
            // ヘッダ解析
            var header = JsonConvert.DeserializeObject<Header>(e.Data);
            Console.WriteLine("Header: " + header.Method);

            var gameServer = GameServer.GetInstance();
            var senderId = ID;

            switch (header.Method)
            {
                case "ping":
                    {
                        gameServer.Ping(senderId, e);
                        break;
                    }
                case "login":
                    {
                        gameServer.Login(senderId, e);
                        break;
                    }
                case "player_update":
                    {
                        gameServer.PlayerUpdate(senderId, e);
                        break;
                    }
            }
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

    enum Status
    {
        OK = 0,
        GameExit
    };

    class GameException : Exception
    {
        Status code;

        public GameException(string message, Status code = Status.OK) : base(message)
        {
            this.code = code;
        }
    }

    class GameExit : GameException
    {
        public GameExit() : base("Game exit.", Status.GameExit) { }
    }
}
