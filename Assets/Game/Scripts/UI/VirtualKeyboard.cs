using TMPro;
using UnityEngine;
using UnityEngine.UI;

// clavier virtuel : fabrique ses touches au démarrage et écrit dans un champ texte
public class VirtualKeyboard : MonoBehaviour
{
    public TMP_InputField target; // le champ où s'écrit le nom
    public Button keyPrefab;   // une touche modèle (bouton avec un texte)
    public RectTransform keysParent; // l'objet qui contiendra les touches
    public float keySize = 50f; // taille d'une touche
    public float spacing = 6f;  // espace entre les touches

    // les lignes du clavier, de haut en bas
    readonly string[] rows = { "AZERTYUIOP", "QSDFGHJKLM", "WXCVBN" };

    private void Start()
    {
        float step = keySize + spacing;

        // une touche par lettre
        for (int r = 0; r < rows.Length; r++)
        {
            string row = rows[r];
            for (int i = 0; i < row.Length; i++)
            {
                string letter = row[i].ToString();
                float x = (i - (row.Length - 1) / 2f) * step;   // ligne centrée
                float y = -r * step;
                CreateKey(letter, x, y, keySize, () => Type(letter));
            }
        }

        // ligne du bas : espace et effacer
        float bottom = -rows.Length * step;
        CreateKey("Space", -step * 1.5f, bottom, keySize * 4f, () => Type(" "));
        CreateKey("<-", step * 2.5f, bottom, keySize * 2f, Backspace);
    }

    // crée une touche à partir du modèle
    void CreateKey(string label, float x, float y, float width, UnityEngine.Events.UnityAction action)
    {
        Button key = Instantiate(keyPrefab, keysParent);
        RectTransform rt = key.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(width, keySize);

        key.GetComponentInChildren<TMP_Text>().text = label;
        key.onClick.AddListener(action);  // ce que fait la touche quand on la presse
        key.gameObject.SetActive(true);
    }

    public void Type(string s)
    {
        if (target == null) return;
        if (target.characterLimit > 0 && target.text.Length >= target.characterLimit) return;  // nom trop long
        target.text += s;
    }

    public void Backspace()
    {
        if (target == null || target.text.Length == 0) return;
        target.text = target.text.Substring(0, target.text.Length - 1);  // retire la dernière lettre
    }
}