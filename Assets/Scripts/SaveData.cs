using System.Collections.Generic;

// une ligne du classement
[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public string difficulty;
}

// tout ce qui est sauvegardé dans le fichier JSON
[System.Serializable]
public class SaveData
{
    public int unlockedLevel = 0;  // niveau de progression (0 = début)
    public List<ScoreEntry> scores = new List<ScoreEntry>(); // le classement
    public float volume = 1f;   // volume général
}