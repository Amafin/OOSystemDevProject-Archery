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
    public DifficultySettings difficulty; // les paramètres de difficulté du niveau

    public TextMeshProUGUI unlockText; // "Nouvel équipement : ..." dans le panneau de résultats
    public bool IsTraining => difficulty != null && difficulty.isTraining;  // vrai si la difficulté choisie est le mode entraînement
    public LevelData level;   // niveau joué (assigné par le menu, ou à la main pour tester sans menu)
    public GameObject hud;   // ScoreGauge (jauge, timer, vie) : visible seulement pendant la partie

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (hud != null) hud.SetActive(false);   // caché dans le menu
        // s'il y a un menu, c'est lui qui lance la partie
        if (MenuManager.Instance == null) StartLevel();
    }

    // appelée par le menu
    public void StartLevel(DifficultySettings chosen, LevelData chosenLevel)
    {
        difficulty = chosen;
        level = chosenLevel;
        StartLevel();
    }

    public void StartLevel()
    {
        if (Leaderboard.Instance != null) Leaderboard.Instance.Hide();

        if (NameEntry.Instance != null) NameEntry.Instance.Close();
        if (resultPanel != null) resultPanel.SetActive(false);   // caché pendant la partie

        // On applique les paramètres de difficulté (tailles, vitesses, durée) et du niveau (nombre de cibles, gladiateurs, tailles aléatoires)
        if (difficulty != null && level != null)
        {
            duration = difficulty.levelDuration;
            ScoreManager.Instance.SetTiers(difficulty.tiers);
            if (TargetSpawner.Instance != null) TargetSpawner.Instance.ApplyDifficulty(difficulty, level);       // NOUVEAU : + level
            if (GladiatorSpawner.Instance != null) GladiatorSpawner.Instance.ApplyDifficulty(difficulty, level); // NOUVEAU : + level
        }
        timeLeft = duration;
        ScoreManager.Instance.ResetScore();
        if (PlayerHealth.Instance != null) PlayerHealth.Instance.ResetHealth();
        if (hud != null) hud.SetActive(true);
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

    // arrête tout et revient au menu
    public void ReturnToMenu()
    {
        if (hud != null) hud.SetActive(false);   // le score final est sur le panneau de résultats
        if (NameEntry.Instance != null) NameEntry.Instance.Close();
        IsRunning = false;
        if (resultPanel != null) resultPanel.SetActive(false);
        if (TargetSpawner.Instance != null) TargetSpawner.Instance.ClearAll();
        if (GladiatorSpawner.Instance != null) GladiatorSpawner.Instance.StopAndClear();
        Arrow.ClearAll();   // plus de flèches qui traînent entre deux parties
        if (MenuManager.Instance != null) MenuManager.Instance.ShowMenu();
        if (Leaderboard.Instance != null) Leaderboard.Instance.Show();
    }

    void EndLevel(bool defeated = false)
    {
        if (hud != null) hud.SetActive(false);   // le score final est sur le panneau de résultats
        if (MenuManager.Instance != null) MenuManager.Instance.SetLecternVisible(true);   // pour saisir son nom

        int tier = ScoreManager.Instance.TierReached;
        int score = ScoreManager.Instance.Score;

        string levelName = level != null ? level.displayName : "";
        string difficultyName = difficulty != null ? difficulty.displayName : "";
        int best = Mathf.Max(SaveSystem.BestScore(levelName, difficultyName), score);   // meilleur score sauvegardé (fichier JSON)

        if (resultText != null)
        {
            if (defeated) resultText.text = "Vaincu !";
            else resultText.text = tier > 0 ? "Objectif réussi ! Palier " + tier : "Objectif raté";
        }
        if (scoreText != null)
            scoreText.text = "Score : " + score;
        if (bestText != null)
            bestText.text = "Meilleur : " + best;

        //on ne débloque que si on gagne son niveau le plus avancé (sinon rejouer le niveau 1 en boucle débloquerait tout)
        bool newestLevel = level != null && EquipmentManager.Instance != null
                           && level.unlockOrder == EquipmentManager.Instance.UnlockedLevel;

        // objectif atteint sans être vaincu, sur son dernier niveau : niveau suivant + équipement suivant
        string unlocked = null;
        if (!defeated && tier > 0 && !IsTraining && newestLevel)
            unlocked = EquipmentManager.Instance.UnlockNext();

        if (unlockText != null)
            unlockText.text = unlocked != null ? "Nouvel équipement :\n" + unlocked : "";

        if (resultPanel != null) resultPanel.SetActive(true);   // on montre les résultats

        if (TargetSpawner.Instance != null) TargetSpawner.Instance.ClearAll();
        if (GladiatorSpawner.Instance != null) GladiatorSpawner.Instance.StopAndClear();
        Arrow.ClearAll();   // plus de flèches qui traînent entre deux parties

        // partie classée (pas l'entraînement) : on propose d'enregistrer le score
        if (!IsTraining && NameEntry.Instance != null && difficulty != null)
            NameEntry.Instance.Open(score, difficulty.displayName, levelName);

        if (Leaderboard.Instance != null)
        {
            if (!IsTraining) Leaderboard.Instance.SetLevel(levelName); // Classement du niveau joué (pour le menu)
            Leaderboard.Instance.Show();
        }
    }

    // fin du niveau avant le timer (joueur vaincu)
    public void ForceEnd(bool defeated)
    {
        if (!IsRunning) return;
        IsRunning = false;
        EndLevel(defeated);
    }
}