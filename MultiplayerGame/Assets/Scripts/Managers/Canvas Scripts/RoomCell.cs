using System;
using TMPro;
using UnityEngine;
using static CanvasS_Conn;

public class RoomCell : MonoBehaviour
{
    public TMP_Text roomId;
    public TMP_Text roomHost;
    public TMP_Text roomDateText;

    [SerializeField] CanvasS_Conn canvas;

    public void SetRoomId(string id)
    {
        roomId.text = id;
    }

    public void SetRoomHost(string name)
    {
        roomHost.text = name + "'s Room";
    }

    public void SetRoomDate(string date)
    {
        DateTime roomDate = DateTime.Parse(date);
        roomDateText.text = roomDate.ToLocalTime().ToShortTimeString() + " " + roomDate.ToLocalTime().ToShortDateString();
    }

    public void JoinRoom()
    {
        ConnectionManager.Instance.JoinRoom(int.Parse(roomId.text));
        canvas.currentPanel = PanelOptions.Room;
    }
}
