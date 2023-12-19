using UnityEngine;

public class ElevatorPlat : MonoBehaviour
{
    [SerializeField] Transform elevator;

    [Header("Elevator Propierties")]
    [SerializeField] float maxHeight;
    [SerializeField] float speed;

    [Header("Debug Info")]
    [SerializeField] float startHeight;
    [SerializeField] bool goUp;

    private void Start()
    {
        startHeight = elevator.position.y;
    }

    private void FixedUpdate()
    {
        if (!GetComponent<NetGameObject>().connectedToServer)
        {
            // UP
            if (elevator.position.y < (maxHeight) && goUp)
            {
                elevator.Translate(new Vector3(0, speed * Time.deltaTime, 0));
            }

            // Down
            if (elevator.position.y > startHeight && !goUp)
            {
                elevator.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
            }

            // Avoid Bugs
            if (elevator.position.y < startHeight)
            {
                elevator.position.Set(0, startHeight, 0);
            }

            GetComponent<NetGameObject>().netValue = elevator.position.y;
        }
        else
        {
            elevator.position = new Vector3(transform.position.x, Mathf.LerpUnclamped(transform.position.y, GetComponent<NetGameObject>().netValue, speed * Time.deltaTime), transform.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(elevator.transform);
    }

    private void OnTriggerStay(Collider other)
    {
        goUp = true;
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
        goUp = false;
    }
}
