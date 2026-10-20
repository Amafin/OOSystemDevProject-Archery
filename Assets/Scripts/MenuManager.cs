using TMPro;
using UnityEngine;
using UnityEngine.UI;

// le pupitre du menu : difficulté, arc, flèche, volume, jouer
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panneaux")]
    public GameObject menuPanel; // le menu principal
    public GameObject trainingPanel; // le bouton "Quitter" visible pendant l'entraînement

    [Header("Difficultés (dans l'ordre affiché)")]
    public DifficultySettings[] difficulties;// Entraînement, Facile, Normal, Difficile
    public int defaultDifficulty = 2;   // niveau choisi au départ (2 = Normal)

    [Header("Textes")]
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI bowText;
    public TextMeshProUGUI bowDescription;
    public TextMeshProUGUI arrowText;
    public TextMeshProUGUI arrowDescription;

    [Header("Volume")]
    public Slider volumeSlider;

    int difficultyIndex;
    int bowIndex;
    int arrowIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // volume enregistré
        AudioListener.volume = SaveSystem.Data.volume;
        if (volumeSlider != null) volumeSlider.value = SaveSystem.Data.volume;

        difficultyIndex = defaultDifficulty;

        // on part de l'équipement actuellement sélectionné
        EquipmentManager eq = EquipmentManager.Instance;
        if (eq != null)
        {
            bowIndex = System.Array.IndexOf(eq.bows, eq.SelectedBow);
            arrowIndex = System.Array.IndexOf(eq.arrows, eq.SelectedArrow);
        }

        ShowMenu();
    }

    public void ShowMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (trainingPanel != null) trainingPanel.SetActive(false);
        Refresh();
    }

    public void Play()
    {
        if (menuPanel != null) menuPanel.SetActive(false);

        DifficultySettings d = difficulties[difficultyIndex];
        if (trainingPanel != null) trainingPanel.SetActive(d.isTraining); // bouton Quitter en entraînement

        LevelManager.Instance.StartLevel(d);
    }

    public void QuitTraining()
    {
        LevelManager.Instance.ReturnToMenu();
    }

    // boutons ◀ ▶
    public void NextDifficulty() { difficultyIndex = Wrap(difficultyIndex + 1, difficulties.Length); Refresh(); }
    public void PrevDifficulty() { difficultyIndex = Wrap(difficultyIndex - 1, difficulties.Length); Refresh(); }

    public void NextBow() { ChangeBow(+1); }
    public void PrevBow() { ChangeBow(-1); }
    public void NextArrow() { ChangeArrow(+1); }
    public void PrevArrow() { ChangeArrow(-1); }

    void ChangeBow(int step)
    {
        EquipmentManager eq = EquipmentManager.Instance;
        bowIndex = Wrap(bowIndex + step, eq.bows.Length);
        eq.SelectBow(eq.bows[bowIndex]); // refusé tout seul s'il est verrouillé
        Refresh();
    }

    void ChangeArrow(int step)
    {
        EquipmentManager eq = EquipmentManager.Instance;
        arrowIndex = Wrap(arrowIndex + step, eq.arrows.Length);
        eq.SelectArrow(eq.arrows[arrowIndex]);
        Refresh();
    }

    // volume
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // volume général de tout le jeu
        SaveSystem.Data.volume = value;
        SaveSystem.Save();
    }

    // mise à jour des textes
    void Refresh()
    {
        if (difficultyText != null)
            difficultyText.text = difficulties[difficultyIndex].displayName;

        EquipmentManager eq = EquipmentManager.Instance;
        if (eq == null) return;

        BowData bow = eq.bows[bowIndex];
        bool bowOk = eq.IsUnlocked(bow);
        if (bowText != null) bowText.text = bowOk ? bow.displayName : bow.displayName + " (verrouillé)";
        if (bowDescription != null) bowDescription.text = bowOk ? bow.description : "Réussissez des parties pour le débloquer.";

        ArrowType arrow = eq.arrows[arrowIndex];
        bool arrowOk = eq.IsUnlocked(arrow);
        if (arrowText != null) arrowText.text = arrowOk ? arrow.displayName : arrow.displayName + " (verrouillé)";
        if (arrowDescription != null) arrowDescription.text = arrowOk ? arrow.description : "Réussissez des parties pour la débloquer.";
    }

    // fait boucler un index : après le dernier, on revient au premier
    int Wrap(int i, int count)
    {
        return (i % count + count) % count;
    }
}