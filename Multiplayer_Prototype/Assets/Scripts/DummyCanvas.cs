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

        player = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        Vector3 dir = player.transform.position - trans.position;
        trans.rotation = Quaternion.LookRotation(dir);

        trans.Rotate(Vector3.up, 180);

        dmgText.text = GetComponentInParent<Dummy>().HP.ToString();
    }
}
