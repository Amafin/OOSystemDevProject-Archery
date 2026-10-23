using UnityEngine;

public interface IArrowHittable
{
    void OnArrowHit(Vector3 hitPoint, Vector3 surfaceNormal);
}