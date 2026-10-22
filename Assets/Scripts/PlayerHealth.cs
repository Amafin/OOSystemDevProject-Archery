using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// la vie du joueur, séparée du score
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;   // accessible partout : PlayerHealth.Instance

    [Header("Vie")]
    public int maxHealth = 100;
    public bool endLevelOnDeath = true;  // à 0 PV, le niveau s'arrête

    [Header("Affichage (optionnel)")]
    public Image healthFill;   // barre de vie
    public TextMeshProUGUI healthText;
    public Image damageOverlay;    // image rouge transparente devant la vu
    public Color barColor = Color.red; // couleur normale de la barre
    public Color flashColor = Color.white; // couleur pendant le clignotement
    public float flashTime = 0.15f;// durée du clignotement de la barre
    public float overlayAlpha = 0.35f; // opacité du rouge au moment du coup
    public float overlayFade = 0.5f; // temps pour que le rouge disparaisse

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
        StopAllCoroutines();
        StartCoroutine(DamageFeedback());   // on voit le coup, en plus de la vibration

        if (PlayerRig.Instance != null) PlayerRig.Instance.Vibrate(0.8f, 0.3f); // on sent le coup

        if (Health <= 0 && endLevelOnDeath && level != null)
            level.ForceEnd(true);   // vaincu
    }

    void UpdateDisplay()
    {
        if (healthFill != null) healthFill.fillAmount = (float)Health / maxHealth;
        if (healthText != null) healthText.text = "HP " + Health;
    }

    // barre qui clignote + bord de la vue qui rougit puis s'efface
    IEnumerator DamageFeedback()
    {
        if (healthFill != null) healthFill.color = flashColor;

        float t = 0f;
        while (t < overlayFade)
        {
            t += Time.deltaTime;
            if (healthFill != null && t >= flashTime) healthFill.color = barColor;
            if (damageOverlay != null)
            {
                Color c = damageOverlay.color;
                c.a = Mathf.Lerp(overlayAlpha, 0f, t / overlayFade);
                damageOverlay.color = c;
            }
            yield return null;
        }
        if (healthFill != null) healthFill.color = barColor;
    }
}