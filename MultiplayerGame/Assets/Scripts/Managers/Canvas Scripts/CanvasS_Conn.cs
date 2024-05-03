using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CanvasS_Conn : MonoBehaviour
{
    [SerializeField] Button firstSelectButt;

    [SerializeField] PlayMode playMode = PlayMode.None;

    [SerializeField] PanelOptions currentPanel;

    [SerializeField] List<ConnectionPanel> panels = new List<ConnectionPanel>();

    [Header("Connection Config UI")]
    [SerializeField] TMP_InputField ipInputF;
    [SerializeField] TMP_Text roomID;

    [Header("Room Players")]
    [SerializeField] List<TMP_Text> roomPlayersTexts = new List<TMP_Text>();

    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject startGameButton2;

    [SerializeField] Button gearButton;

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
            roomID.text = "Room ID: " + ConnectionManager.Instance.GetHostIP(); // + ":" + ConnectionManager.Instance.GetCurrentPort();

            // AÑADIR AQUI UN BOTON DE COPY ROOM ID

            if (ConnectionManager.Instance.isHosting && SceneManagerScript.Instance.sceneName == ConnectionManager.Instance.lobbyScene)
            {
                startGameButton.SetActive(true);
                startGameButton2.SetActive(true);
            }
            else
            {
                startGameButton.SetActive(false);
                startGameButton2.SetActive(false);
            }
        }

        for (int i = 0; i < roomPlayersTexts.Count; i++)
        {
            if (i < ConnectionManager.Instance.playerPackages.Count)
            {
                roomPlayersTexts[i].text = ConnectionManager.Instance.playerPackages[i].userName;
                roomPlayersTexts[i].gameObject.GetComponentInChildren<Image>().color = SceneManagerScript.Instance.GetTeamColor(ConnectionManager.Instance.playerPackages[i].teamTag);
                continue;
            }

            roomPlayersTexts[i].text = "... ... ...";
            roomPlayersTexts[i].gameObject.GetComponentInChildren<Image>().color = new Vector4(1, 1, 1, 0.2f);
        }

        if (SceneManagerScript.Instance.sceneName != ConnectionManager.Instance.lobbyScene)
            gearButton.interactable = false;
        else
            gearButton.interactable = true;
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
                    ConnectionManager.Instance.SetIP(ConnectionManager.Instance.GetLocalIPAddress());
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
        //ConnectionManager.Instance.SetPort(int.Parse(inputField[1]));
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

    public void Button_CopyCode()
    {
        GUIUtility.systemCopyBuffer = ConnectionManager.Instance.GetHostIP();
    }

    public void Button_PasteCode()
    {
        ipInputF.text = GUIUtility.systemCopyBuffer;
    }

    // UI
    public void OpenSettings()
    {
        UI_Manager.Instance.ToggleSettings();
    }

    public void OpenGear()
    {
        UI_Manager.Instance.ToggleGear();
    }

    public void StartGame(string scene)
    {
        UI_Manager.Instance.PopUp_LogMessage("Starting Battle", 1, true, scene);
        SceneManagerScript.Instance.TeamsForBattle();
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
