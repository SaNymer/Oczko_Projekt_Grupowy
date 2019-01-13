using Assets;
using System.Collections.Generic;

public static class SharedData
{
    public static Player thisPlayer;
    public static Dictionary<int, Player> connectedPlayers = new Dictionary<int, Player>();
    public static bool isMultiplayerGame = false;
    public static bool isServer = false;
    public static ServerControl serverConstrol;
    public static ClientControl clientControl;
    public static GameController gameControl;

    public static void Clear()
    {
        thisPlayer = null;
        connectedPlayers = new Dictionary<int, Player>();
        isMultiplayerGame = false;
        isServer = false;
        serverConstrol = null;
        clientControl = null;
        gameControl = null;
    }
}
