using UnityEngine;

using WebSocketSharp;
using RPC = WebSocketSample.RPC;

public class MainController : MonoBehaviour
{
    WebSocket webSocket;    // WebSocketコネクション

    void Start()
    {
        webSocket = new WebSocket("ws://localhost:5678");

        // コネクション確立したときのハンドラ
        webSocket.OnOpen += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Opened");
        };

        // エラーが発生したときのハンドラ
        webSocket.OnError += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Error Message: " + eventArgs.Message);
        };

        // コネクションを閉じたときのハンドラ
        webSocket.OnClose += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Closed");
        };

        // メッセージを受信したときのハンドラ
        webSocket.OnMessage += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Message: " + eventArgs.Data);
        };

        webSocket.Connect();

        webSocket.Send(JsonUtility.ToJson(new RPC.Ping(new RPC.PingPayload("ping"))));
    }

    void Update()
    {
    }

    void OnDestroy()
    {
        webSocket.Close();
    }
}
