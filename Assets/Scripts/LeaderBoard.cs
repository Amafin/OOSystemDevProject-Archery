using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// affiche les meilleurs scores, du meilleur au moins bon
public class Leaderboard : MonoBehaviour
{
    public static Leaderboard Instance;

    public TextMeshProUGUI listText; // le texte de la tablette
    public int maxLines = 10;  // nombre de lignes affichées

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (listText == null) return;

        // copie triée : le classement reste juste même si le fichier a été modifié à la main
        List<ScoreEntry> sorted = new List<ScoreEntry>(SaveSystem.Data.scores);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));

        StringBuilder sb = new StringBuilder();
        int count = Mathf.Min(maxLines, sorted.Count);
        for (int i = 0; i < count; i++)
        {
            ScoreEntry e = sorted[i];
            sb.AppendLine((i + 1) + ". " + e.playerName + "   " + e.score + "   " + e.difficulty);
        }
        if (count == 0) sb.Append("No scores yet");

        listText.text = sb.ToString();
    }
}