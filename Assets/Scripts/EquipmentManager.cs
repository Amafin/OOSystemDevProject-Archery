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
        // par défaut : le premier arc et la première flèche
        if (bows.Length > 0) SelectedBow = bows[0];
        if (arrows.Length > 0) SelectedArrow = arrows[0];
    }

    private void Start()
    {
        SpawnBow();
    }

    // appelée plus tard par le menu
    public void SelectBow(BowData bow)
    {
        SelectedBow = bow;
        SpawnBow();
    }

    public void SelectArrow(ArrowType arrow)
    {
        SelectedArrow = arrow;
    }

    // remplace l'arc présent par celui qui est sélectionné
    public void SpawnBow()
    {
        if (currentBow != null) Destroy(currentBow.gameObject);
        if (SelectedBow == null || SelectedBow.prefab == null || bowSpawnPoint == null) return;

        currentBow = Instantiate(SelectedBow.prefab, bowSpawnPoint.position, bowSpawnPoint.rotation);
    }
}