using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;

    void FixedUpdate()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
