using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour
{
    [SerializeField] GameObject titlePanel;
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
        titlePanel.SetActive(SceneManagerScript.Instance.gameState == SceneManagerScript.GameState.Title);

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
    }
}
