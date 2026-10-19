using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Paliers")]
    public int[] tiers = { 30, 60, 100 };  // seuils des paliers, le dernier = jauge pleine
    public Color[] tierColors; // une couleur de plus que de paliers
    public RectTransform[] tierMarkers; // marqueurs de paliers sur la jauge

    [Header("Affichage")]
    public Image gaugeFill;  // l'image Fill de la jauge
    public TextMeshProUGUI scoreText;  // texte du score sur la jauge

    public int Score { get; private set; }

    // nombre de paliers atteints (0 = aucun)
    public int TierReached
    {
        get
        {
            int count = 0;
            for (int i = 0; i < tiers.Length; i++)
                if (Score >= tiers[i]) count++;
            return count;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateGauge();   // la jauge est vide et le texte affiche 0 au départ
    }

    public void AddPoints(int points)
    {
        // un niveau existe mais il est terminé : on ne compte plus rien
        if (LevelManager.Instance != null && !LevelManager.Instance.IsRunning) return;

        Score += points;
        UpdateGauge();
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateGauge();
    }

    void UpdateGauge()
    {
        int maxScore = tiers[tiers.Length - 1];   // le dernier palier remplit la jauge

        for (int i = 0; i < tierMarkers.Length; i++)
        {
            RectTransform bar = gaugeFill.rectTransform;
            float height = bar.rect.height;
            float y = -height / 2f + height * tiers[i] / maxScore;
            tierMarkers[i].anchoredPosition = new Vector2(tierMarkers[i].anchoredPosition.x, y);
        }

        if (gaugeFill != null)
        {
            // attention float car division entière
            gaugeFill.fillAmount = Mathf.Clamp01((float)Score / maxScore);

            // couleur selon le nombre de paliers atteints
            int tier = TierReached;
            if (tier < tierColors.Length)
                gaugeFill.color = tierColors[tier];
        }

        if (scoreText != null)
            scoreText.text = Score + " / " + maxScore;
    }

    // permet de changer les paliers à la volée (par exemple selon la difficulté)
    public void SetTiers(int[] newTiers)
    {
        tiers = newTiers;
        UpdateGauge();
    }
}