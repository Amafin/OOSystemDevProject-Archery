using UnityEngine;
using UnityEngine.UI;
using TMPro;

// la vie du joueur, séparée du score
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;   // accessible partout : PlayerHealth.Instance

    [Header("Vie")]
    public int maxHealth = 100;
    public bool endLevelOnDeath = true;    // à 0 PV, le niveau s'arrête

    [Header("Affichage (optionnel)")]
    public Image healthFill;               // barre de vie (Image en Filled)
    public TextMeshProUGUI healthText;     // texte "PV 85 / 100"

    public int Health { get; private set; }

    private void Awake()
    {
        Instance = this;
        Health = maxHealth;
    }

    private void Start()
    {
        UpdateDisplay();
    }

    // début du niveau : vie pleine
    public void ResetHealth()
    {
        Health = maxHealth;
        UpdateDisplay();
    }

    public void TakeDamage(int amount)
    {
        // niveau terminé ou joueur déjà à 0 : plus de dégâts
        LevelManager level = LevelManager.Instance;
        if (level != null && !level.IsRunning) return;
        if (Health <= 0) return;

        Health = Mathf.Max(0, Health - amount);
        UpdateDisplay();

        if (PlayerRig.Instance != null) PlayerRig.Instance.Vibrate(0.8f, 0.3f); // on sent le coup

        if (Health <= 0 && endLevelOnDeath && level != null)
            level.ForceEnd(true);   // vaincu
    }

    void UpdateDisplay()
    {
        if (healthFill != null) healthFill.fillAmount = (float)Health / maxHealth;
        if (healthText != null) healthText.text = "PV " + Health + " / " + maxHealth;
    }
}