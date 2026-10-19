using System.Collections.Generic;
using UnityEngine;

public class GladiatorSpawner : MonoBehaviour
{
    public static GladiatorSpawner Instance;

    [Header("Références")]
    public Gladiator gladiatorPrefab;   // le prefab Gladiator
    public Transform[] gates; // Gate1, Gate2... 
    public Transform arenaCenter;// ArenaCenter

    [Header("Réglages (remplacés par la difficulté)")]
    public int maxAlive = 2;  // nombre max en même temps
    public float spawnInterval = 10f; // un nouveau toutes les X secondes
    public float gladiatorSpeed = 2f;// vitesse de marche
    public float firstDelay = 5f;   // attente avant le premier

    readonly List<Gladiator> alive = new List<Gladiator>(); // les gladiateurs en vie
    float timer;
    bool running;

    private void Awake()
    {
        Instance = this;
    }

    // reçoit les réglages de la difficulté choisie
    public void ApplyDifficulty(DifficultySettings d)
    {
        maxAlive = d.maxGladiators;
        spawnInterval = d.gladiatorInterval;
        gladiatorSpeed = d.gladiatorSpeed;
        firstDelay = d.gladiatorFirstDelay;
    }

    // début du niveau
    public void StartSpawning()
    {
        ClearAll();
        timer = firstDelay;
        running = maxAlive > 0;  // 0 gladiateur max = pas de gladiateur
    }

    // fin du niveau
    public void StopAndClear()
    {
        running = false;
        ClearAll();
    }

    private void Update()
    {
        if (!running) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            if (alive.Count < maxAlive) Spawn(); // trop de gladiateurs : on attend le prochain tour
        }
    }

    void Spawn()
    {
        if (gladiatorPrefab == null || gates.Length == 0) return;

        Transform gate = gates[Random.Range(0, gates.Length)]; // une porte au hasard
        Gladiator g = Instantiate(gladiatorPrefab, gate.position, gate.rotation);
        g.Setup(gate, arenaCenter, gladiatorSpeed); // il se place lui-même sur P0

        alive.Add(g);
        g.OnRemoved += OnGladiatorRemoved; // on sera prévenu de sa mort
    }

    void OnGladiatorRemoved(Gladiator g)
    {
        alive.Remove(g); // une place se libère
    }

    void ClearAll()
    {
        foreach (Gladiator g in alive)
            if (g != null) Destroy(g.gameObject);
        alive.Clear();
    }
}