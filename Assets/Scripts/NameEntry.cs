using TMPro;
using UnityEngine;
using UnityEngine.UI;

// panneau de fin de partie : saisir son nom et enregistrer son score
public class NameEntry : MonoBehaviour
{
    public static NameEntry Instance;

    public GameObject panel;   // le panneau sur le pupitre
    public TMP_InputField nameInput;  // le champ où s'écrit le nom
    public Button saveButton; // bouton SAVE
    public TextMeshProUGUI infoText; // "Score: 85 (Normal)" puis "Saved!"
    public int maxLength = 12;    // longueur max du nom

    int pendingScore;
    string pendingDifficulty;
    bool saved;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    // appelée par LevelManager à la fin d'une partie
    public void Open(int score, string difficulty)
    {
        pendingScore = score;
        pendingDifficulty = difficulty;
        saved = false;

        if (panel != null) panel.SetActive(true);
        if (nameInput != null) nameInput.text = SaveSystem.Data.lastName;  // on propose le dernier nom
        if (saveButton != null) saveButton.interactable = true;
        if (infoText != null) infoText.text = "Score: " + score + " (" + difficulty + ")";
    }

    // appelée par le bouton SAVE
    public void Save()
    {
        if (saved) return; // un seul enregistrement par partie

        string playerName = nameInput != null ? nameInput.text.Trim() : "";
        if (playerName.Length == 0) playerName = "Player";
        if (playerName.Length > maxLength) playerName = playerName.Substring(0, maxLength);

        SaveSystem.Data.lastName = playerName;
        SaveSystem.AddScore(playerName, pendingScore, pendingDifficulty);

        saved = true;
        if (saveButton != null) saveButton.interactable = false;
        if (infoText != null) infoText.text = "Saved!";
        if (Leaderboard.Instance != null) Leaderboard.Instance.Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}