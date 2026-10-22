using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class Arrow : MonoBehaviour
{
    public ArrowType type;   // le type de cette flèche
    public float lifeTime = 20f;
    public float touchLifeTime = 2f;
    public float dropLifeTime = 5f;   // flèche lâchée sans être tirée : disparaît après ce délai

    static readonly List<Arrow> all = new List<Arrow>();   // toutes les flèches présentes dans la scène
    public Collider grabZone;   // la zone de saisie, éteinte en vol

    public XRGrabInteractable Grab { get; private set; }
    public bool IsNocked { get; private set; }

    Rigidbody rb;
    bool flying;
    public Transform tip;
    Vector3 lastVelocity;          // vitesse juste avant le choc

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable() { all.Add(this); }
    private void OnDisable() { all.Remove(this); }

    private void Start()
    {
        Grab.selectExited.AddListener(OnReleased);
        Grab.selectEntered.AddListener(OnGrabbed);
    }

    // lâchée sans être encochée ni tirée : on la retire après un délai
    void OnReleased(SelectExitEventArgs args)
    {
        if (!IsNocked && !flying) Invoke(nameof(DropDestroy), dropLifeTime);
    }

    // reprise en main avant le délai : on annule
    void OnGrabbed(SelectEnterEventArgs args)
    {
        CancelInvoke(nameof(DropDestroy));
    }

    void DropDestroy()
    {
        if (!IsNocked && !flying) Destroy(gameObject);
    }

    // fin de partie : on retire toutes les flèches sauf celle tenue en main ou encochée
    public static void ClearAll()
    {
        for (int i = all.Count - 1; i >= 0; i--)
        {
            Arrow a = all[i];
            if (a.IsNocked || a.Grab.isSelected) continue;
            Destroy(a.gameObject);
        }
    }

    public void Nock()
    {
        CancelInvoke(nameof(DropDestroy));   // encochée : elle ne doit pas disparaître
        IsNocked = true;
        Grab.enabled = false;
        rb.isKinematic = true;
    }

    public void Launch(Vector3 velocity)
    {
        if (grabZone != null) grabZone.enabled = false;  // en vol, seule la pointe touche
        IsNocked = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        float speedMult = type != null ? type.speedMultiplier : 1f;
        rb.linearVelocity = velocity * speedMult; // une flèche rapide part plus vite avec le même arc
        flying = true;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!flying) return;
        lastVelocity = rb.linearVelocity;   // mémorisation de la vitesse avant un éventuel choc
                                             
        if (type != null && type.gravityFactor != 1f)// gravité ajustée : gravityFactor 0.5 = on retire la moitié de la gravité normale
            rb.AddForce(Physics.gravity * (type.gravityFactor - 1f), ForceMode.Acceleration);

        if (rb.linearVelocity.sqrMagnitude > 1f)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!flying) return;
        flying = false;

        // point exact d'impact
        ContactPoint contact = collision.GetContact(0);

        // orienter la flèche selon la normale de l'impact
        if (lastVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lastVelocity);
        }

        // position : pointe bien sur le point d'impact
        if (tip != null)
            transform.position += contact.point - tip.position;

        // effacer le rebond et figer la flèche
        rb.linearVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        transform.SetParent(collision.transform, true);

        // l'objet touché sait-il réagir à une flèche ?
        IArrowHittable hittable = collision.collider.GetComponentInParent<IArrowHittable>();
        if (hittable != null) 
        {
            hittable.OnArrowHit(contact.point, contact.normal);
        }
            

        Destroy(gameObject, touchLifeTime);
    }
}