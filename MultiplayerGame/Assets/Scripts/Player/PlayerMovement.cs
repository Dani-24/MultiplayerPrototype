using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;
    PlayerInput input;

    public GameObject playerBody;

    #region Horizontal Movement Propierties

    [Header("Movement")]
    [SerializeField] private Vector2 moveInput;

    Vector3 forward;
    Vector3 moveDir;

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float runSpeed = 20.0f;
    //[SerializeField] private float slowSpeed = 5.0f;
    public float weaponSpeedMultiplier = 1.0f;

    public bool isRunning = false;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float rotationSpeedWhileShooting = 100.0f;

    #endregion

    #region Vertical Movement Propierties

    [Header("Gravity")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float gravityMultiplier = 1.0f;
    [SerializeField] float fallSpeed;
    [SerializeField] float maxFallSpeed;
    [SerializeField] float groundedFallSpeed = -3.0f;

    public bool isGrounded;
    float originalStepOffset;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isJumping = false;

    [SerializeField] float groundCheckDist = 0.1f;
    public List<LayerMask> groundLayers = new List<LayerMask>();

    #endregion

    public bool isUsingGamepad;

    [SerializeField] Texture maskT;
    [SerializeField] Texture2D texture;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        originalStepOffset = controller.stepOffset;

        texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);

    }

    void Update()
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
        {
            // UI Input
            if (input.actions["ShowConsole"].WasReleasedThisFrame())
            {
                SceneManagerScript.Instance.showConsole = !SceneManagerScript.Instance.showConsole;
            }

            if (input.actions["OpenUI"].WasReleasedThisFrame())
            {
                UI_Manager.Instance.openSettings = !UI_Manager.Instance.openSettings;
            }

            // Check if using Gamepad or not
            if (input.currentControlScheme == "Gamepad")
            {
                isUsingGamepad = true;
            }
            else
            {
                isUsingGamepad = false;
            }

            GetMovementDirection();
            Rotation();
            Movement();
            JumpingAndFalling();

            // Para que esté pegado al suelo
            controller.Move(new Vector3(0, fallSpeed * Time.deltaTime, 0));
        }

        //CheckGroundPaint();
    }

    #region Player Movement

    void GetMovementDirection()
    {
        forward = GetComponent<PlayerOrbitCamera>().GetCameraTransform().forward;
        Vector3 right = GetComponent<PlayerOrbitCamera>().GetCameraTransform().right;

        forward.y = right.y = 0;

        forward = forward.normalized;
        right = right.normalized;

        Vector3 forwardRelativeVerticalInput = moveInput.y * forward;
        Vector3 rigthRelativeHorizontalInput = moveInput.x * right;

        moveDir = forwardRelativeVerticalInput + rigthRelativeHorizontalInput;
        moveDir.Normalize();
    }

    void Movement()
    {
        if (moveInput.x != 0 || moveInput.y != 0)
        {
            // Made so controller smoothly moves to its rotation while not shooting anything
            if (GetComponent<PlayerArmament>().weaponShooting || GetComponent<PlayerArmament>().subWeaponShooting)
            {
                controller.Move(moveDir * Time.deltaTime * moveSpeed * weaponSpeedMultiplier);
            }
            else if (!isRunning)
            {
                controller.Move(moveDir * Time.deltaTime * moveSpeed);
            }
            else
            {
                controller.Move(moveDir * Time.deltaTime * runSpeed);
            }
        }
    }

    void Rotation()
    {
        if (moveDir != Vector3.zero || GetComponent<PlayerArmament>().weaponShooting || GetComponent<PlayerArmament>().subWeaponShooting)
        {
            Quaternion rotDes = Quaternion.identity;

            if (!GetComponent<PlayerArmament>().weaponShooting && !GetComponent<PlayerArmament>().subWeaponShooting)
            {
                rotDes = Quaternion.LookRotation(moveDir, Vector3.up);
                playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, rotDes, rotationSpeed * Time.deltaTime);
            }
            else
            {
                rotDes = Quaternion.LookRotation(forward, Vector3.up);

                playerBody.transform.rotation = Quaternion.Slerp(playerBody.transform.rotation, rotDes, rotationSpeedWhileShooting * Time.deltaTime);
            }
        }
    }

    void JumpingAndFalling()
    {
        if (fallSpeed > maxFallSpeed)
        {
            fallSpeed += gravity * gravityMultiplier * Time.deltaTime;
        }
        else
        {
            fallSpeed = maxFallSpeed;
        }

        foreach (var layer in groundLayers)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDist, layer);

            if (isGrounded) break;
        }

        if (isGrounded)
        {
            controller.stepOffset = originalStepOffset;
            fallSpeed = groundedFallSpeed;

            if (isJumping)
            {
                fallSpeed = jumpForce;
            }
        }
        else
        {
            controller.stepOffset = 0;
        }
    }

    #endregion

    #region Ground Paint

    void CheckGroundPaint()
    {
        // Lanzar un raycast hacía abajo y mirar si es suelo pintable
        RaycastHit hit;

        for (int i = 0; i < groundLayers.Count; i++)
        {
            if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity, groundLayers[i]))
            {
                try
                {
                    Paintable hitPaintable = hit.collider.GetComponent<Paintable>();

                    //Debug.Log(texture.GetPixel((int)pos.x, (int)pos.y));

                    /*Color colorGround = */

                    maskT = hitPaintable.getRenderer().material.GetTexture("_MaskTexture"); //PaintManager.instance.GetPaintColor(hitPaintable, hit.point);

                    Graphics.CopyTexture(maskT, texture);

                    float xNormalized = hit.point.x / texture.width;
                    float yNormalized = hit.point.y / texture.height;

                    int x = Mathf.FloorToInt(xNormalized);
                    int y = Mathf.FloorToInt(yNormalized);

                    Debug.Log(texture.GetPixel(x, y) + " at: " + x + " " + y);

                    //if (hitPaintable != null)
                    //{
                    //    if (colorGround == SceneManagerScript.Instance.GetTeamColor("Alpha"))
                    //    {
                    //        Debug.Log("Alpha team Ink:" + colorGround);
                    //    }
                    //    else if (colorGround == SceneManagerScript.Instance.GetTeamColor("Beta"))
                    //    {
                    //        Debug.Log("Beta team Ink:" + colorGround);
                    //    }
                    //    else
                    //    {
                    //        Debug.Log("No Ink?");
                    //    }
                    //}
                }
                catch
                {
                    // No Paintable Component
                }
            }
        }
    }

    #endregion

    #region Player Input Actions

    void OnMove(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
            moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
            isJumping = value.isPressed;
    }

    void OnRun(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
            isRunning = value.isPressed;
    }

    // For Network
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public bool GetRunInput()
    {
        return isRunning;
    }

    public bool GetJumpInput()
    {
        return isJumping;
    }

    public void SetMoveInput(Vector2 _input)
    {
        moveInput = _input;
    }

    public void SetRunInput(bool _run)
    {
        isRunning = _run;
    }

    public void SetJumpInput(bool _jump)
    {
        isJumping = _jump;
    }

    public void SetPosition(Vector3 _position)
    {
        if (controller != null)
        {
            controller.enabled = false;
            controller.transform.position = Vector3.LerpUnclamped(controller.transform.position, _position, 20 * Time.deltaTime);
            controller.enabled = true;
        }
    }

    public void SetRotation(Quaternion _rot)
    {
        playerBody.transform.rotation = Quaternion.LerpUnclamped(playerBody.transform.rotation, _rot, 20 * Time.deltaTime);
    }

    #endregion
}
