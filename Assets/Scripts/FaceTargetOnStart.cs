using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

// au lancement, tourne le joueur vers le pupitre, quelle que soit la direction du casque dans la pièce
[RequireComponent(typeof(XROrigin))]
public class FaceTargetOnStart : MonoBehaviour
{
    public Transform target;    // le menu du pupitre (MenuPanel)
    public float delay = 0.5f;  // attente supplémentaire une fois le casque suivi

    XROrigin origin;
    readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();

    IEnumerator Start()
    {
        origin = GetComponent<XROrigin>();

        // le casque se recentre (bouton Meta ou au démarrage) : on se retourne vers le pupitre
        SubsystemManager.GetSubsystems(subsystems);
        foreach (XRInputSubsystem s in subsystems) s.trackingOriginUpdated += OnRecenter;

        // on attend que le casque soit vraiment suivi (la caméra a quitté sa position par défaut)
        float t = 0f;
        while (t < 3f && Cam() != null && Cam().localPosition == Vector3.zero)
        {
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(delay);
        FaceTarget();

        // deuxième passage : si le casque s'est recalé entre-temps, on corrige (sinon l'angle est ~0)
        yield return new WaitForSeconds(1f);
        FaceTarget();
    }

    private void OnDestroy()
    {
        foreach (XRInputSubsystem s in subsystems) s.trackingOriginUpdated -= OnRecenter;
    }

    void OnRecenter(XRInputSubsystem s)
    {
        FaceTarget();
    }

    // la caméra du joueur (celle du XR Origin, sinon la caméra principale)
    Transform Cam()
    {
        if (origin != null && origin.Camera != null) return origin.Camera.transform;
        return Camera.main != null ? Camera.main.transform : null;
    }

    // fait pivoter le joueur sur place pour regarder la cible
    public void FaceTarget()
    {
        Transform cam = Cam();
        if (target == null || cam == null || origin == null) return;

        Vector3 look = cam.forward; look.y = 0f;  // direction du regard, à l'horizontale
        Vector3 toTarget = target.position - cam.position; toTarget.y = 0f;   // direction du pupitre
        if (look.sqrMagnitude < 0.001f || toTarget.sqrMagnitude < 0.001f) return;

        float angle = Vector3.SignedAngle(look, toTarget, Vector3.up);
        origin.RotateAroundCameraUsingOriginUp(angle);   // tourne autour de la tête, sans déplacer le joueur
        Debug.Log("FaceTarget : rotation de " + angle.ToString("0") + "°");   // TEMP
    }
}