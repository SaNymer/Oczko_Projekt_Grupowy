using Assets;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientControl : MonoBehaviour
{
    // Objects on the screen
    public Text infoMessage;
    public InputField inputIp;

    // Multiplayer configuration options
    int port = 9999;
    short messageID = 1000;
    NetworkClient client;

    // Prepares client
    public void Start()
    {
        var config = new ConnectionConfig();

        config.AddChannel(QosType.ReliableFragmented);
        config.AddChannel(QosType.UnreliableFragmented);

        client = new NetworkClient();
        client.Configure(config, 1);

        RegisterHandlers();
    }

    // Closes connection when application is closed
    void OnApplicationQuit()
    {
        client.Shutdown();
    }

    public void Disconnect()
    {
        client.Shutdown();
    }

    // Connects client to the server
    public void ConnectClient()
    {
        var ip = inputIp.text;
        client.Connect(ip, port);

        if (!client.isConnected)
        {
            infoMessage.text = "Failed to connect to server!";
        }
    }

    // Register handlers to listen information send between server and clients
    void RegisterHandlers()
    {
        client.RegisterHandler(messageID, OnMessageReceived);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
    }

    // Event performed when client connects to server. Informs player about his and shows information about new connected player on the screen
    void OnConnected(NetworkMessage message)
    {
        var msg = "Name:" + SharedData.thisPlayer.Name + ";";
        SendMsgToServer(msg);

        infoMessage.text = "Connected to server!\nWaiting for server to start game";
    }

    // Event performed when client disconnects from the server or the connection is lost
    void OnDisconnected(NetworkMessage message)
    {
        infoMessage.text = "Lost connection to server!";
    }

    // Event performed when message is received. Messages should be send using template 'msgType:msgInfo;'
    void OnMessageReceived(NetworkMessage netMessage)
    {
        var objectMessage = netMessage.ReadMessage<MyNetworkMessage>();
        var splitMessage = objectMessage.message.Split(';');

        foreach (var msg in splitMessage)
        {
            ReactOnMessage(netMessage.conn.connectionId, msg);
        }
    }

    // Performs action to the message type
    void ReactOnMessage(int clientId, string msg)
    {
        if (msg.IndexOf(':') <= 0)
            return;

        var msgType = msg.Substring(0, msg.IndexOf(':')).ToLower();
        var msgInfo = msg.Substring(msg.IndexOf(':') + 1);

        switch (msgType)
        {
            case "id": // Sets this player id 
                {
                    SharedData.thisPlayer.Id = int.Parse(msgInfo);
                    SharedData.connectedPlayers[SharedData.thisPlayer.Id] = new Player(name) { Id = SharedData.thisPlayer.Id };
                    break;
                }

            case "player": // Saves information about other players in the game
                {
                    var id = msgInfo.Substring(0, 1);
                    var name = msgInfo.Substring(1);
                    SharedData.connectedPlayers[int.Parse(id)] = new Player(name) { Id = int.Parse(id) };
                    break;
                }

            case "deck": // Loads information about deck in the game
                {
                    SharedData.gameControl.LoadDeckFromServer(msgInfo);
                    SharedData.gameControl.StartGame();
                    break;
                }

            case "game": // Starts or ends game
                {
                    if (msgInfo == "start")
                    {
                        SharedData.isMultiplayerGame = true;
                        SharedData.clientControl = this;
                        SceneManager.LoadScene("BlackJackGame");
                    }
                    else if (msgInfo == "end")
                    {
                        SharedData.gameControl.EndGame();
                    }
                    break;
                }
            case "hit": // Performs hit for specific player
                {
                    var id = int.Parse(msgInfo);
                    SharedData.gameControl.HitPlayer(id);
                    break;
                }
            case "stick": // Performs stick for specific player 
                {
                    if (int.Parse(msgInfo) != SharedData.thisPlayer.Id)
                    {
                        SharedData.gameControl.NextPlayerTurn();
                    }
                    break;
                }
        }
    }

    // Sends message to the server
    public void SendMsgToServer(string msg)
    {
        var messageContainer = new MyNetworkMessage();
        messageContainer.message = msg;
        client.Send(messageID, messageContainer);
    }

    // Sends hit message to the server
    public void SendHit(int clientId)
    {
        SendMsgToServer("hit:" + clientId);
    }

    // Sends stick message to the server
    public void SendStick(int clientId)
    {
        SendMsgToServer("stick:" + clientId);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}