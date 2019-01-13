using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace Assets
{
    public class Player
    {
        public string Name { get; set; } // Player name
        public int Score { get; set; } // Player score
        // Multiplayer attributes
        public int Id { get; set; } // Player Id
        public CardStack Stack { get; set; } // Player cards
        public Text TextName { get; set; } // Object in the screen that shows player name
        public Text TextHandValue { get; set; } // Object in the screen that shows player hand value
        public bool IsComputerPlayer { get; set; } // If it's computer player

        public Player()
        {
            IsComputerPlayer = false;
        }

        public Player(string name) : base()
        {
            Name = name;
        }

        public Player(string name, int score) : base()
        {
            Name = name;
            Score = score;
        }
    }
}
