using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Gladiator : MonoBehaviour, IArrowHittable
{
    [Header("Vie")]
    public int maxHealth = 50;
    public int damagePerArrow = 25; // 2 flèches pour le tuer
    public int killPoints = 5; // gagnés en le tuant

    [Header("Déplacement")]
    public float speed = 2f;  // vitesse de marche (m/s)
    public float attackRange = 1.5f; // distance au joueur où il s'arrête pour attaquer (m)

    [Header("Attaque")]
    public float attackInterval = 2f; // un coup toutes les X secondes
    public int attackDamage = 5;  // points perdus par coup
    public float hitDelay = 0.4f;   // délai entre le début de l'animation et le moment où l'épée touche

    [Header("Test sans spawner")]
    public Transform testGate; // Gate1
    public Transform testDestination;  // ArenaCenter

    [Header("Animations (optionnel)")]
    public Animator animator;

    public event System.Action<Gladiator> OnRemoved;  // le futur spawner sera prévenu

    // les 4 états possibles du gladiateur
    enum State { Entering, Chasing, Attacking, Dead }
    State state;

    NavMeshAgent agent;
    Transform arenaCenter; // centre de l'arène, sert de repère pour la hauteur du sol
    Transform[] path; // les points P0, P1... de la porte
    int pathIndex;
    int health;
    float attackTimer;
    float repathTimer; // temps avant de recalculer le chemin vers le joueur

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // il ne s'active qu'une fois sur le sable
        health = maxHealth;
    }

    private void Start()
    {
        if (arenaCenter == null && testDestination != null)
            Setup(testGate, testDestination, speed);
    }

    // appelée par le spawner juste après l'apparition
    public void Setup(Transform gate, Transform center, float walkSpeed)
    {
        arenaCenter = center;
        speed = walkSpeed;

        if (gate != null && gate.childCount > 0)
        {
            path = new Transform[gate.childCount];
            for (int i = 0; i < gate.childCount; i++) path[i] = gate.GetChild(i);

            transform.position = path[0].position;
            pathIndex = 1;
            state = State.Entering;
            if (pathIndex >= path.Length) StartChasing();
        }
        else
        {
            StartChasing();
        }
    }

    private void Update()
    {
        if (arenaCenter == null) return;

        switch (state)
        {
            case State.Entering: FollowPath(); break;
            case State.Chasing: ChasePlayer(); break;
            case State.Attacking: Attack(); break;
        }

        if (animator != null)
        {
            float s = state == State.Entering ? speed : (agent.enabled && state == State.Chasing ? agent.velocity.magnitude : 0f);
            animator.SetFloat("Speed", s);
        }
    }

    // position du joueur ramenée au niveau du sol (ses pieds, pas sa tête)
    Vector3 PlayerGround()
    {
        Vector3 p = Camera.main != null ? Camera.main.transform.position : arenaCenter.position;
        p.y = arenaCenter.position.y;
        return p;
    }

    // distance au joueur, à l'horizontale
    float DistanceToPlayer()
    {
        Vector3 flat = PlayerGround() - transform.position;
        flat.y = 0f;
        return flat.magnitude;
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
            if (pathIndex >= path.Length) StartChasing();
        }
    }

    // arrivé sur le sable : le NavMesh prend le relais
    void StartChasing()
    {
        state = State.Chasing;
        agent.enabled = true;
        agent.speed = speed;
        agent.stoppingDistance = attackRange * 0.9f; // s'arrête un peu avant la portée max
        agent.Warp(transform.position); // "pose" le gladiateur sur le NavMesh
        agent.isStopped = false;
        agent.SetDestination(PlayerGround());
        repathTimer = 0f;
    }

    // marche vers le joueur
    void ChasePlayer()
    {
        // le joueur peut bouger : on recalcule le chemin 4 fois par seconde
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = 0.25f;
            agent.SetDestination(PlayerGround());
        }

        bool inRange = DistanceToPlayer() <= attackRange;

        // joueur hors d'atteinte (plateforme inaccessible) : il frappe depuis le point le plus proche
        bool stuck = !agent.pathPending && agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.05f;

        if (inRange || stuck) StartAttack();
    }

    void StartAttack()
    {
        state = State.Attacking;
        agent.isStopped = true;
        attackTimer = attackInterval * 0.5f; // premier coup un peu plus vite
    }

    void Attack()
    {
        // toujours tourné vers le joueur
        FaceTowards(PlayerGround());

        // le joueur s'est éloigné : il le poursuit à nouveau
        if (DistanceToPlayer() > attackRange + 0.75f)
        {
            CancelInvoke(nameof(DealDamage));
            StartChasing();
            return;
        }

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
        if (state != State.Attacking) return; // tué ou joueur parti entre-temps : le coup n'arrive pas

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(-attackDamage);
        if (PlayerRig.Instance != null) PlayerRig.Instance.Vibrate(0.8f, 0.3f);
    }

    // touché par une flèche
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