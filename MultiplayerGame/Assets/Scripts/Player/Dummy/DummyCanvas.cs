using TMPro;
using UnityEngine;

public class DummyCanvas : MonoBehaviour
{
    RectTransform trans;

    GameObject player;

    [SerializeField]TMP_Text dmgText;

    void Start()
    {
        trans = GetComponent<RectTransform>();

        player = SceneManagerScript.Instance.GetOwnPlayerInstance();
    }

    void Update()
    {
        Vector3 dir = player.GetComponent<PlayerOrbitCamera>().GetCameraTransform().position - trans.position;
        trans.rotation = Quaternion.LookRotation(dir);

        trans.Rotate(Vector3.up, 180);

        dmgText.text = GetComponentInParent<Dummy>().dmgReceived.ToString();
    }
}
