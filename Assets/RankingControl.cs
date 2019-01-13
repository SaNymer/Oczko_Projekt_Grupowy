using Assets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingControl : MonoBehaviour
{
    public Text NamesText;
    public Text ScoresText;

    public void Awake()
    {
        var scores = new List<Player>();

        using (var reader = new StreamReader("scores.txt"))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var nameAndScore = line.Split('\t');
                scores.Add(new Player(nameAndScore[0], int.Parse(nameAndScore[1])));
            }
        }

        scores = scores.OrderByDescending(x => x.Score).ToList();

        for (int i = 1; i <= scores.Count && i <= 10; i++)
        {
            NamesText.text += i + ". " + scores[i-1].Name + "\n";
            ScoresText.text += scores[i-1].Score + "\n";
        }
    }

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
