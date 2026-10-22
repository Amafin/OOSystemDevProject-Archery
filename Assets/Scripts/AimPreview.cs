using System.Collections.Generic;
using UnityEngine;

// aide à la visée : quelques points de plus en plus espacés et transparents dans la direction du tir
// (indique où part la flèche, sans montrer exactement où elle arrive)
public class AimPreview : MonoBehaviour
{
    [Header("Points")]
    public GameObject dotPrefab;         // petite sphère SANS collider, matériau transparent
    public int dotCount = 8;             // nombre de points
    public float firstTime = 0.04f;      // moment du vol où apparaît le premier point (s)
    public float spacing = 0.035f;       // écart de base entre deux points (en secondes de vol)
    public float spacingGrowth = 1.5f;   // plus de 1 : les points s'écartent de plus en plus
    public float maxDistance = 6f;       // au-delà de cette distance, plus de points : on ne montre pas l'arrivée

    [Header("Apparence")]
    public float startAlpha = 0.8f;      // opacité du premier point
    public float endAlpha = 0.05f;       // opacité du dernier point
    public float startSize = 0.025f;     // taille du premier point (m)
    public float endSize = 0.012f;       // taille du dernier point (m)

    [Header("Collisions")]
    public LayerMask hitMask = ~0;       // obstacles qui coupent la courbe (sans Arrow, Bow, Player, UI)

    readonly List<Renderer> dots = new List<Renderer>();
    MaterialPropertyBlock block;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");   // couleur des matériaux URP
    Color baseColor = Color.white;

    private void Awake()
    {
        block = new MaterialPropertyBlock();

        // on crée tous les points une fois pour toutes, cachés
        for (int i = 0; i < dotCount; i++)
        {
            GameObject d = Instantiate(dotPrefab, transform);
            Renderer r = d.GetComponent<Renderer>();
            if (i == 0 && r.sharedMaterial.HasProperty(BaseColor)) baseColor = r.sharedMaterial.GetColor(BaseColor);
            d.SetActive(false);
            dots.Add(r);
        }
    }

    private void LateUpdate()
    {
        Bow bow = EquipmentManager.Instance != null ? EquipmentManager.Instance.CurrentBow : null;
        Arrow arrow = bow != null ? bow.NockedArrow : null;

        // rien à montrer : aide désactivée, pas de flèche encochée ou corde pas assez tendue
        if (!SaveSystem.Data.aimHelp || arrow == null || bow.Draw01 < bow.minDrawToFire)
        {
            HideFrom(0);
            return;
        }

        float speedMult = arrow.type != null ? arrow.type.speedMultiplier : 1f;
        float gravityFactor = arrow.type != null ? arrow.type.gravityFactor : 1f;

        // même calcul que le vrai tir (Bow.Release puis Arrow.Launch)
        Vector3 start = arrow.transform.position;
        Vector3 velocity = bow.ShootDirection() * bow.Draw01 * bow.maxArrowSpeed * speedMult;
        Vector3 gravity = Physics.gravity * gravityFactor;

        Vector3 previous = start;
        float travelled = 0f;
        int shown = 0;

        for (int i = 0; i < dotCount; i++)
        {
            // position de la flèche au temps t : départ + vitesse × t + ½ gravité × t²
            float t = firstTime + spacing * Mathf.Pow(i, spacingGrowth);
            Vector3 p = start + velocity * t + 0.5f * gravity * t * t;

            Vector3 segment = p - previous;
            travelled += segment.magnitude;
            if (travelled > maxDistance) break;   // trop loin : on ne dévoile pas l'arrivée

            // un obstacle avant ce point : la courbe s'arrête là
            if (Physics.Raycast(previous, segment.normalized, segment.magnitude, hitMask, QueryTriggerInteraction.Ignore)) break;

            // 0 pour le premier point, 1 pour le dernier : de plus en plus petit et transparent
            float k = dotCount > 1 ? (float)i / (dotCount - 1) : 0f;

            Renderer r = dots[i];
            r.gameObject.SetActive(true);
            r.transform.position = p;
            r.transform.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, k);

            Color c = baseColor;
            c.a = Mathf.Lerp(startAlpha, endAlpha, k);
            r.GetPropertyBlock(block);
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);

            previous = p;
            shown++;
        }

        HideFrom(shown);
    }

    // cache les points à partir de l'index donné
    void HideFrom(int index)
    {
        for (int i = index; i < dots.Count; i++)
            if (dots[i].gameObject.activeSelf) dots[i].gameObject.SetActive(false);
    }
}