using UnityEngine;
using UnityEngine.UI;

public class CanvasS_StateConn : MonoBehaviour
{
    [SerializeField] Image connectedImg;
    CanvasGroup canGroup;

    private void Start()
    {
        canGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (SceneManagerScript.Instance.gameState == SceneManagerScript.GameState.Loading) { canGroup.alpha = 0; } else { canGroup.alpha = 1; }
        if (ConnectionManager.Instance.IsConnected()) { connectedImg.color = Color.green; } else { connectedImg.color = Color.red; }
    }
}
