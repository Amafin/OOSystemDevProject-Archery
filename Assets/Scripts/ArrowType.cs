using UnityEngine;

// fiche d'un type de flèche : chaque type = un fichier dans Project
[CreateAssetMenu(fileName = "ArrowType", menuName = "Arena/Arrow Type")]
public class ArrowType : ScriptableObject
{
    public string displayName = "Standard"; // nom affiché dans le menu
    public Arrow prefab; // le prefab de cette flèche

    [Header("Caractéristiques")]
    public float speedMultiplier = 1f;   // 1.3 = part 30 % plus vite
    public float gravityFactor = 1f; // 0.5 = chute deux fois moins (trajectoire plus tendue)

    [Header("Progression")]
    public int unlockOrder = 0; // 0 = disponible dès le début
}