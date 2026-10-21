using UnityEngine;

// sait quel arc et quel type de flèche le joueur utilise et fait apparaître l'arc
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;  // accessible partout

    [Header("Catalogue")]
    public BowData[] bows; // tous les arcs du jeu, dans l'ordre
    public ArrowType[] arrows; // tous les types de flèches, dans l'ordre

    [Header("Apparition de l'arc")]
    public Transform bowSpawnPoint; // où l'arc apparaît, à portée de main

    public BowData SelectedBow { get; private set; }
    public ArrowType SelectedArrow { get; private set; }

    Bow currentBow;   // l'arc présent dans la scène

    private void Awake()
    {
        Instance = this;
        // par défaut : le meilleur arc et la meilleure flèche déjà débloqués
        foreach (BowData b in bows) if (IsUnlocked(b)) SelectedBow = b;
        foreach (ArrowType a in arrows) if (IsUnlocked(a)) SelectedArrow = a;
    }

    private void Start()
    {
        SpawnBow();
    }

    public void SelectBow(BowData bow)
    {
        if (!IsUnlocked(bow)) return;   // verrouillé : on garde l'arc actuel
        SelectedBow = bow;
        SpawnBow();
    }

    public void SelectArrow(ArrowType arrow)
    {
        if (!IsUnlocked(arrow)) return; // verrouillée : on garde la flèche actuelle
        SelectedArrow = arrow;
    }

    // remplace l'arc présent par celui qui est sélectionné
    public void SpawnBow()
    {
        if (currentBow != null) Destroy(currentBow.gameObject);
        if (SelectedBow == null || SelectedBow.prefab == null || bowSpawnPoint == null) return;

        currentBow = Instantiate(SelectedBow.prefab, bowSpawnPoint.position, bowSpawnPoint.rotation);
    }

    // progression
    public int UnlockedLevel => SaveSystem.Data.unlockedLevel;

    public bool IsUnlocked(BowData bow) => bow.unlockOrder <= UnlockedLevel;
    public bool IsUnlocked(ArrowType arrow) => arrow.unlockOrder <= UnlockedLevel;

    // le plus haut niveau existant (3 avec 4 arcs)
    int MaxLevel()
    {
        int max = 0;
        foreach (BowData b in bows) max = Mathf.Max(max, b.unlockOrder);
        foreach (ArrowType a in arrows) max = Mathf.Max(max, a.unlockOrder);
        return max;
    }

    // appelée en fin de partie réussie : débloque le niveau suivant
    // renvoie le texte à afficher ou null s'il n'y a plus rien à débloquer
    public string UnlockNext()
    {
        if (UnlockedLevel >= MaxLevel()) return null;

        SaveSystem.Data.unlockedLevel++;
        SaveSystem.Save();

        // on équipe directement les nouveautés et on prépare le message
        string message = "";
        foreach (BowData b in bows)
            if (b.unlockOrder == UnlockedLevel) { SelectBow(b); message += b.displayName + "\n"; }
        foreach (ArrowType a in arrows)
            if (a.unlockOrder == UnlockedLevel) { SelectArrow(a); message += a.displayName + "\n"; }

        return message;
    }

    // clic droit sur le composant dans l'Inspector > "Reset progression"
    [ContextMenu("Reset progression")]
    void ResetProgression()
    {
        SaveSystem.ResetAll();
        Debug.Log("Progression remise à zéro");
    }
}