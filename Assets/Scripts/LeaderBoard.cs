using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// affiche les meilleurs scores d'un niveau, du meilleur au moins bon
public class Leaderboard : MonoBehaviour
{
    public static Leaderboard Instance;

    public TextMeshProUGUI titleText;  // "Leaderboard - Initiation"
    public TextMeshProUGUI listText; // le texte de la tablette
    public int maxLines = 10;  // nombre de lignes affichées
    public GameObject panel;   // la tablette entière (LeaderboardPanel)

    string currentLevel;   // niveau affiché (null = tous les niveaux)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Refresh();
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
        Refresh();   // à jour à chaque affichage
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    // change le niveau affiché (appelée par le menu et en fin de partie)
    public void SetLevel(string level)
    {
        currentLevel = level;
        Refresh();
    }

    public void Refresh()
    {
        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(currentLevel) ? "Leaderboard" : "Leaderboard - " + currentLevel;

        if (listText == null) return;

        // on ne garde que les scores du niveau affiché
        List<ScoreEntry> sorted = new List<ScoreEntry>();
        foreach (ScoreEntry e in SaveSystem.Data.scores)
            if (string.IsNullOrEmpty(currentLevel) || e.level == currentLevel) sorted.Add(e);

        // copie triée : le classement reste juste même si le fichier a été modifié à la main
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