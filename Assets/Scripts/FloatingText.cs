using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float lifeTime = 1f;     // durée d'affichage (s)
    public float riseSpeed = 0.5f;  // vitesse de montée (m/s)

    TextMeshPro text;
    float timer;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void SetPoints(int points)
    {
        SetText("+" + points);
    }

    public void SetText(string message)
    {
        text.text = message;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;   // monte

        // toujours face au joueur
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        // s'efface petit à petit
        Color c = text.color;
        c.a = 1f - timer / lifeTime;
        text.color = c;

        if (timer >= lifeTime) Destroy(gameObject);
    }
}