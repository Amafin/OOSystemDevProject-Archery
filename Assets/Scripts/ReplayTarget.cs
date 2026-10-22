using UnityEngine;

public class ReplayTarget : MonoBehaviour, IArrowHittable
{
    public void OnArrowHit(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        LevelManager level = LevelManager.Instance;
        if (level != null && !level.IsRunning)   // que quand le niveau est fini
            level.StartLevel();
    }
}