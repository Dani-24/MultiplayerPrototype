using TMPro;
using UnityEngine;

public class RoomCell : MonoBehaviour
{
    public TMP_Text roomId;
    public TMP_Text roomHost;
    public TMP_Text roomDate;

    public void SetRoomId(string id)
    {
        roomId.text = id;
    }

    public void SetRoomHost(string name)
    {
        roomHost.text = name + "'s Room";
    }

    public void SetRoomDate(string num)
    {
        roomDate.text = num;
    }

    public void JoinRoom()
    {
        ConnectionManager.Instance.JoinRoom(int.Parse(roomId.text));
    }
}
