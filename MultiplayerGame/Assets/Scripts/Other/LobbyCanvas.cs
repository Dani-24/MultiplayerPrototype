using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour
{
    [SerializeField] GameObject roomPanel;

    [SerializeField] TMP_Text roomTitle;

    [SerializeField] List<TMP_Text> roomPlayersTexts = new List<TMP_Text>();

    void Start()
    {
        roomPanel.SetActive(false);
    }

    void Update()
    {
        roomPanel.SetActive(ConnectionManager.Instance.IsConnected());

        for (int i = 0; i < roomPlayersTexts.Count; i++)
        {
            roomPlayersTexts[i].text = "... ... ...";
            roomPlayersTexts[i].gameObject.GetComponentInChildren<Image>().color = new Vector4(1, 1, 1, 0.2f);
        }

        for (int i = 0; i < ConnectionManager.Instance.playerPackages.Count; i++)
        {
            roomPlayersTexts[i].text = ConnectionManager.Instance.playerPackages[i].userName;
            roomPlayersTexts[i].gameObject.GetComponentInChildren<Image>().color = SceneManagerScript.Instance.GetTeamColor(ConnectionManager.Instance.playerPackages[i].teamTag);
        }
    }
}
