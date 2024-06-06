using TMPro;
using UnityEngine;

public class RoomCell : MonoBehaviour
{
    public TMP_Text roomId;
    public TMP_Text roomHost;
    public TMP_Text roomPlayers;

    public void SetRoomId(int id)
    {
        roomId.text = id.ToString();
    }

    public void SetRoomHost(string name)
    {
        roomHost.text = name + "'s Room";
    }

    public void SetRoomPlayers(int num)
    {
        roomHost.text = num + "/8";
    }
}
