using System;
using WebSocketSharp.Server;

namespace WebSocketSample.Server
{
    public class GameServer
    {
        const string SERVICE_NAME = "/";
        const ConsoleKey EXIT_KEY = ConsoleKey.Q;

        public event Action OnUpdate = () => { };

        WebSocketServer WebSocketServer;

        public GameServer(string address)
        {
            WebSocketServer = new WebSocketServer(address);
            WebSocketServer.AddWebSocketService<GameService>(SERVICE_NAME, () => new GameService(this));
        }

        public void RunForever()
        {
            WebSocketServer.Start();
            Console.WriteLine("Game Server started.");

            while (!IsInputtedExitKey())
            {
                OnUpdate();
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
}