using TMPro;
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

    [SerializeField] Button firstSelectButton;

    void Update()
    {
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

    public void SelectDefaultButton()
    {
        firstSelectButton.Select();
    }

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
}
