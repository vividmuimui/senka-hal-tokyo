using System.Collections.Generic;

namespace WebSocketSample.RPC
{
    [System.Serializable]
    public class Header
    {
        public string Method;
    }

    [System.Serializable]
    public class Position
    {
        public float X;
        public float Y;
        public float Z;

        public Position(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    [System.Serializable]
    public class PlayerUpdate
    {
        public string Method = "player_update";
        public PlayerUpdatePayload Payload;

        public PlayerUpdate(PlayerUpdatePayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class PlayerUpdatePayload
    {
        public int Id;
        public Position Position;

        public PlayerUpdatePayload(int id, Position position)
        {
            this.Id = id;
            this.Position = position;
        }
    }

    [System.Serializable]
    public class Login
    {
        public string Method = "login";
        public LoginPayload Payload;

        public Login(LoginPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class LoginPayload
    {
        public int Id;
        public string Name;

        public LoginPayload(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }

    [System.Serializable]
    public class Sync
    {
        public string Method = "sync";
        public SyncPayload Payload;

        public Sync(SyncPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class SyncPayload
    {
        public List<Player> Players;

        public SyncPayload(List<Player> players)
        {
            this.Players = players;
        }
    }

    [System.Serializable]
    public class Player
    {
        public int Id;
        public Position Position;

        public Player(int id, Position position)
        {
            this.Id = id;
            this.Position = position;
        }
    }

    [System.Serializable]
    public class Register
    {
        public string Method = "register";
        public RegisterPayload Payload;

        public Register(RegisterPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class RegisterPayload
    {
        public string Name;

        public RegisterPayload(string name)
        {
            this.Name = name;
        }
    }

    [System.Serializable]
    public class RegisterResponse
    {
        public string Method = "register_response";
        public RegisterResponsePayload Payload;

        public RegisterResponse(int id)
        {
            this.Payload = new RegisterResponsePayload(id);
        }
    }

    [System.Serializable]
    public class RegisterResponsePayload
    {
        public int Id;

        public RegisterResponsePayload(int id)
        {
            this.Id = id;
        }
    }

    [System.Serializable]
    public class Ping
    {
        public string Method = "ping";
        public PingPayload Payload;

        public Ping(PingPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class PingPayload
    {
        public string Message;

        public PingPayload(string message)
        {
            this.Message = message;
        }
    }
}
