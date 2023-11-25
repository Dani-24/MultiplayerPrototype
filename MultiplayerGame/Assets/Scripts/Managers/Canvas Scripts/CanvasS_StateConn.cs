using UnityEngine;
using UnityEngine.UI;

public class CanvasS_StateConn : MonoBehaviour
{
    [SerializeField] Image connectedImg;

    void Update()
    {
        if (SceneManagerScript.Instance.gameObject.GetComponent<ConnectionManager>().IsConnected()) { connectedImg.color = Color.green; } else { connectedImg.color = Color.red; }
    }
}
