using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;

    

    void FixedUpdate()
    {
        if (!GetComponent<NetGameObject>().connectedToServer)
        {
            transform.Rotate(0, speed * Time.deltaTime, 0);
            GetComponent<NetGameObject>().netValue = transform.rotation.eulerAngles.y;
        }
        else
        {
            Quaternion rot = Quaternion.Euler(0, Mathf.LerpAngle(transform.rotation.eulerAngles.y, GetComponent<NetGameObject>().netValue, speed * Time.deltaTime), 0);
            transform.rotation = rot;
        }
    }
}
