using UnityEngine;

// cible volante : tourne en arc autour de l'arène, flotte, et fait toujours face au joueur
public class TargetMover : MonoBehaviour
{
    [Header("Tirés au hasard à l'apparition (min - max)")]
    public Vector2 radiusRange = new Vector2(8f, 15f); // éloignement du joueur (m)
    public Vector2 heightRange = new Vector2(2f, 5f); // hauteur (m)
    public Vector2 speedRange = new Vector2(0.3f, 1f); // vitesse du va-et-vient
    public Vector2 arcRange = new Vector2(10f, 30f); // amplitude du va-et-vient (degrés)

    [Header("Flottement")]
    public float bobHeight = 0.3f;
    public float bobSpeed = 2f;

    Transform center; // le centre de l'arène (la plateforme)
    float radius, height, speed, arc, baseAngle, phase;
    Quaternion faceOffset; // pour que la face de la cible soit toujours vers le joueur

    public void Setup(Transform arenaCenter, float angle)
    {
        center = arenaCenter;
        baseAngle = angle;

        // tirage au sort des réglages de la cible
        radius = Random.Range(radiusRange.x, radiusRange.y);
        height = Random.Range(heightRange.x, heightRange.y);
        speed = Random.Range(speedRange.x, speedRange.y);
        arc = Random.Range(arcRange.x, arcRange.y);
        phase = Random.Range(0f, 10f);

        // on mémorise la rotation de la face (Center) par rapport à la racine
        Target target = GetComponentInChildren<Target>();
        Quaternion faceLocal = Quaternion.Inverse(transform.rotation) * target.center.rotation;
        faceOffset = Quaternion.Inverse(faceLocal);
    }

    private void Update()
    {
        if (center == null) return;  // pas encore configurée par le spawner
        float t = Time.time + phase;

        // yourne autour de la plateforme
        float a = (baseAngle + Mathf.Sin(t * speed) * arc) * Mathf.Deg2Rad;

        // position sur un cercle de rayon "radius" autour du centre
        Vector3 pos = center.position + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
        pos.y = center.position.y + height + Mathf.Sin(t * bobSpeed) * bobHeight;
        transform.position = pos;

        // la face regarde le centre de l'arène (seulement à l'horizontale)
        Vector3 toCenter = center.position - pos;
        toCenter.y = 0f;
        transform.rotation = Quaternion.LookRotation(toCenter) * faceOffset;
    }
}