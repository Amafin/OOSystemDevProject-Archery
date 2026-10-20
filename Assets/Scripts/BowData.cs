using UnityEngine;

// fiche d'un arc : nom, description, prefab, ordre de déblocage
[CreateAssetMenu(fileName = "BowData", menuName = "Arena/Bow Data")]
public class BowData : ScriptableObject
{
    public string displayName = "Arc d'entraînement";
    [TextArea] public string description;
    public Bow prefab; // le prefab de l'arc (BowLvl0...)

    [Header("Progression")]
    public int unlockOrder = 0; // 0 = disponible dès le début
}