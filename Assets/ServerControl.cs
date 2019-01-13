using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Assets;

public class ServerControl : MonoBehaviour
{
    // Text object available on the screen
    public Text connectedPlayersText;

    // Multiplayer configuration options
    int port = 9999;
    int maxConnections = 3;
    short messageID = 1000;

    int clientsGamesStarted = 0; // Used to know when all players are ready to start the game

    // Prepares server
    void Start()
    {
        Application.runInBackground = true;
        CreateServer();
    }

    // Configures and creates server
    void CreateServer()
    {
        RegisterHandlers();

        var config = new ConnectionConfig();
        config.AddChannel(QosType.ReliableFragmented);
        config.AddChannel(QosType.UnreliableFragmented);

        var ht = new HostTopology(config, maxConnections);

        if (!NetworkServer.Configure(ht))
        {
            return;
        }
        else
        {
            if (NetworkServer.Listen(port))
                Debug.Log("Server created, listening on port: " + port);
            else
                Debug.Log("No server created, could not listen to the port: " + port);
        }
    }

    // Closes connection when application is closed
    void OnApplicationQuit()
    {
        NetworkServer.Shutdown();
    }

    public void Disconnect()
    {
        NetworkServer.Shutdown();
    }

    // Register handlers to listen information send between server and clients
    private void RegisterHandlers()
    {
        NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

        NetworkServer.RegisterHandler(messageID, OnMessageReceived);
    }

    // Event performed when client connects to server. Informs player about his and shows information about new connected player on the screen
    void OnClientConnected(NetworkMessage netMessage)
    {
        var id = netMessage.conn.connectionId;
        MyNetworkMessage messageContainer = new MyNetworkMessage();
        messageContainer.message = "id:" + id + ";player:0" + SharedData.thisPlayer.Name + ";";

        NetworkServer.SendToClient(netMessage.conn.connectionId, messageID, messageContainer);

        SharedData.connectedPlayers[id] = new Player("NoName");
        ShowConnectedPlayers();
    }

    // Event performed when client disconnects - removes from connected player collection and updates information on the screen
    void OnClientDisconnected(NetworkMessage netMessage)
    {
        SharedData.connectedPlayers.Remove(netMessage.conn.connectionId);
        ShowConnectedPlayers();
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

    // Show connected players on the screen
    void ShowConnectedPlayers()
    {
        string text = "";

        foreach (var player in SharedData.connectedPlayers)
        {
            text += "ID: " + player.Key + " Name: " + player.Value.Name + "\n";
        }

        connectedPlayersText.text = text;
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
            case "name": // Sets name of the connected player
                {
                    var name = msg.Substring(msgType.Length + 1);
                    SharedData.connectedPlayers[clientId] = new Player(name);
                    ShowConnectedPlayers();
                    break;
                }
            case "deck": // Sends information to client about deck and sends information to start game when all players connected
                {
                    if (msgInfo == "request")
                    {
                        var deckInfo = SharedData.gameControl.GenerateDeckInfo();
                        SendMsgToClient(clientId, deckInfo);
                        clientsGamesStarted++;
                    }
                    if (clientsGamesStarted >= SharedData.connectedPlayers.Count)
                    {
                        SharedData.gameControl.StartGame();
                    }
                    break;
                }
            case "hit": // Sends information about hit done by player
                {
                    var id = int.Parse(msgInfo);
                    if (id != SharedData.thisPlayer.Id)
                    {
                        SharedData.gameControl.HitPlayer(id, false);
                    }
                    break;
                }
            case "stick": // Sends information about stick done by player
                {
                    SharedData.gameControl.NextPlayerTurn();
                    SendStick(int.Parse(msgInfo));
                    break;
                }
        }
    }

    // Switches to the screen with game and informs connected players to do the same
    public void StartGame()
    {
        foreach (var player in SharedData.connectedPlayers)
        {
            var msg = "player:" + player.Key + player.Value.Name + ";";
            SendMsgToAllClients(msg);
        }

        SendMsgToAllClients("game:start;");

        SharedData.isMultiplayerGame = true;
        SharedData.isServer = true;
        SharedData.thisPlayer.Id = 0;
        SharedData.serverConstrol = this;

        SceneManager.LoadScene("BlackJackGame");
    }

    // Sends message to all clients
    public void SendMsgToAllClients(string msg)
    {
        var messageContainer = new MyNetworkMessage();
        messageContainer.message = msg;
        NetworkServer.SendToAll(messageID, messageContainer);
    }

    // Sends message to specific client defined by clientId
    public void SendMsgToClient(int clientId, string msg)
    {
        var messageContainer = new MyNetworkMessage();
        messageContainer.message = msg;
        NetworkServer.SendToClient(clientId, messageID, messageContainer);
    }

    // Sends hit message to all clients
    public void SendHit(int clientId)
    {
        SendMsgToAllClients("hit:" + clientId);
    }

    // Sends stick message to all clients
    public void SendStick(int clientId)
    {
        SendMsgToAllClients("stick:" + clientId);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}