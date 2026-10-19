using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Arena/Difficulty Settings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Général")]
    public string displayName = "Normal";  // nom affiché dans le menu
    public float levelDuration = 60f; // durée du niveau en secondes
    public int[] tiers = { 20,50,100 }; // seuils des paliers, le dernier = jauge pleine

    [Header("Cibles au sol")]
    public int groundCount = 2; // nombre de cibles au sol

    [Header("Cibles volantes")]
    public int flyingCount = 1; // nombre de cibles volantes
    public Vector2 flyingRadius = new Vector2(8f, 15f); // disance min/max du centre de la plateforme pour les cibles volantes
    public Vector2 flyingHeight = new Vector2(2f, 5f); // hauteur
    public Vector2 flyingSpeed = new Vector2(0.3f, 1f); // vitesse du va-et-vient
    public Vector2 flyingArc = new Vector2(10f, 30f); // amplitude du va-et-vient (degrés)
}
