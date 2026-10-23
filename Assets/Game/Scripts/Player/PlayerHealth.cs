using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// la vie du joueur, séparée du score
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;   // accessible partout : PlayerHealth.Instance

    [Header("Vie")]
    public int maxHealth = 80;
    public bool endLevelOnDeath = true;  // à 0 PV, le niveau s'arrête

    [Header("Vibration")]
    public float vibrationStrength = 0.6f;   // force de la vibration (0 à 1)
    public float vibrationDuration = 0.2f;   // durée d'une vibration (s)
    public float vibrationCooldown = 1.5f;     // au moins X secondes entre deux vibrations, même avec plusieurs gladiateurs
    float lastVibration = -10f;

    [Header("Affichage (optionnel)")]
    public Image healthFill;   // barre de vie
    public TextMeshProUGUI healthText;
    public Image damageOverlay;    // image rouge transparente devant la vu
    public Color barColor = Color.red; // couleur normale de la barre
    public Color flashColor = Color.white; // couleur pendant le clignotement
    public float flashTime = 0.15f;// durée du clignotement de la barre
    public float overlayAlpha = 0.35f; // opacité du rouge au moment du coup
    public float overlayFade = 0.5f; // temps pour que le rouge disparaisse

    [Header("Son")]
    public AudioClip hurtSound;                 
    [Range(0f, 1f)] public float hurtVolume = 1f;
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

        if (hurtSound != null)
        {
            Vector3 earPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(hurtSound, earPos, hurtVolume);
        }
        // on sent le coup, mais pas en continu si plusieurs gladiateurs frappent
        if (PlayerRig.Instance != null && Time.time - lastVibration >= vibrationCooldown)
        {
            PlayerRig.Instance.Vibrate(vibrationStrength, vibrationDuration);
            lastVibration = Time.time;
        }

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