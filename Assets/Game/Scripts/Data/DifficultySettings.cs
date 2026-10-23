using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Arena/Difficulty Settings")]
public class DifficultySettings : ScriptableObject
{
    [Header("General")]
    public string displayName = "Normal";  // nom affiché dans le menu
    public float levelDuration = 60f; // durée du niveau en secondes
    public int[] tiers = { 20,50,100 }; // seuils des paliers, le dernier = jauge pleine

    [Header("Targets on the ground")]
    public float targetScale = 1f;   // taille des cibles (1.3 = 30 % plus grandes, 0.7 = 30 % plus petites)
    public bool isTraining = false;   // entraînement : pas de chrono, pas de progression

    [Header("Flying Targets")]
    public Vector2 flyingRadius = new Vector2(8f, 15f); // distance min/max du centre de la plateforme pour les cibles volantes
    public Vector2 flyingHeight = new Vector2(2f, 5f); // hauteur
    public Vector2 flyingSpeed = new Vector2(0.3f, 1f); // vitesse du va-et-vient
    public Vector2 flyingArc = new Vector2(10f, 30f); // amplitude du va-et-vient (degrés)


    [Header("Gladiators")]
    public float gladiatorInterval = 10f; // un nouveau toutes les X secondes
    public float gladiatorSpeed = 2f;// vitesse de marche
    public float gladiatorFirstDelay = 5f; // attente avant le premier

}
