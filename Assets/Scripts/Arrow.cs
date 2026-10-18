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
        if (flying && rb.linearVelocity.sqrMagnitude > 1f)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!flying) return;
        flying = false;

        // cible touché ?
        Target target = collision.collider.GetComponentInParent<Target>();
        if (target != null && ScoreManager.Instance != null)
        {
            ContactPoint contact = collision.GetContact(0);   // le point exact de l'impact
            ScoreManager.Instance.AddPoints(target.GetPoints(contact.point, contact.normal));
        }

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        transform.SetParent(collision.transform, true);
        Destroy(gameObject, touchLifeTime);
    }
}