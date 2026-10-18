using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Cible")]
    public Transform center;      // objet Center, au milieu du jaune, flèche bleue (Z) vers le joueur
    public float radius = 0.5f;   // rayon de la cible jusqu'au bord du blanc (m)
    [Range(0f, 1f)]
    public float minFaceAlignment = 0.7f;   // en dessous, la flèche a touché la tranche ou l'arrière

    [Header("Retour visuel")]
    public FloatingText hitTextPrefab;   // texte flottant à instancier quand on touche la cible
    public bool showRingDebug = false;   // affiche aussi la distance en anneaux, pour calibrer

    // score classique : 10 anneaux de même largeur, 10 points au centre, 1 au bord
    public int GetPoints(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        // la flèche doit toucher la face avant (pas la tranche ni l'arrière)
        Vector3 faceNormal = center.forward;
        if (Vector3.Dot(surfaceNormal, faceNormal) < minFaceAlignment) return 0;

        // décalage par rapport au centre, mis à plat sur la face
        Vector3 offset = Vector3.ProjectOnPlane(hitPoint - center.position, faceNormal);
        float ring = offset.magnitude * 10f / radius;   // 0 au centre, 10 au bord
        if (ring >= 10f) return 0;  // hors de la cible

        int points = 10 - Mathf.FloorToInt(ring);

        // texte flottant : une copie du prefab, rangée dans une variable locale
        if (hitTextPrefab != null)
        {
            FloatingText hitText = Instantiate(hitTextPrefab, hitPoint + faceNormal * 0.05f, Quaternion.identity);
            if (showRingDebug)
                hitText.SetText("+" + points + " (" + ring.ToString("0.0") + ")");
            else
                hitText.SetPoints(points);
        }

        return points;
    }

    // repères visibles uniquement dans l'éditeur
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