using UnityEngine;
using UnityEngine.AI;

// un gladiateur sort d'une porte, descend les marches, marche vers la plateforme
// puis attaque le joueur jusqu'à ce qu'il soit tué
[RequireComponent(typeof(NavMeshAgent))]
public class Gladiator : MonoBehaviour, IArrowHittable
{
    [Header("Vie")]
    public int maxHealth = 50;
    public int damagePerArrow = 25; // 2 flèches pour le tuer
    public int killPoints = 5; // gagnés en le tuant

    [Header("Déplacement")]
    public float speed = 2f;  // vitesse de marche (m/s)
    public float reachDistance = 3f; // distance au centre où il s'arrête pour attaquer (m)

    [Header("Attaque")]
    public float attackInterval = 2f; // un coup toutes les 2 secondes
    public int attackDamage = 5;  // points perdus par coup
    public float hitDelay = 0.4f;   // délai entre le début de l'animation et le moment où l'épée touche

    [Header("Test sans spawner")]
    public Transform testGate; // Gate1
    public Transform testDestination;  // ArenaCenter

    [Header("Animations (optionnel)")]
    public Animator animator;

    public event System.Action<Gladiator> OnRemoved;  // le futur spawner sera prévenu

    // les 4 états possibles du gladiateur
    enum State { Entering, Walking, Attacking, Dead }
    State state;

    NavMeshAgent agent;
    Transform target; //  centre de l'arène
    Transform[] path; // les points P0, P1... de la porte
    int pathIndex;
    int health;
    float attackTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // il ne s'active qu'une fois sur le sable
        health = maxHealth;
    }

    private void Start()
    {
        if (target == null && testDestination != null)
            Setup(testGate, testDestination, speed);
    }

    // appelée par le spawner juste après l'apparition
    public void Setup(Transform gate, Transform dest, float walkSpeed)
    {
        target = dest;
        speed = walkSpeed;

        if (gate != null && gate.childCount > 0)
        {
            path = new Transform[gate.childCount];
            for (int i = 0; i < gate.childCount; i++) path[i] = gate.GetChild(i);

            transform.position = path[0].position;
            pathIndex = 1;
            state = State.Entering;
            if (pathIndex >= path.Length) StartWalking();
        }
        else
        {
            StartWalking();
        }
    }

    private void Update()
    {
        if (target == null) return;

        switch (state)
        {
            case State.Entering: FollowPath(); break;
            case State.Walking: CheckArrival(); break;
            case State.Attacking: Attack(); break;
        }

        if (animator != null)
        {
            float s = state == State.Entering ? speed : (agent.enabled ? agent.velocity.magnitude : 0f);
            animator.SetFloat("Speed", s);
        }
    }

    // descente des marches
    void FollowPath()
    {
        Transform next = path[pathIndex];
        transform.position = Vector3.MoveTowards(transform.position, next.position, speed * Time.deltaTime);
        FaceTowards(next.position);

        if (Vector3.Distance(transform.position, next.position) < 0.05f)
        {
            pathIndex++;
            if (pathIndex >= path.Length) StartWalking();
        }
    }


    void StartWalking()
    {
        state = State.Walking;
        agent.enabled = true;
        agent.speed = speed;
        agent.Warp(transform.position); // "pose" le gladiateur sur le NavMesh
        agent.SetDestination(target.position);
    }

    void CheckArrival()
    {
        Vector3 flat = target.position - transform.position;
        flat.y = 0f;
        if (flat.magnitude <= reachDistance)
        {
            state = State.Attacking;
            agent.isStopped = true;
            attackTimer = attackInterval * 0.5f; // premier coup un peu plus vite
        }
    }

    void Attack()
    {
        // toujours tourné vers le joueur
        if (Camera.main != null) FaceTowards(Camera.main.transform.position);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            if (animator != null) animator.SetTrigger("Attack");
            Invoke(nameof(DealDamage), hitDelay);  // l'épée touche un peu après le début du geste
        }
    }

    void DealDamage()
    {
        if (state != State.Attacking) return; // tué entre-temps : le coup n'arrive pas

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(-attackDamage);
        if (PlayerRig.Instance != null) PlayerRig.Instance.Vibrate(0.8f, 0.3f);
    }

    //touché par une flèche
    public void OnArrowHit(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        if (state == State.Dead) return;

        health -= damagePerArrow;
        if (health <= 0) Die();
        else if (animator != null) animator.SetTrigger("Hit");
    }

    void Die()
    {
        state = State.Dead;
        CancelInvoke(nameof(DealDamage)); // un coup en cours est annulé
        if (agent.enabled) agent.isStopped = true;

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(killPoints);
        if (animator != null) animator.SetTrigger("Die");

        OnRemoved?.Invoke(this);
        Destroy(gameObject, 2f); // 2 s pour voir l'animation de mort
    }

    // tourne le gladiateur vers un point, à l'horizontale
    void FaceTowards(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
    }
}