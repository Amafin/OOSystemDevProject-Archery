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
    public Transform arrowRest;             // l'appui de la flèche, sur le côté de la poignée

    [Header("Mains")]
    public Transform leftHand;              // XR Origin > Camera Offset > Left Controller
    public Transform rightHand;             // XR Origin > Camera Offset > Right Controller
    public InputActionReference leftGrip;   // XRI Left Interaction/Select
    public InputActionReference rightGrip;  // XRI Right Interaction/Select
    public float grabRadius = 0.15f;        // distance flèche-corde pour encocher (m)
    public float nockRadius = 0.1f;        // distance encoche-corde pour encocher (m)

    [Header("Réglages")]
    public float maxDraw = 0.5f;        // tirage max en mètres
    public int bendBlendShapeIndex = 0; // index de Bend
    public float returnSpeed = 15f;     // vitesse de retour de la corde (m/s)
    public float maxArrowSpeed = 60f;   // vitesse de la flèche à pleine tension (m/s)
    public float minDrawToFire = 0.1f;  // en dessous de 10 %, pas de tir

    public float Draw01 { get; private set; } // tension de la corde de 0 à 1

    XRGrabInteractable grab;
    Vector3 restLocal;               // position de repos de la corde
    Vector3 pullAxisLocal;           // direction repose-flèche -> corde
    Quaternion attachLocalRotation;  // rotation de l'AttachPoint par rapport à l'arc
    float currentPull;
    Transform pullingHand;
    InputAction pullingGrip;
    Arrow nockedArrow;
    public Arrow NockedArrow => nockedArrow;   // la flèche encochée        
    public bool canProcess => isActiveAndEnabled;// une seule main peut tenir l'arc

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectFilters.Add(this);
        restLocal = transform.InverseTransformPoint(stringBone.position);

        // l'axe de tir va du repose-flèche vers la corde
        Vector3 arrowRestLocal = arrowRest != null ? transform.InverseTransformPoint(arrowRest.position) : Vector3.zero;
        pullAxisLocal = (restLocal - arrowRestLocal).normalized;

        Transform attach = grab.attachTransform != null ? grab.attachTransform : transform;
        attachLocalRotation = Quaternion.Inverse(transform.rotation) * attach.rotation;
    }

    private void Start()
    {
        if (leftHand == null) leftHand = PlayerRig.Instance.leftHand;
        if (rightHand == null) rightHand = PlayerRig.Instance.rightHand;
        if (leftGrip == null) leftGrip = PlayerRig.Instance.leftGrip;
        if (rightGrip == null) rightGrip = PlayerRig.Instance.rightGrip;
    }

    private void OnDestroy()
    {
        if (grab != null) grab.selectFilters.Remove(this);
    }



    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!grab.isSelected) return true;
        foreach (var holder in grab.interactorsSelecting)
            if (holder == interactor) return true;
        return false;
    }

    // Corde
    private void LateUpdate()
    {
        if (pullingHand == null)
            TryStartPull();
        else if (!grab.isSelected || !pullingGrip.IsPressed())
            ReleaseString();

        if (pullingHand != null)
        {
            AimWithBothHands();

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

    // pendant la tension, l'arc vise de la main de la corde vers la main de l'arc
    void AimWithBothHands()
    {
        // le point de la main qui tient l'arc
        Transform handAttach = grab.firstInteractorSelecting.GetAttachTransform(grab);

        // direction de visée : de la main de la corde vers la main de l'arc
        Vector3 aimDirection = (handAttach.position - pullingHand.position).normalized;

        // 1. l'orientation que l'arc aurait si la main seule le tenait
        Quaternion handRotation = handAttach.rotation * Quaternion.Inverse(attachLocalRotation);

        // 2. on la tourne juste assez pour viser vers la main de la corde
        Vector3 handShootDir = handRotation * -pullAxisLocal;
        transform.rotation = Quaternion.FromToRotation(handShootDir, aimDirection) * handRotation;

        // la poignée reste dans la main
        Transform attach = grab.attachTransform != null ? grab.attachTransform : transform;
        transform.position += handAttach.position - attach.position;
    }

    void TryStartPull()
    {
        if (!grab.isSelected) return;

        // quelle main tient l'arc ? l'autre est la main libre
        Transform holder = grab.firstInteractorSelecting.transform;
        bool leftHolds = holder.IsChildOf(leftHand);
        Transform freeHand = leftHolds ? rightHand : leftHand;
        InputAction freeGrip = leftHolds ? rightGrip.action : leftGrip.action;

        Arrow arrow = FindHeldArrowNearString(freeHand);

        // la main libre est-elle sur la corde ?
        Vector3 stringRestWorld = transform.TransformPoint(restLocal);
        bool handOnString = Vector3.Distance(freeHand.position, stringRestWorld) <= grabRadius;

        if (arrow != null && freeGrip.IsPressed())
        {
            // une flèche est amenée sur la corde : on l'encoche et on tire
            var arrowHolder = arrow.Grab.firstInteractorSelecting;
            arrow.Grab.interactionManager.SelectExit(arrowHolder, arrow.Grab);
            arrow.Nock();
            IgnoreBowCollisions(arrow);
            nockedArrow = arrow;
            pullingHand = freeHand;
            pullingGrip = freeGrip;
            grab.trackRotation = false;   // c'est notre code qui oriente l'arc pendant la tension
        }
        else if (arrow == null && handOnString && freeGrip.WasPressedThisFrame())
        {
            // main vide qui attrape la corde : on tire sans flèche
            pullingHand = freeHand;
            pullingGrip = freeGrip;
            grab.trackRotation = false;
        }
    }

    // une flèche tenue par la main libre, près de la corde ?
    Arrow FindHeldArrowNearString(Transform freeHand)
    {
        Vector3 stringRestWorld = transform.TransformPoint(restLocal);
        foreach (Collider c in Physics.OverlapSphere(stringRestWorld, grabRadius, ~0, QueryTriggerInteraction.Collide))
        {
            Arrow arrow = c.GetComponentInParent<Arrow>();
            if (arrow == null || arrow.IsNocked || !arrow.Grab.isSelected) continue;

            // l'encoche (pivot de la flèche) doit être tout près de la corde
            if (Vector3.Distance(arrow.transform.position, stringRestWorld) > nockRadius) continue;

            if (arrow.Grab.firstInteractorSelecting.transform.IsChildOf(freeHand)) return arrow;
        }
        return null;
    }

    // la flèche ne doit jamais toucher l'arc qui la tire
    void IgnoreBowCollisions(Arrow arrow)
    {
        foreach (Collider arrowCol in arrow.GetComponentsInChildren<Collider>())
            foreach (Collider bowCol in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(arrowCol, bowCol);
    }

    void ReleaseString()
    {
        grab.trackRotation = true;    // l'arc reprend l'orientation de la main
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

    public Vector3 ShootDirection()
    {
        return -transform.TransformDirection(pullAxisLocal);
    }

    // repères visibles uniquement dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        if (stringBone == null) return;

        // zone où la main peut attraper la corde
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(stringBone.position, grabRadius);

        // zone où l'encoche de la flèche s'accroche
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(stringBone.position, nockRadius);

        // trajet de la flèche : de la corde, en passant par le repose-flèche
        if (arrowRest != null)
        {
            Gizmos.color = Color.red;
            Vector3 dir = (arrowRest.position - stringBone.position).normalized;
            Gizmos.DrawLine(stringBone.position, stringBone.position + dir * 1f);
        }
    }
}