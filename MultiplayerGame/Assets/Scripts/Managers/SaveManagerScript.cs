using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManagerScript
{
    public static void SaveGame(SaveData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/save.sus";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Game Data has been saved");
    }

    public static SaveData LoadGame()
    {
        string path = Application.persistentDataPath + "/save.sus";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;

            stream.Close();

            Debug.Log("Save Data has been loaded succesfully");

            return data;
        }
        else
        {
            Debug.Log("Save Data File not found in " + path);

            return null;
        }
    }
}

[System.Serializable]
public class SaveData
{
    // User
    public string name;

    // Equipment
    public int mainW;
    public int mainBomb;

    // Sens
    public float mouseSensX; 
    public float mouseSensY;
    public float padSensX; 
    public float padSensY;

    // Graphics
    public int quality;
    public int windowMode;
    public int resolution;

    // Audio
    public float masterV;
    public float musicV;
    public float sfxV;
}
