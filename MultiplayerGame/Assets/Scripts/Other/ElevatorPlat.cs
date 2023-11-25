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

    // UP
    private void FixedUpdate()
    {
        if (elevator.position.y < (maxHeight) && goUp)
        {
            elevator.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
    }

    // DOWN
    private void Update()
    {
        if (elevator.position.y > startHeight && !goUp)
        {
            elevator.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }

        // Avoid Bugs
        if (elevator.position.y < startHeight)
        {
            elevator.position.Set(0, startHeight, 0);
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
