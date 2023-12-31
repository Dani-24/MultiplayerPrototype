using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CanvasS_Settings;

public class CanvasS_Conn : MonoBehaviour
{
    [SerializeField] Button firstSelectButt;

    [SerializeField] PlayMode playMode = PlayMode.None;

    [SerializeField] PanelOptions currentPanel;

    [SerializeField] List<ConnectionPanel> panels = new List<ConnectionPanel>();

    [Header("Connection Config UI")]
    [SerializeField] TMP_InputField ipInputF;
    [SerializeField] TMP_Text roomID;

    void Start()
    {
        SelectDefaultButton();

        if (ConnectionManager.Instance.IsConnected()) currentPanel = PanelOptions.Room;
    }

    void Update()
    {
        foreach (var panel in panels)
        {
            if (panel.option == currentPanel) panel.panel.SetActive(true); else panel.panel.SetActive(false);
        }

        if (!UI_Manager.Instance.openNetSettings) Button_OnClose();

        if (currentPanel == PanelOptions.Room)
        {
            roomID.text = "Room ID: " + ConnectionManager.Instance.GetHostIP() + ":" + ConnectionManager.Instance.GetCurrentPort();
        }
    }

    void SelectDefaultButton()
    {
        firstSelectButt.Select();
    }

    #region Buttons Inputs

    // GameModes
    public void Button_Lan()
    {
        playMode = PlayMode.Lan;
        currentPanel = PanelOptions.ChooseHosting;

        ConnectionManager.Instance.localHost = false;
    }

    public void Button_Online()
    {
        playMode = PlayMode.Online;

        ConnectionManager.Instance.localHost = false;
        // WIP
    }

    public void Button_Local()
    {
        playMode = PlayMode.Local;
        currentPanel = PanelOptions.ChooseHosting;

        ConnectionManager.Instance.localHost = true;
    }

    // Hosting
    public void Button_HostSelect(bool host)
    {
        ConnectionManager.Instance.isHosting = host;

        switch (playMode)
        {
            case PlayMode.Local:

                ConnectionManager.Instance.SetIP("");
                ConnectionManager.Instance.SetPort(0);

                ConnectionManager.Instance.reconnect = true;
                currentPanel = PanelOptions.Room;

                break;
            case PlayMode.Lan:

                if (host)
                {
                    ConnectionManager.Instance.SetIP(ConnectionManager.Instance.GetLocalIPv4());
                    ConnectionManager.Instance.reconnect = true;
                    currentPanel = PanelOptions.Room;
                }
                else
                {
                    currentPanel = PanelOptions.Joining;
                }

                break;
        }
    }

    public void ReturnToGameModes()
    {
        playMode = PlayMode.None;
        currentPanel = PanelOptions.Gamemodes;
    }

    // Joining
    public void ReturnToHosting()
    {
        currentPanel = PanelOptions.ChooseHosting;
    }

    public void Button_Connect()
    {
        string[] inputField = ipInputF.text.Split(':');

        ConnectionManager.Instance.SetIP(inputField[0]);
        ConnectionManager.Instance.SetPort(int.Parse(inputField[1]));
        ConnectionManager.Instance.reconnect = true;

        currentPanel = PanelOptions.Room;

        // Aqui se deberia hacer un check para ver si la IP introducida conecta a algo / es el formato que se pide
    }

    // Room
    public void Button_Disconnect()
    {
        ConnectionManager.Instance.disconnect = true;
        currentPanel = PanelOptions.Gamemodes;
    }

    // Close
    public void Button_OnClose()
    {
        UI_Manager.Instance.openNetSettings = false;
        UI_Manager.Instance.currentCanvasMenu = UI_Manager.GameUIs.Gameplay;
        Destroy(gameObject);
    }

    public void Button_OnExit() // Close Software
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    #endregion

    public enum PlayMode
    {
        None,
        Lan,
        Online,
        Local
    }

    public enum PanelOptions
    {
        Gamemodes,
        ChooseHosting,
        Joining,
        Room
    }

    [System.Serializable]
    public class ConnectionPanel
    {
        public ConnectionPanel(PanelOptions option, GameObject panel)
        {
            this.option = option;
            this.panel = panel;
        }

        public PanelOptions option;
        public GameObject panel;
    }
}
