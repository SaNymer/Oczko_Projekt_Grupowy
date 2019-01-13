using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // Players cards - connected to object on the screen
    public CardStack playerStack;
    public CardStack opponent1Stack;
    public CardStack opponent2Stack;
    public CardStack opponent3Stack;
    public CardStack deck; // Deck of shuffled cards from which players will drag cards

    private Dictionary<int, Player> players; // Information about players in the game

    // Buttons available on the screen
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Button saveAndExitButton;
    public Button exitButton;

    // Text object available on the screen
    public Text winnerText;
    public Text playerHandValueText;
    public Text opponent1HandValueText;
    public Text opponent2HandValueText;
    public Text opponent3HandValueText;
    public Text playerName;
    public Text opponent1Name;
    public Text opponent2Name;
    public Text opponent3Name;
    public Text scoreValue;

    private int score; // Player score
    private int playerTurn; // Actual player turn

    public void Awake()
    {
        SharedData.gameControl = this;
        playerTurn = 0;

        score = SharedData.thisPlayer.Score;
        opponent1Name.text = "Opponent1";
        opponent2Name.text = "Opponent2";
        opponent3Name.text = "Opponent3";

        CreatePlayers();

        playerName.text = SharedData.thisPlayer.Name;

        SharedData.gameControl = this;

        PrepareScreen();

        if (SharedData.isMultiplayerGame && !SharedData.isServer)
        {
            SharedData.clientControl.SendMsgToServer("deck:request;");
        }
    }

    void Start()
    {
        if (!SharedData.isMultiplayerGame)
        {
            StartGame();
        }
    }

    // Prepares screen objects for game
    void PrepareScreen()
    {
        Screen.SetResolution(1366, 600, false);

        if (SharedData.isMultiplayerGame)
        {
            saveAndExitButton.interactable = false;
            exitButton.interactable = false;

            if (!SharedData.isServer)
            {
                hitButton.interactable = false;
                stickButton.interactable = false;
            }
        }
    }

    // Preparing information about players 
    void CreatePlayers()
    {
        players = new Dictionary<int, Player>(SharedData.connectedPlayers);

        var playerNames = new List<Text> { playerName, opponent1Name, opponent2Name, opponent3Name };
        var playerStacks = new List<CardStack> { playerStack, opponent1Stack, opponent2Stack, opponent3Stack };
        var playerHandValues = new List<Text> { playerHandValueText, opponent1HandValueText, opponent2HandValueText, opponent3HandValueText };
        
        // Assigning players to their positions on the screen
        int i = SharedData.thisPlayer.Id;

        while (playerNames.Count > 0)
        {
            if (i > 3)
            {
                i = 0;
            }

            if (!players.ContainsKey(i))
            {
                players[i] = new Player() { Id = i };
                if (i != 0)
                {
                    players[i].IsComputerPlayer = true;
                }
            }

            players[i].Id = i;
            players[i].TextName = playerNames[0];
            playerNames.RemoveAt(0);
            players[i].Stack = playerStacks[0];
            playerStacks.RemoveAt(0);
            players[i].TextHandValue = playerHandValues[0];
            playerHandValues.RemoveAt(0);

            if (!players[i].IsComputerPlayer)
            {
                players[i].TextName.text = players[i].Name;
            }

            i++;
        }
    }

    // Generates information about deck to string - used by server to inform clients about deck look
    public string GenerateDeckInfo()
    {
        var deckInfo = "Deck:";

        foreach (var card in deck.GetCards())
        {
            deckInfo += card + "-";
        }
        deckInfo += ";";

        return deckInfo;
    }

    // Loads deck from the string - used by client
    public void LoadDeckFromServer(string deckString)
    {
        deck.Reset();
        var cardNums = deckString.Split('-').ToList();
        cardNums.RemoveAt(cardNums.Count - 1);

        foreach (var cardNum in cardNums)
        {
            deck.Push(int.Parse(cardNum));
        }
    }

    // Exit procedure from the game
    public void Exit()
    {
        if (!SharedData.isMultiplayerGame)
        {
            using (var writer = new StreamWriter("scores.txt", true))
            {
                writer.WriteLine(SharedData.thisPlayer.Name + "\t" + score);
            }
        }
        else
        {
            if (SharedData.isServer)
            {
                SharedData.serverConstrol.Disconnect();
            }
            else
            {
                SharedData.clientControl.Disconnect();
            }
        }

        SharedData.Clear();
        SceneManager.LoadScene("MainMenu");
    }

    // Save and exit procedure from the game
    public void SaveAndExit()
    {
        if (SharedData.isMultiplayerGame)
            return;

        using (var writer = new StreamWriter("savedGame.txt"))
        {
            writer.WriteLine(SharedData.thisPlayer.Name);
            writer.WriteLine(score);
        }

        SceneManager.LoadScene("MainMenu");
    }

    // Performs hit action for the local player
    public void Hit()
    {
        HitPlayer(SharedData.thisPlayer.Id);
        playerHandValueText.text = "Hand value: " + playerStack.HandValue;

        if (SharedData.isMultiplayerGame)
        {
            if (!SharedData.isServer)
            {
                SharedData.clientControl.SendHit(SharedData.thisPlayer.Id);
            }
        }
    }

    // Performs hit action for the player specified by the playerId. If sendHitInfo is true Server sends information to clients about hit action
    public void HitPlayer(int playerId, bool sendHitInfo = true)
    {
        if (SharedData.isServer && sendHitInfo)
        {
            SharedData.serverConstrol.SendHit(playerId);
        }

        int card = deck.Pop();
        players[playerId].Stack.Push(card);

        if (playerId == SharedData.thisPlayer.Id && players[playerId].Stack.HandValue > 21)
        {
            Stick();
        }
    }

    // Performs stick action for the player and gives next player control (or to the computer player)
    public void Stick()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;

        if (SharedData.isMultiplayerGame)
        {
            if (SharedData.isServer)
            {
                SharedData.serverConstrol.SendStick(SharedData.thisPlayer.Id);
            }
            else
            {
                SharedData.clientControl.SendStick(SharedData.thisPlayer.Id);
            }

            NextPlayerTurn();
        }
        else
        {
            playerTurn++;
            StartCoroutine(AITurn());
        }
    }

    // Gives next player turn - ends game if every player have sticked
    public void NextPlayerTurn()
    {
        playerTurn++;

        if (SharedData.thisPlayer.Id == playerTurn)
        {
            hitButton.interactable = true;
            stickButton.interactable = true;
        }

        if (playerTurn > 3)
        {
            playerTurn = 0;
            EndGame();
        }

        if (SharedData.isServer && players[playerTurn].IsComputerPlayer)
        {
            StartCoroutine(AITurn());
        }
    }

    // Restarts game 
    public void PlayAgain()
    {
        playAgainButton.interactable = false;
        hitButton.interactable = true;
        stickButton.interactable = true;
        winnerText.text = "";
        playerTurn = 0;

        foreach (var player in players)
        {
            player.Value.Stack.GetComponent<CardStackView>().Clear();
        }

        deck.CreateDeck();
        StartGame();
    }

    // Starts new game
    public void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                HitPlayer(j, false);
            }
        }

        playerHandValueText.text = "Hand value: " + playerStack.HandValue;
        opponent1HandValueText.text = "";
        opponent2HandValueText.text = "";
        opponent3HandValueText.text = "";

        scoreValue.text = "Score: " + score;
    }

    // Performs computer player turns and ends game after
    IEnumerator AITurn()
    {
        while (playerTurn <= 3)
        {
            yield return RunAI(players[playerTurn]);
        }

        EndGame();

        yield return new WaitForSeconds(1f);

        if (!SharedData.isMultiplayerGame)
        {
            playAgainButton.interactable = true;
        }
    }

    // Performs end game, showing information about winner and other player cards
    public void EndGame()
    {
        if (SharedData.isServer)
        {
            SharedData.serverConstrol.SendMsgToAllClients("game:end");
        }

        SetWinner();

        foreach (var player in players)
        {
            ShowPlayerCards(player.Value.Stack);
            player.Value.TextHandValue.text = "Hand value: " + player.Value.Stack.HandValue;
        }

        exitButton.interactable = true;
    }

    // Runs specific computer player turn
    IEnumerator RunAI(Player player)
    {
        while (player.Stack.HandValue < 21)
        {
            var chanceToHit = (int)Math.Pow(21 - player.Stack.HandValue, 2)*2 - 2;
            var rand = new System.Random();

            if (chanceToHit < rand.Next(0, 100) || chanceToHit <= 0)
                break;

            HitPlayer(player.Id);
            yield return new WaitForSeconds(1f);
        }
        
        if (SharedData.isServer)
        {
            SharedData.serverConstrol.SendStick(playerTurn);
        }
        playerTurn++;
    }

    // Shows player cards in his stack
    void ShowPlayerCards(CardStack player)
    {
        CardStackView view = player.GetComponent<CardStackView>();
        foreach (int card in player.GetCards())
        {
            view.Toggle(card, true);
        }
        view.ShowCards();
    }

    // Defines winner and prints result on the screen
    void SetWinner()
    {
        var handValues = new List<int>(){ opponent1Stack.HandValue, opponent2Stack.HandValue, opponent3Stack.HandValue };
        var validHandValues = new List<int>() { 0 }; // We add 0 so the list won't be ever empty and Max method won't crash
        for (int i = 0; i < handValues.Count; i++)
        {
            if (handValues[i] <= 21)
                validHandValues.Add(handValues[i]);
        }
        var maxHandValue = validHandValues.Max();

        if ((playerStack.WinStatus && (opponent1Stack.WinStatus || opponent2Stack.WinStatus || opponent3Stack.WinStatus)) || playerStack.HandValue == maxHandValue)
        {
            winnerText.text = "DRAW!";
        }
        else if (playerStack.WinStatus || (playerStack.HandValue > maxHandValue && playerStack.HandValue <= 21) || (playerStack.HandValue <= 21 && maxHandValue == 0))
        {
            winnerText.text = "You WON!";
            score++;
        }
        else
        {
            winnerText.text = "You LOST!";
            score--;
        }
    }
}
