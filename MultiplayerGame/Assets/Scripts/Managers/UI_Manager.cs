using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public GameUIs currentCanvasMenu;

    [SerializeField] List<CanvasMenu> canvasMenus = new List<CanvasMenu>();

    [SerializeField] GameObject connectionStateCanvas;

    [Header("Data from UIs")]
    public string userName;

    [Header("Debug")]
    public bool debugUIs = false;
    [SerializeField] GameObject debugConsole;
    [SerializeField] GameObject debugAnalysis;

    public bool openSettings;
    public bool openNetSettings;
    public bool gameplayMenuCreated = false;

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
        else if (openNetSettings && currentCanvasMenu != GameUIs.Sett_Connection)
        {
            currentCanvasMenu = GameUIs.Sett_Connection;
        }

        for (int i = 0; i < canvasMenus.Count; i++)
        {
            if (canvasMenus[i].menu == currentCanvasMenu)
            {
                if (currentCanvasMenu == GameUIs.Gameplay && gameplayMenuCreated)
                {
                    continue;
                }
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
                break;
            case GameUIs.Gameplay:
                SceneManagerScript.Instance.gameState = SceneManagerScript.GameState.Gameplay;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameUIs.Settings:
            case GameUIs.Sett_Connection:
                SceneManagerScript.Instance.gameState = SceneManagerScript.GameState.Settings;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    public void ToggleSettings()
    {
        if(!openNetSettings) openSettings = !openSettings;
    }

    public void ToggleNetSettings()
    {
        if (!openSettings) openNetSettings = !openNetSettings;
    }

    public enum GameUIs
    {
        Title,
        Gameplay,
        Settings,
        Sett_Connection,
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
}
