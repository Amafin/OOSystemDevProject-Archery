using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    public float lifeTime = 20f;

    Rigidbody rb;
    bool flying;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void Launch (Vector3 Velocity)
    {
        rb.isKinematic = false;
        rb.linearVelocity = Velocity;
        flying = true;
        rb.isKinematic = false;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        // flèche orienté dans le sens de la trajectoire
        if (flying && rb.linearVelocity.sqrMagnitude > 1f)
        {
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!flying) return;
        flying = false;
        rb.isKinematic = true;
        transform.SetParent(collision.transform, true);
    }

}
