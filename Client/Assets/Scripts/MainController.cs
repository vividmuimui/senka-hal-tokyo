using UnityEngine;

using WebSocketSharp;

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
    }

    void Update()
    {
    }

    void OnDestroy()
    {
        webSocket.Close();
    }
}
