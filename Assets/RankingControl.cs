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
        var scores = new List<PlayerScore>();

        using (var reader = new StreamReader("scores.txt"))
        {
            int counter = 1;

            while (!reader.EndOfStream && counter <= 10)
            {
                var line = reader.ReadLine();
                var nameAndScore = line.Split('\t');
                scores.Add(new PlayerScore(nameAndScore[0], int.Parse(nameAndScore[1])));

                counter++;
            }
        }

        scores = scores.OrderByDescending(x => x.Score).ToList();

        for (int i = 1; i <= scores.Count; i++)
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
