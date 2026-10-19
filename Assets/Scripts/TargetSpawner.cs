using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance;

    [Header("Prefabs")]
    public GameObject standTargetPrefab;
    public GameObject flyingTargetPrefab;

    [Header("Cibles au sol")]
    public Transform[] groundPoints;
    public int groundCount = 2;

    [Header("Cibles volantes")]
    public Transform arenaCenter; // centre de la plateforme
    public int flyingCount = 1;

    [Header("Réglages")]
    public float respawnDelay = 1f;
    private DifficultySettings settings;    // paramètres de difficulté appliqués

    // chaque cible présente et le point qu'elle occupe (null pour les volantes)
    readonly Dictionary<GameObject, Transform> active = new Dictionary<GameObject, Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnAll()
    {
        ClearAll();
        for (int i = 0; i < groundCount; i++) SpawnGround();
        for (int i = 0; i < flyingCount; i++) SpawnFlying();
    }

    public void ClearAll()
    {
        StopAllCoroutines();
        foreach (GameObject obj in active.Keys)
            if (obj != null) Destroy(obj);
        active.Clear();
    }

    void SpawnGround()
    {
        Transform point = PickFreePoint(groundPoints);
        if (standTargetPrefab == null || point == null) return;

        GameObject obj = Instantiate(standTargetPrefab, point.position, point.rotation);
        Register(obj, point, SpawnGround);
    }

    void SpawnFlying()
    {
        if (flyingTargetPrefab == null || arenaCenter == null) return;

        GameObject obj = Instantiate(flyingTargetPrefab);
        TargetMover mover = obj.GetComponent<TargetMover>();
        if (mover != null) mover.Setup(arenaCenter, Random.Range(0f, 360f), settings); // n'importe où autour

        Register(obj, null, SpawnFlying);
    }

    void Register(GameObject obj, Transform point, System.Action respawn)
    {
        active.Add(obj, point);
        Target target = obj.GetComponentInChildren<Target>();
        if (target != null)
            target.OnHit += () => OnTargetHit(obj, respawn);
    }

    Transform PickFreePoint(Transform[] points)
    {
        List<Transform> free = new List<Transform>();
        foreach (Transform p in points)
            if (!active.ContainsValue(p)) free.Add(p);

        if (free.Count == 0) return null;
        return free[Random.Range(0, free.Count)];
    }

    void OnTargetHit(GameObject obj, System.Action respawn)
    {
        active.Remove(obj);
        Destroy(obj, respawnDelay);
        StartCoroutine(RespawnLater(respawn));
    }

    IEnumerator RespawnLater(System.Action respawn)
    {
        yield return new WaitForSeconds(respawnDelay);

        LevelManager level = LevelManager.Instance;
        if (level != null && !level.IsRunning) yield break;

        respawn(); // même type de cible, nouvelle position
    }

    public void ApplyDifficulty(DifficultySettings d)
    {
        settings = d;
        groundCount = d.groundCount;
        flyingCount = d.flyingCount;
    }
}