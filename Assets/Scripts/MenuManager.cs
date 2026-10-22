using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance; // accessible partout : MenuManager.Instance

    [Header("Panels")]
    public GameObject menuPanel;        // le menu sur le pupitre
    public GameObject trainingPanel;    // bouton Quit pendant l'entraînement
    public GameObject lectern;  // le pupitre 3D (caché pendant une partie)

    [Header("Difficulty")]
    public DifficultySettings[] difficulties;  // Training, Easy, Normal, Hard
    public int defaultDifficulty = 2;  // Normal par défaut
    public TextMeshProUGUI difficultyText;

    [Header("Levels")]
    public LevelData[] levels;  // Level_1 à Level_4, dans l'ordre
    public LevelData trainingLevel; // contenu de l'entraînement (cibles fixes)
    public GameObject levelRow; // caché en entraînement
    public TextMeshProUGUI levelText;

    [Header("Equipment")]
    public TextMeshProUGUI bowText;
    public TextMeshProUGUI arrowText;

    [Header("Other")]
    public Slider volumeSlider;
    public Button playButton;

    int difficultyIndex;
    int levelIndex;
    int bowIndex;
    int arrowIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // volume sauvegardé
        float volume = SaveSystem.Data.volume;
        AudioListener.volume = volume;
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(volume);

        difficultyIndex = defaultDifficulty;

        // on part de l'équipement déjà sélectionné par EquipmentManager
        EquipmentManager eq = EquipmentManager.Instance;
        bowIndex = System.Array.IndexOf(eq.bows, eq.SelectedBow);
        arrowIndex = System.Array.IndexOf(eq.arrows, eq.SelectedArrow);
        if (bowIndex < 0) bowIndex = 0;
        if (arrowIndex < 0) arrowIndex = 0;

        // on se place sur le niveau débloqué le plus avancé
        levelIndex = 0;
        for (int i = 0; i < levels.Length; i++)
            if (eq.IsUnlocked(levels[i])) levelIndex = i;

        ShowMenu();
    }

    // affiche le menu (au démarrage et en revenant d'une partie)
    public void ShowMenu()
    {
        SetLecternVisible(true);
        FaceTargetOnStart face = FindAnyObjectByType<FaceTargetOnStart>();
        if (face != null) face.FaceTarget();   // le joueur se retrouve face au pupitre
        if (menuPanel != null) menuPanel.SetActive(true);
        if (trainingPanel != null) trainingPanel.SetActive(false);
        Refresh();
    }

    public void SetLecternVisible(bool visible)
    {
        if (lectern != null) lectern.SetActive(visible);
    }

    // bouton Play
    public void Play()
    {
        EquipmentManager eq = EquipmentManager.Instance;
        // sécurité : on ne lance pas avec un équipement verrouillé
        if (!eq.IsUnlocked(eq.SelectedBow) || !eq.IsUnlocked(eq.SelectedArrow)) return;

        DifficultySettings d = difficulties[difficultyIndex];
        LevelData chosenLevel = d.isTraining ? trainingLevel : levels[levelIndex];
        if (chosenLevel == null) return;

        SetLecternVisible(d.isTraining);   // en entraînement on garde le pupitre pour le bouton Quit
        if (menuPanel != null) menuPanel.SetActive(false);
        if (trainingPanel != null) trainingPanel.SetActive(d.isTraining);

        LevelManager.Instance.StartLevel(d, chosenLevel);   //+ le niveau
    }

    // bouton Quit de l'entraînement
    public void QuitTraining()
    {
        LevelManager.Instance.ReturnToMenu();
    }

    // Difficulté
    public void NextDifficulty() { difficultyIndex = Wrap(difficultyIndex + 1, difficulties.Length); Refresh(); }
    public void PrevDifficulty() { difficultyIndex = Wrap(difficultyIndex - 1, difficulties.Length); Refresh(); }

    // Niveau
    public void NextLevel() => ChangeLevel(1);
    public void PrevLevel() => ChangeLevel(-1);

    void ChangeLevel(int step)
    {
        EquipmentManager eq = EquipmentManager.Instance;
        // on avance jusqu'au prochain niveau débloqué (au pire on revient au même)
        for (int i = 0; i < levels.Length; i++)
        {
            levelIndex = Wrap(levelIndex + step, levels.Length);
            if (eq.IsUnlocked(levels[levelIndex])) break;
        }
        Refresh();
    }
    // Arc
    public void NextBow() => ChangeBow(1);
    public void PrevBow() => ChangeBow(-1);

    void ChangeBow(int step)
    {
        EquipmentManager eq = EquipmentManager.Instance;
        // on avance jusqu'au prochain arc débloqué (au pire on revient au même)
        for (int i = 0; i < eq.bows.Length; i++)
        {
            bowIndex = Wrap(bowIndex + step, eq.bows.Length);
            if (eq.IsUnlocked(eq.bows[bowIndex])) break;
        }
        eq.SelectBow(eq.bows[bowIndex]);
        Refresh();
    }

    // Flèche
    public void NextArrow() => ChangeArrow(1);
    public void PrevArrow() => ChangeArrow(-1);

    void ChangeArrow(int step)
    {
        EquipmentManager eq = EquipmentManager.Instance;
        for (int i = 0; i < eq.arrows.Length; i++)
        {
            arrowIndex = Wrap(arrowIndex + step, eq.arrows.Length);
            if (eq.IsUnlocked(eq.arrows[arrowIndex])) break;
        }
        eq.SelectArrow(eq.arrows[arrowIndex]);
        Refresh();
    }

    // Volume
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        SaveSystem.Data.volume = value;
        SaveSystem.Save();
    }

    // bouton Reset progress : efface la sauvegarde et remet le menu à zéro (tests / démo)
    public void ResetProgress()
    {
        SaveSystem.ResetAll();

        // équipement de départ
        EquipmentManager eq = EquipmentManager.Instance;
        eq.SelectBestUnlocked();
        eq.SpawnBow();
        bowIndex = System.Array.IndexOf(eq.bows, eq.SelectedBow);
        arrowIndex = System.Array.IndexOf(eq.arrows, eq.SelectedArrow);
        levelIndex = 0;

        // volume par défaut
        AudioListener.volume = SaveSystem.Data.volume;
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(SaveSystem.Data.volume);

        // classement vidé
        if (Leaderboard.Instance != null) Leaderboard.Instance.Refresh();

        Refresh();
    }

    // met à jour tous les textes du menu
    void Refresh()
    {
        EquipmentManager eq = EquipmentManager.Instance;
        DifficultySettings d = difficulties[difficultyIndex];

        if (difficultyText != null) difficultyText.text = d.displayName;

        //niveau (caché en entraînement)
        if (levelRow != null) levelRow.SetActive(!d.isTraining);
        if (levels.Length > 0)
        {
            LevelData l = levels[levelIndex];
            if (levelText != null) levelText.text = "Level " + (levelIndex + 1) + "/" + levels.Length + " - " + l.displayName;
            if (Leaderboard.Instance != null && !d.isTraining) Leaderboard.Instance.SetLevel(l.displayName);   // le classement suit le niveau choisi
        }

        // arc
        BowData bow = eq.bows[bowIndex];
        int lockedBows = 0;
        foreach (BowData b in eq.bows) if (!eq.IsUnlocked(b)) lockedBows++;
        if (bowText != null) bowText.text = bow.displayName;

        // flèche
        ArrowType arrow = eq.arrows[arrowIndex];
        int lockedArrows = 0;
        foreach (ArrowType a in eq.arrows) if (!eq.IsUnlocked(a)) lockedArrows++;
        if (arrowText != null) arrowText.text = arrow.displayName;

        // sécurité : Play grisé si l'équipement n'est pas débloqué
        if (playButton != null)
            playButton.interactable = eq.IsUnlocked(bow) && eq.IsUnlocked(arrow);
    }

    // fait boucler un index : après le dernier on revient au premier, et inversement
    int Wrap(int i, int count)
    {
        return (i % count + count) % count;
    }
}