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
        startHeight = elevator.localPosition.y;
    }

    private void FixedUpdate()
    {
        if (!GetComponent<NetGameObject>().connectedToServer)
        {
            // UP
            if (elevator.localPosition.y < maxHeight && goUp)
            {
                elevator.Translate(new Vector3(0, speed * Time.deltaTime, 0));
            }

            // Down
            if (elevator.localPosition.y > startHeight && !goUp)
            {
                elevator.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
            }

            #region Avoid Bugs
            if (elevator.localPosition.y < startHeight && !goUp)
            {
                elevator.localPosition.Set(0, startHeight, 0);
            }
            if(elevator.localPosition.y > maxHeight && goUp)
            {
                elevator.localPosition.Set(0, maxHeight, 0);
            }
            #endregion

            GetComponent<NetGameObject>().netValue = elevator.localPosition.y;
        }
        else
        {
            elevator.localPosition = new Vector3(elevator.localPosition.x, Mathf.LerpUnclamped(elevator.localPosition.y, GetComponent<NetGameObject>().netValue, speed * Time.deltaTime), elevator.localPosition.z);
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
