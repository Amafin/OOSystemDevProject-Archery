using UnityEngine;

// cible "Menu" : une flèche dedans ramène au menu
public class MenuTarget : MonoBehaviour, IArrowHittable
{
    public void OnArrowHit(Vector3 hitPoint, Vector3 surfaceNormal)
    {
        LevelManager level = LevelManager.Instance;
        if (level != null && !level.IsRunning)   // seulement quand la partie est finie
            level.ReturnToMenu();
    }
}