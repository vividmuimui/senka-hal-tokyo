using UnityEngine;

using WebSocketSharp;

public class MainController : MonoBehaviour
{
    WebSocket webSocket;    // WebSocketコネクション

    void Start()
    {
        webSocket = new WebSocket("ws://localhost:5678");

        webSocket.Connect();
    }

    void Update()
    {
    }
}
