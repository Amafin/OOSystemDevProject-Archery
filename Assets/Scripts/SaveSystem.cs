using System.IO;
using UnityEngine;

// lit et écrit la sauvegarde dans un fichier JSON
public static class SaveSystem
{
    // dossier propre à l'appareil (sur Quest : stockage interne de l'application)
    static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    static SaveData data;

    // les données : chargées depuis le fichier la première fois qu'on les demande
    public static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public static void Load()
    {
        if (File.Exists(FilePath))
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));

        if (data == null) data = new SaveData();   // pas de fichier (ou fichier illisible) : on repart de zéro
    }

    public static void Save()
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));   // true = JSON lisible, avec retours à la ligne
    }

    // pour les tests et la démo
    public static void ResetAll()
    {
        data = new SaveData();
        Save();
    }
}