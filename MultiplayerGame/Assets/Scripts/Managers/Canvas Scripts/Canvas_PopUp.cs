using TMPro;
using UnityEngine;
using static UI_Manager;

public class Canvas_PopUp : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text msg_text;
    [SerializeField] float cont;

    void Start()
    {
        msg_text.text = UI_Manager.Instance.popUpMsgLog.msg;
        cont = UI_Manager.Instance.popUpMsgLog.duration;

        panel.SetActive(UI_Manager.Instance.popUpMsgLog.visible);
    }

    void Update()
    {
        UI_Manager.Instance.currentCanvasMenu = GameUIs.Msg_Log;

        if (cont > 0)
            cont -= Time.deltaTime;
        else
        {
            UI_Manager.Instance.CloseAll();

            if (UI_Manager.Instance.popUpMsgLog.goToThisScene != "" && UI_Manager.Instance.popUpMsgLog.goToThisScene != SceneManagerScript.Instance.sceneName)
                SceneManagerScript.Instance.ChangeScene(UI_Manager.Instance.popUpMsgLog.goToThisScene, ConnectionManager.Instance.isHosting);

            Destroy(gameObject);
        }
    }
}
