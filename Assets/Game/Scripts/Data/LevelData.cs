using UnityEngine;

// Contenu d'un niveau : quelles cibles et quels ennemis apparaissent.
// La difficulté (DifficultySettings) règle ensuite tailles, vitesses et durée.
[CreateAssetMenu(fileName = "Level", menuName = "Arena/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("General")]
    public string displayName = "Level";
    public int unlockOrder = 0; // 0 = débloqué dès le début

    [Header("Targets")]
    public int groundCount = 4;  // cibles fixes au sol
    public int flyingCount = 0;  // cibles volantes

    [Header("Random size (harder levels)")]
    public bool randomSize = false; // chaque cible qui apparaît a une taille différente
    public Vector2 sizeRange = new Vector2(0.6f, 1.2f); // multiplié par targetScale de la difficulté

    [Header("Gladiators")]
    public int maxGladiators = 0;  // 0 = pas de gladiateurs
}