using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public GameUIs currentCanvasMenu;

    [SerializeField] List<CanvasMenu> canvasMenus = new List<CanvasMenu>();

    [SerializeField] GameObject connectionStateCanvas;

    [Header("Data from UIs")]
    public string userName;
    public string defaultName = "Player";

    [Header("Debug")]
    public bool debugUIs = false;
    [SerializeField] GameObject debugConsole;
    [SerializeField] GameObject debugAnalysis;

    public bool openSettings;
    public bool openNetSettings;
    public bool openGear;
    public bool gameplayMenuCreated = false;
    public bool alreadyShownTitle = false;

    public PopUpMsgLog popUpMsgLog;

    #region Instance

    private static UI_Manager _instance;
    public static UI_Manager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    void Start()
    {
        GameObject canv = Instantiate(connectionStateCanvas);
        canv.transform.SetParent(transform);
        userName = defaultName;
    }

    void Update()
    {
        // Debug UIs
        debugAnalysis.SetActive(debugUIs);
        debugConsole.SetActive(debugUIs);

        #region Manage UIs

        if (openSettings && currentCanvasMenu != GameUIs.Settings)
        {
            currentCanvasMenu = GameUIs.Settings;
        }
        else if (openNetSettings && currentCanvasMenu != GameUIs.Sett_Connection && currentCanvasMenu != GameUIs.Title)
        {
            currentCanvasMenu = GameUIs.Sett_Connection;
        }
        else if(openGear && currentCanvasMenu != GameUIs.Sett_Gear)
        {
            currentCanvasMenu= GameUIs.Sett_Gear;
        }

        for (int i = 0; i < canvasMenus.Count; i++)
        {
            if (canvasMenus[i].menu == currentCanvasMenu)
            {
                if (currentCanvasMenu == GameUIs.Gameplay && gameplayMenuCreated) continue;
                if (!canvasMenus[i].activated)
                {
                    GameObject canv = Instantiate(canvasMenus[i].canvas);
                    canv.transform.SetParent(transform);

                    canvasMenus[i].activated = true;
                }
            }
            else
            {
                canvasMenus[i].activated = false;
            }
        }

        #endregion

        ChangeGameState();
    }

    void ChangeGameState()
    {
        // Scene Game State
        switch (currentCanvasMenu)
        {
            case GameUIs.Title:
                SceneManagerScript.Instance.gameState = SceneManagerScript.GameState.Title;
                Cursor.lockState = CursorLockMode.None;
                alreadyShownTitle = true;
                break;
            case GameUIs.Gameplay:
                SceneManagerScript.Instance.gameState = SceneManagerScript.GameState.Gameplay;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameUIs.Settings:
            case GameUIs.Sett_Connection:
            case GameUIs.Sett_Gear:
                SceneManagerScript.Instance.gameState = SceneManagerScript.GameState.Settings;
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameUIs.Msg_Log:
                SceneManagerScript.Instance.gameState = SceneManagerScript.GameState.Loading;
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }

    public void ToggleSettings()
    {
        openNetSettings = false;
        openSettings = !openSettings;
    }

    public void ToggleNetSettings()
    {
        openSettings = openGear = false;
        openNetSettings = !openNetSettings;
    }

    public void ToggleGear()
    {
        openNetSettings = false;
        openGear = !openGear;
    }

    public void PopUp_LogMessage(string _msg, float _duration = 5.0f, bool _visible = true, string _goToThisScene = "")
    {
        openNetSettings = openSettings = false;
        currentCanvasMenu = GameUIs.Msg_Log;

        popUpMsgLog = new PopUpMsgLog
        {
            msg = _msg,
            duration = _duration,
            visible = _visible,
            goToThisScene = _goToThisScene
        };
    }

    public void CloseAll()
    {
        openNetSettings = openSettings = openGear = false;
        currentCanvasMenu = GameUIs.Gameplay;
    }

    public enum GameUIs
    {
        Title,
        Gameplay,
        Settings,
        Sett_Connection,
        Sett_Gear,
        Msg_Log,
        None
    }

    [System.Serializable]
    public class CanvasMenu
    {
        public CanvasMenu(GameUIs menu, GameObject canvas)
        {
            this.menu = menu;
            this.canvas = canvas;
        }

        public GameUIs menu;
        public GameObject canvas;

        [HideInInspector]
        public bool activated;
    }

    [System.Serializable]
    public struct PopUpMsgLog
    {
        public string msg;
        public float duration;
        public bool visible;
        public string goToThisScene;
    }
}
