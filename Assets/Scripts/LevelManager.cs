using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // accessible partout : LevelManager.Instance
    public float duration = 60f; // durée du niveau en secondes
    public TextMeshProUGUI timerText; // texte du timer
    private float timeLeft; // temps restant en secondes
    public bool IsRunning { get; private set; } // indique si le niveau est en cours

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
        timeLeft = duration;
        ScoreManager.Instance.ResetScore();
        IsRunning = true;
    }

    private void Update()
    {
        if (IsRunning)
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

    void EndLevel()
    {
        int tier = ScoreManager.Instance.TierReached;
        if (timerText != null)
            timerText.text = tier > 0 ? "Fin ! Palier " + tier : "Fin ! Objectif raté";
    }
}
