using UnityEngine;

public class PoolPlatform : MonoBehaviour
{
    public bool changeWaterLevel = false;

    [Header("Propierties")]
    [SerializeField] float speed = 1f;
    [SerializeField] float targetY;

    private void FixedUpdate()
    {
        if (changeWaterLevel && transform.localPosition.y > targetY)
        {
            transform.Translate(speed * Time.deltaTime * -Vector3.up);
        }
        else if (changeWaterLevel)  // Just to be secure
        {
            transform.localPosition.Set(0, transform.localPosition.y, 0);
        }
    }

    public void ChangeWaterLevel()
    {
        changeWaterLevel = true;
    }
}
