using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public bool showUI;
    bool firstShow;

    [Header("Connection Config UI")]
    [SerializeField] GameObject connectionUI;

    [SerializeField] TMP_InputField ipInputF;
    [SerializeField] TMP_InputField portInputF;

    [SerializeField] Image hostButton;
    [SerializeField] Image connectedImg;

    [SerializeField] Button connectionFirstSelectButton;

    [SerializeField] GameObject titleCanvas;
    bool firstTitleShow = false;
    Button mainTitleButton;
    TMP_InputField titleInput;
    string nameFromTitle;

    void Start()
    {
        try
        {
            titleCanvas = GameObject.FindGameObjectWithTag("showOnTitle");
            mainTitleButton = GameObject.FindGameObjectWithTag("titleButt").GetComponent<Button>();
            titleInput = GameObject.FindGameObjectWithTag("titleInput").GetComponent<TMP_InputField>();

            titleCanvas.SetActive(false);
        }
        catch
        {
            Debug.Log("Title Canvas is missing");
        }
    }

    void Update()
    {
        // Title UI
        if (titleCanvas != null)
        {
            switch (GetComponent<CameraManager>().gameState)
            {
                case CameraManager.GameState.Title:
                    if (!titleCanvas.activeInHierarchy)
                    {
                        titleCanvas.SetActive(true);
                        firstTitleShow = true;
                    }

                    nameFromTitle = titleInput.text;

                    break;
                case CameraManager.GameState.Gameplay:
                    if (titleCanvas.activeInHierarchy)
                    {
                        titleCanvas.SetActive(false);
                    }
                    break;
            }

            if (firstTitleShow)
            {
                SelectDefaultTitleButton();
                firstTitleShow = false;
            }
        }

        // Connection UI
        connectionUI.SetActive(showUI);

        if (showUI)
        {
            if (firstShow)
            {
                SelectDefaultButton();
                firstShow = false;
            }
        }
        else { firstShow = true; };

        if (GetComponent<ConnectionManager>().isHosting) { hostButton.color = Color.green; } else { hostButton.color = Color.white; }
        if (GetComponent<ConnectionManager>().IsConnected()) { connectedImg.color = Color.green; } else { connectedImg.color = Color.red; }
    }

    void SelectDefaultButton()
    {
        connectionFirstSelectButton.Select();
    }

    void SelectDefaultTitleButton()
    {
        mainTitleButton.Select();
    }

    #region UI Inputs

    public void Button_OnClose()
    {
        showUI = false;
    }

    public void Button_OnReconnect()
    {
        ConnectionManager.Instance.reconnect = true;
    }

    public void Button_Disconnect()
    {
        ConnectionManager.Instance.disconnect = true;
    }

    public void Button_OnHost()
    {
        ConnectionManager.Instance.isHosting = !ConnectionManager.Instance.isHosting;
    }

    public string GetIpFromInput()
    {
        return ipInputF.text;
    }

    public int GetPortFromInput()
    {
        if (portInputF.text != "")
        {
            return int.Parse(portInputF.text);
        }
        else { return 0; }
    }

    public void Button_OnExit() // Close Software
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    public string GetUserName()
    {
        return nameFromTitle;
    }

    public void Button_OnPlay()
    {
        SceneManagerScript.Instance.addNewOwnPlayer = true;
    }

    #endregion
}
