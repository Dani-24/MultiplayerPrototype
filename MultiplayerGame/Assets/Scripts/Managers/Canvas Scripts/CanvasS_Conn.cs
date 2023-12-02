using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CanvasS_Conn : MonoBehaviour
{
    [Header("Connection Config UI")]

    [SerializeField] TMP_InputField ipInputF;
    [SerializeField] TMP_InputField portInputF;

    [SerializeField] Image hostButton;

    [SerializeField] Button connectionFirstSelectButton;

    void Start()
    {
        SelectDefaultButton();
    }

    void Update()
    {
        if (ConnectionManager.Instance.isHosting) { hostButton.color = Color.green; } else { hostButton.color = Color.white; }

        UI_Manager.Instance.userIP = ipInputF.text;
        if (portInputF.text != "") { UI_Manager.Instance.userPort = int.Parse(portInputF.text); } else { UI_Manager.Instance.userPort = 0; }

        if(UI_Manager.Instance.openSettings == false) // Closing Settings by ESC from PlayerMovement Inputs
        {
            Button_OnClose();
        }
    }

    void SelectDefaultButton()
    {
        connectionFirstSelectButton.Select();
    }

    #region Buttons Inputs

    public void Button_OnClose()
    {
        UI_Manager.Instance.currentCanvasMenu = UI_Manager.GameUIs.Gameplay;
        UI_Manager.Instance.openSettings = false;
        Destroy(gameObject);
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

    public void Button_OnExit() // Close Software
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    #endregion

}
