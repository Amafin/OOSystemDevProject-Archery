using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class Arrow : MonoBehaviour
{
    public float lifeTime = 20f;
    public float touchLifeTime = 2f;
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

    public void Nock()
    {
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
        rb.linearVelocity = velocity;
        flying = true;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!flying) return;
        lastVelocity = rb.linearVelocity;   // mémorisation de la vitesse avant un éventuel choc
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

        // cible touché ?
        Target target = collision.collider.GetComponentInParent<Target>();
        if (target != null && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(target.GetPoints(contact.point, contact.normal));
        }

        Destroy(gameObject, touchLifeTime);
    }
}