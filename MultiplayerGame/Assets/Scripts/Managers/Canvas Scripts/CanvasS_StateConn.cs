using UnityEngine;
using UnityEngine.UI;

public class CanvasS_StateConn : MonoBehaviour
{
    [SerializeField] Image connectedImg;
    CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (SceneManagerScript.Instance.gameState == SceneManagerScript.GameState.Loading) { canvasGroup.alpha = 0; } else { canvasGroup.alpha = 1; }
        if (ConnectionManager.Instance.IsConnected()) { connectedImg.color = Color.green; } else { connectedImg.color = Color.red; }
    }
}
