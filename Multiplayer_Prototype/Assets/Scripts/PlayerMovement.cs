using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [SerializeField]
    private GameObject playerBody;

    [SerializeField]
    private Vector2 moveInput;

    [SerializeField]
    private float moveSpeed = 10.0f;

    [SerializeField]
    private float rotationSpeed = 10.0f;

    [HideInInspector]
    public Camera cam;

    [SerializeField]
    float fallingSpeed = 1.0f;

    [Header("Weapon")]
    public GameObject weapon;
    public bool weaponShooting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        GameObject weaponSpawnPoint = GameObject.FindGameObjectWithTag("WeaponSpawn");
        Instantiate(weapon, weaponSpawnPoint.transform);
    }

    void Update()
    {
        // Camera Rotation & applying it to the player model
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = right.y = 0;

        forward = forward.normalized;
        right = right.normalized;

        Vector3 forwardRelativeVerticalInput = moveInput.y * forward;
        Vector3 rigthRelativeHorizontalInput = moveInput.x * right;

        Vector3 moveDir = forwardRelativeVerticalInput + rigthRelativeHorizontalInput;

        if (moveDir != Vector3.zero)
        {
            Quaternion rotDes = Quaternion.identity;

            if (!weaponShooting)
            {
                rotDes = Quaternion.LookRotation(moveDir, Vector3.up);
            }
            else
            {
                rotDes = Quaternion.LookRotation(forward, Vector3.up);
            }

            playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, rotDes, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveDir * Time.deltaTime * moveSpeed);

        controller.Move(new Vector3(0, -fallingSpeed * Time.deltaTime, 0));
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnFire(InputValue value)
    {
        if(weapon == null)
        {
            Debug.Log("There is no weapon to shoot");
            return;
        }

        weaponShooting = value.isPressed;
    }
}
