using UnityEngine;

public class Target : MonoBehaviour
{
    public Transform center;      // objet Center, au milieu du jaune
    public float radius = 0.5f;   // rayon de la cible (m)
    public int maxPoints = 100;   // points au centre

    public FloatingText hitTextPrefab;   // texte flottant à instancier quand on touche la cible

    // plus on est près du centre, plus on gagne de points
    public int GetPoints(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        // décalage du point touché par rapport au centre, mis à plat sur la face de la cible
        Vector3 offset = Vector3.ProjectOnPlane(hitPoint - center.position, surfaceNormal);
        float distance01 = offset.magnitude / radius;   // 0 au centre, 1 au bord
        if (distance01 > 1f) return 0;                   // touché le bord du support

        int points = Mathf.RoundToInt((1f - distance01) * maxPoints);

        // texte flottant : une copie du prefab, rangée dans une variable locale
        if (hitTextPrefab != null)
        {
            FloatingText hitText = Instantiate(hitTextPrefab, hitPoint + surfaceNormal * 0.05f, Quaternion.identity);
            hitText.SetPoints(points);
        }

        return points;
    }

    // cercle visible dans l'éditeur pour régler le rayon
    private void OnDrawGizmosSelected()
    {
        if (center == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center.position, radius);
    }
}