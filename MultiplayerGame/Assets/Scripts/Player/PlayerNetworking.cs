using System.IO;
using UnityEngine;

public class PlayerNetworking : MonoBehaviour
{
    public bool isOwnByClient;

    [SerializeField] AudioListener audioListener;

    MemoryStream stream;

    [Header("Debug")]
    public bool serialize = false;

    void Start()
    {

    }

    void Update()
    {
        if (isOwnByClient) { audioListener.enabled = true; } else { audioListener.enabled = false; }

        if (serialize)
        {
            SerializeJson();
            DeserializeJson();
        }
    }

    #region Serialization

    void SerializeJson()
    {
        var pck = new PlayerPackage();

        // Asignar variables de "pck"
        pck.moveInput = GetComponent<PlayerMovement>().GetMoveInput();

        string json = JsonUtility.ToJson(pck);
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }

    void DeserializeJson()
    {
        var pck = new PlayerPackage();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        pck = JsonUtility.FromJson<PlayerPackage>(json);

        // Debug Log
        Debug.Log(pck.moveInput);
    }
    #endregion
}

[System.Serializable]
class PlayerPackage
{
    public Vector2 moveInput;

    public bool isRunning = false;
    public bool isJumping = false;

    public bool isShooting = false;
    public bool isSubWeapon = false;
}