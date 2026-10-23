using System.IO;
using UnityEngine;

// lit et écrit la sauvegarde dans un fichier JSON
public static class SaveSystem
{
    // dossier propre à l'appareil (sur Quest : stockage interne de l'application)
    static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    static SaveData data;

    // les données : chargées depuis le fichier la première fois qu'on les demande
    public static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public static void Load()
    {
        if (File.Exists(FilePath))
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));

        if (data == null) data = new SaveData();   // pas de fichier (ou fichier illisible) : on repart de zéro
    }

    public static void Save()
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));   // true = JSON lisible, avec retours à la ligne
    }

    // pour les tests et la démo
    public static void ResetAll()
    {
        data = new SaveData();
        Save();
    }

    // ajoute un score, trie du meilleur au moins bon et enregistre
    // ajoute un score, trie du meilleur au moins bon et enregistre
    public static void AddScore(string playerName, int score, string difficulty, string level)
    {
        Data.scores.Add(new ScoreEntry { playerName = playerName, score = score, difficulty = difficulty, level = level });
        Data.scores.Sort((a, b) => b.score.CompareTo(a.score));   // tri décroissant : le meilleur en premier
        if (Data.scores.Count > 200) Data.scores.RemoveRange(200, Data.scores.Count - 200);   // on garde les 200 meilleurs (tous niveaux confondus)
        Save();
    }

    // meilleur score enregistré pour un niveau et une difficulté (0 s'il n'y en a pas)
    public static int BestScore(string level, string difficulty)
    {
        int best = 0;
        foreach (ScoreEntry e in Data.scores)
            if (e.level == level && e.difficulty == difficulty) best = Mathf.Max(best, e.score);
        return best;
    }
}