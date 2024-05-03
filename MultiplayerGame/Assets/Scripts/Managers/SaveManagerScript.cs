using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManagerScript
{
    public static void SaveGame(SaveData data)
    {
        string path = Application.persistentDataPath + "/save.sus";

        BinaryFormatter formatter = new BinaryFormatter();
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
            Debug.Log("No Save Data found in " + path);

            return null;
        }
    }

    public static void DeleteSavedGame()
    {
        string path = Application.persistentDataPath + "/save.sus";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log(path + " save data has been deleted");
        }
        else
            Debug.Log(path + " can't be deleted");
    }

    public static void SaveRuntimeData(RuntimeData data)
    {
        string path = Application.persistentDataPath + "/data.sus";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Runtime data has been saved");
    }

    public static RuntimeData LoadRuntimeData()
    {
        string path = Application.persistentDataPath + "/data.sus";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            RuntimeData data = formatter.Deserialize(stream) as RuntimeData;

            stream.Close();

            Debug.Log("Runtime Data has been loaded succesfully");

            return data;
        }
        else
        {
            Debug.Log("No Runtime Data found in " + path);

            return null;
        }
    }

    public static void DeleteRuntimeData()
    {
        string path = Application.persistentDataPath + "/data.sus";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log(path + " runtime data has been deleted");
        }
        else
            Debug.Log(path + " can't be deleted");
    }
}

[System.Serializable]
public class SaveData
{
    // User
    public string name;

    // Equipment
    public int mainW;
    public int secW;

    // Sens
    public float[] mouseSens = new float[2];
    public float[] padSens = new float[2];

    // Graphics
    public int quality;
    public int windowMode;
    public int resolution;

    // Audio
    public float masterV;
    public float musicV;
    public float sfxV;
}

[System.Serializable]
public class RuntimeData
{
    public float[] savedColor = new float[3];
}
