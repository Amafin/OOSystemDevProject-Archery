using UnityEngine;

public class Target : MonoBehaviour, IArrowHittable
{
    [Header("Cible")]
    public Transform center; // objet Center, au milieu du jaune, flèche bleue (Z) vers le joueur
    public float radius = 0.5f;  // rayon de la cible jusqu'au bord du blanc (m)
    [Range(0f, 1f)]
    public float minFaceAlignment = 0.7f;  // en dessous, la flèche a touché la tranche ou l'arrière

    public event System.Action OnHit; // le spawner sera prévenu quand la cible est touchée
    bool wasHit; // une cible ne compte qu'une fois
    float baseScale;   // taille de la cible à sa création, pour adapter le rayon si elle est agrandie


    private void Awake()
    {
        baseScale = transform.lossyScale.x;   // taille d'origine, avant que le spawner la change
    }

    // appelée par la flèche quand elle touche la cible
    public void OnArrowHit(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        if (wasHit) return;

        int points = GetPoints(hitPoint, surfaceNormal);
        if (points <= 0) return;  // tranche ou bord : la cible reste en place

        wasHit = true;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddPoints(points);

        OnHit?.Invoke(); // on prévient le spawner
    }

    public int GetPoints(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        // la flèche doit toucher la face avant (pas la tranche ni l'arrière)
        Vector3 faceNormal = center.forward;
        if (Vector3.Dot(surfaceNormal, faceNormal) < minFaceAlignment) return 0;

        // décalage par rapport au centre, mis à plat sur la face
        Vector3 offset = Vector3.ProjectOnPlane(hitPoint - center.position, faceNormal);


        float scaledRadius = radius * transform.lossyScale.x / baseScale;   // le rayon suit la taille de la cible
        float ring = offset.magnitude * 10f / scaledRadius; // 0 au centre, 10 au bord
        if (ring >= 10f) return 0;  // hors de la cible

        int points = 10 - Mathf.FloorToInt(ring);

        return points;
    }

    private void OnDrawGizmosSelected()
    {
        if (center == null) return;

        // bord de la cible
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center.position, radius);

        // direction de la face : doit sortir vers le joueur
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center.position, center.position + center.forward * 0.3f);
    }
}