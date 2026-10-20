using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // accessible partout : LevelManager.Instance
    public float duration = 60f; // durée du niveau en secondes
    public TextMeshProUGUI timerText; // texte du timer
    private float timeLeft; // temps restant en secondes
    public bool IsRunning { get; private set; } // indique si le niveau est en cours
    public GameObject resultPanel;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;
    int bestScore = 0;
    public DifficultySettings difficulty; // les paramètres de difficulté du niveau

    public TextMeshProUGUI unlockText; // "Nouvel équipement : ..." dans le panneau de résultats
    public bool IsTraining => difficulty != null && difficulty.isTraining;  // vrai si la difficulté choisie est le mode entraînement

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        if (resultPanel != null) resultPanel.SetActive(false);   // caché pendant la partie

        // On applique les paramètres de difficulté
        if (difficulty != null)
        {
            duration = difficulty.levelDuration;
            ScoreManager.Instance.SetTiers(difficulty.tiers);
            if (TargetSpawner.Instance != null) TargetSpawner.Instance.ApplyDifficulty(difficulty);
            if (GladiatorSpawner.Instance != null) GladiatorSpawner.Instance.ApplyDifficulty(difficulty);
        }
        timeLeft = duration;
        ScoreManager.Instance.ResetScore();
        if (PlayerHealth.Instance != null) PlayerHealth.Instance.ResetHealth();
        IsRunning = true;

        if (TargetSpawner.Instance != null) TargetSpawner.Instance.SpawnAll();
        if (GladiatorSpawner.Instance != null) GladiatorSpawner.Instance.StartSpawning();
        if (IsTraining && timerText != null) timerText.text = "Entraînement"; // pas de timer en mode entraînement
        }

    private void Update()
    {
        if (IsRunning && !IsTraining)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                IsRunning = false;
                EndLevel();
            }
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(timeLeft);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = minutes + ":" + seconds.ToString("00");
    }

    void EndLevel(bool defeated = false)
    {
        int tier = ScoreManager.Instance.TierReached;
        int score = ScoreManager.Instance.Score;

        if (score > bestScore)
        {
            bestScore = score;
        }

        if (resultText != null)
        {
            if (defeated) resultText.text = "Vaincu !";
            else resultText.text = tier > 0 ? "Objectif réussi ! Palier " + tier : "Objectif raté";
        }
        if (scoreText != null)
            scoreText.text = "Score : " + score;
        if (bestText != null)
            bestText.text = "Meilleur : " + bestScore;

        // objectif atteint sans être vaincu : on débloque l'équipement suivant
        string unlocked = null;
        if (!defeated && tier > 0 && !IsTraining && EquipmentManager.Instance != null)
            unlocked = EquipmentManager.Instance.UnlockNext();

        if (unlockText != null)
            unlockText.text = unlocked != null ? "Nouvel équipement :\n" + unlocked : "";

        if (resultPanel != null) resultPanel.SetActive(true);   // on montre les résultats

        if (TargetSpawner.Instance != null) TargetSpawner.Instance.ClearAll();
        if (GladiatorSpawner.Instance != null) GladiatorSpawner.Instance.StopAndClear();
    }

    // fin du niveau avant le timer (joueur vaincu)
    public void ForceEnd(bool defeated)
    {
        if (!IsRunning) return;
        IsRunning = false;
        EndLevel(defeated);
    }
}
