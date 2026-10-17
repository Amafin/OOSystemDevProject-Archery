using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class Bow : MonoBehaviour, IXRSelectFilter
{
    [Header("Références")]
    public Transform stringBone;            // l'os String
    public SkinnedMeshRenderer bowRenderer; // le maillage avec la shape key Bend
    public Arrow arrowPrefab;               // laisser vide pour tester sans flèche

    [Header("Mains")]
    public Transform leftHand;              // XR Origin > Camera Offset > Left Controller
    public Transform rightHand;             // XR Origin > Camera Offset > Right Controller
    public InputActionReference leftGrip;   // XRI Left Interaction/Select
    public InputActionReference rightGrip;  // XRI Right Interaction/Select
    public float grabRadius = 0.15f;        // distance main-corde pour attraper (m)

    [Header("Réglages")]
    public float maxDraw = 0.5f;        // tirage max en mètres
    public int bendBlendShapeIndex = 0; // index de Bend
    public float returnSpeed = 15f;     // vitesse de retour de la corde (m/s)
    public float maxArrowSpeed = 60f;   // vitesse de la flèche à pleine tension (m/s)
    public float minDrawToFire = 0.1f;  // en dessous de 10 %, pas de tir

    public float Draw01 { get; private set; } // tension de la corde de 0 à 1

    XRGrabInteractable grab;
    Vector3 restLocal;     // position de repos de la corde
    Vector3 pullAxisLocal; // direction poignée -> corde
    float currentPull;
    Transform pullingHand;
    InputAction pullingGrip;
    Arrow nockedArrow;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectFilters.Add(this);
        restLocal = transform.InverseTransformPoint(stringBone.position);
        pullAxisLocal = restLocal.normalized;
    }

    private void OnDestroy()
    {
        if (grab != null) grab.selectFilters.Remove(this);
    }

    // ---- Filtre : une seule main peut tenir l'arc ----
    public bool canProcess => isActiveAndEnabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!grab.isSelected) return true; // personne ne tient l'arc : on peut le prendre

        foreach (var holder in grab.interactorsSelecting)
        {
            if (holder == interactor) return true; // la main qui tient déjà l'arc le garde
        }
        return false; // une autre main essaie de le prendre : refusé
    }

    // ---- Corde ----
    private void LateUpdate()
    {
        if (pullingHand == null)
        {
            TryStartPull();
        }
        else if (!grab.isSelected || !pullingGrip.IsPressed())
        {
            ReleaseString();
        }

        if (pullingHand != null)
        {
            // position de la main projetée sur l'axe de tir
            Vector3 handLocal = transform.InverseTransformPoint(pullingHand.position);
            float pull = Vector3.Dot(handLocal - restLocal, pullAxisLocal);
            currentPull = Mathf.Clamp(pull, 0f, maxDraw);
        }
        else
        {
            currentPull = Mathf.MoveTowards(currentPull, 0f, returnSpeed * Time.deltaTime);
        }

        stringBone.position = transform.TransformPoint(restLocal + pullAxisLocal * currentPull);
        Draw01 = currentPull / maxDraw;
        bowRenderer.SetBlendShapeWeight(bendBlendShapeIndex, Draw01 * 100f);

        if (nockedArrow != null)
        {
            nockedArrow.transform.position = stringBone.position;
            nockedArrow.transform.rotation = Quaternion.LookRotation(ShootDirection(), transform.up);
        }
    }

    void TryStartPull()
    {
        // il faut d'abord tenir l'arc
        if (!grab.isSelected) return;

        // quelle main tient l'arc ? l'autre peut tirer la corde
        Transform holder = grab.firstInteractorSelecting.transform;
        bool leftHolds = holder.IsChildOf(leftHand);
        Transform freeHand = leftHolds ? rightHand : leftHand;
        InputAction freeGrip = leftHolds ? rightGrip.action : leftGrip.action;

        Vector3 stringRestWorld = transform.TransformPoint(restLocal);
        bool closeEnough = Vector3.Distance(freeHand.position, stringRestWorld) <= grabRadius;

        if (closeEnough && freeGrip.WasPressedThisFrame())
        {
            pullingHand = freeHand;
            pullingGrip = freeGrip;
            if (arrowPrefab != null && nockedArrow == null)
                nockedArrow = Instantiate(arrowPrefab);
        }
    }

    void ReleaseString()
    {
        bool stillHeld = grab.isSelected;
        pullingHand = null;
        pullingGrip = null;
        if (nockedArrow == null) return;

        if (stillHeld && Draw01 >= minDrawToFire)
            nockedArrow.Launch(ShootDirection() * Draw01 * maxArrowSpeed);
        else
            Destroy(nockedArrow.gameObject);

        nockedArrow = null;
    }

    Vector3 ShootDirection()
    {
        return -transform.TransformDirection(pullAxisLocal);
    }

    // affiche la zone de prise de la corde dans la vue Scene
    private void OnDrawGizmosSelected()
    {
        if (stringBone == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(stringBone.position, grabRadius);
    }
}