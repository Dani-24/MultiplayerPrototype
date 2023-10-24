using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [SerializeField] private GameObject playerBody;

    [Header("Movement")]
    [SerializeField] private Vector2 moveInput;

    [SerializeField] private float moveSpeed = 10.0f;

    [SerializeField] private float rotationSpeed = 10.0f;

    [HideInInspector] public Camera cam;

    [SerializeField] float maxFallingSpeed = 1.0f;

    float fallingSpeed = 0f;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 5f;

    [SerializeField] bool jumping = false;

    [SerializeField] float groundCheckDist = 0.1f;

    public LayerMask groundLayer;

    [Header("Weapon")]
    public GameObject weapon;
    public bool weaponShooting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        GameObject weaponSpawnPoint = GameObject.FindGameObjectWithTag("WeaponSpawn");
        Instantiate(weapon, weaponSpawnPoint.transform);

        fallingSpeed = -maxFallingSpeed;
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

        if (moveDir != Vector3.zero || weaponShooting)
        {
            Quaternion rotDes = Quaternion.identity;

            if (!weaponShooting)
            {
                rotDes = Quaternion.LookRotation(moveDir, Vector3.up);
                playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, rotDes, rotationSpeed * Time.deltaTime);
            }
            else
            {
                rotDes = Quaternion.LookRotation(forward, Vector3.up);

                playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, rotDes, rotationSpeed * 100 * Time.deltaTime);
            }
        }

        controller.Move(moveDir * Time.deltaTime * moveSpeed);

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDist, groundLayer);

        if (isGrounded && jumping)
        {
            fallingSpeed = jumpForce;
        }

        if(fallingSpeed > -maxFallingSpeed)
        {
            fallingSpeed -= Time.deltaTime * 10f;
        }

        controller.Move(new Vector3(0, fallingSpeed * Time.deltaTime, 0));
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        jumping = value.isPressed;
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
