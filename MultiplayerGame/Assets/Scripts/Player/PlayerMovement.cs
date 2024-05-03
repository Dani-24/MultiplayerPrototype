using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;
    PlayerInput input;
    public GameObject playerBody;

    public bool isUsingGamepad;

    [Header("Ground Paint")]
    [SerializeField] Color groundColor;
    public GroundInk groundInk;
    RenderTexture maskT;
    [SerializeField] Texture2D texture;

    #region Horizontal Movement Propierties

    [Header("Movement")]
    [SerializeField] private Vector2 moveInput;

    Vector3 forward;
    Vector3 moveDir;
    Vector3 lastDir;

    [SerializeField][Range(0.1f, 25f)] private float moveSpeed = 10.0f;
    [SerializeField][Range(0.1f, 25f)] private float runSpeed = 20.0f;
    [SerializeField][Range(0.1f, 25f)] private float slowSpeed = 5.0f;
    [Range(1f, 10f)] public float weaponSpeedMultiplier = 1.0f;

    [SerializeField][Range(0.1f, 20f)] float acceleration = 1.0f;
    [SerializeField][Range(0.1f, 10f)] float accelerationMultiplier = 1.0f;

    [Header("Final Speed")]
    [SerializeField] float actualSpeed;
    [SerializeField] float targetSpeed;

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

    [SerializeField][Range(0f, 2f)] float groundCheckDist = 0.1f;
    public List<LayerMask> groundLayers = new List<LayerMask>();

    #endregion

    [Header("Network")]
    [SerializeField] int interpolationSpeed = 20;

    #region App

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        originalStepOffset = controller.stepOffset;

        texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
    }

    void Update()
    {
        UIInputs();
        CheckGroundPaint();
    }

    private void FixedUpdate()
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
        {
            if (GetComponent<PlayerStats>().playerInputEnabled)
            {
                GetMovementDirection();
                BodyRotation();
                Movement();
                JumpingAndFalling();
            }
            else
            {
                Falling();

                moveDir.Set(0, fallSpeed, 0);
            }
        }
        controller.Move(moveDir * Time.deltaTime);
    }

    #endregion

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
        targetSpeed = 0;

        if (moveInput != Vector2.zero)
        {
            lastDir = moveDir;
            switch (groundInk)
            {
                case GroundInk.NoInk:

                    if (GetComponent<PlayerArmament>().weaponShooting || GetComponent<PlayerArmament>().subWeaponShooting)
                        targetSpeed += moveSpeed * weaponSpeedMultiplier;
                    else
                        targetSpeed += moveSpeed;

                    break;
                case GroundInk.AllyInk:

                    if (GetComponent<PlayerArmament>().weaponShooting || GetComponent<PlayerArmament>().subWeaponShooting)
                        targetSpeed += moveSpeed * weaponSpeedMultiplier;
                    else if (isRunning)
                        targetSpeed += runSpeed;
                    else
                        targetSpeed += moveSpeed;

                    break;
                case GroundInk.EnemyInk:

                    targetSpeed += slowSpeed;

                    break;
            }
        }
        else
        {
            moveDir = lastDir;
        }

        if (actualSpeed != targetSpeed/* && isGrounded*/)
        {
            if (actualSpeed < targetSpeed)
                actualSpeed += acceleration * accelerationMultiplier;
            else
                actualSpeed -= acceleration * accelerationMultiplier;
        }

        moveDir *= actualSpeed;
    }

    void BodyRotation()
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

    void Falling()
    {
        if (fallSpeed > maxFallSpeed)
            fallSpeed += gravity * gravityMultiplier * Time.deltaTime;
        else
            fallSpeed = maxFallSpeed;
    }

    void JumpingAndFalling()
    {
        Falling();

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

        moveDir.y += fallSpeed;
    }

    #endregion

    #region Ground Paint

    void CheckGroundPaint()
    {
        if (!GetComponent<PlayerNetworking>().isOwnByThisInstance || !isGrounded)
            return;

        RaycastHit hit;

        for (int i = 0; i < groundLayers.Count; i++)
        {
            if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity, groundLayers[i]))
            {
                try
                {
                    Paintable hitPaintable = hit.collider.GetComponent<Paintable>();

                    maskT = (RenderTexture)hitPaintable.getRenderer().material.GetTexture(hitPaintable.maskTextureID);

                    RenderTexture.active = maskT;
                    texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    texture.Apply();

                    Vector2 coord = hit.textureCoord;   // Solo funka con meshCollider

                    groundColor = texture.GetPixelBilinear(coord.x, coord.y);

                    // Comparar colores con alphaTeam y BetaTeam
                    string inkTeam = SceneManagerScript.Instance.GetTeamFromColor(groundColor);

                    if (GetComponent<PlayerStats>().teamTag == inkTeam)
                        groundInk = GroundInk.AllyInk;
                    else if (SceneManagerScript.Instance.GetRivalTag(GetComponent<PlayerStats>().teamTag) == inkTeam)
                        groundInk = GroundInk.EnemyInk;
                    else
                        groundInk = GroundInk.NoInk;
                }
                catch
                {
                    // No Paintable Component
                    Debug.Log("That Ground is not <color=red>Pain</color>table");
                }
            }
        }

        if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity, groundLayers[1]))
            groundInk = GroundInk.NoInk;
    }

    public enum GroundInk
    {
        AllyInk,
        EnemyInk,
        NoInk
    }

    #endregion

    #region Player Input Actions

    void UIInputs()
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
        {
            if (input.actions["ShowConsole"].WasReleasedThisFrame()) SceneManagerScript.Instance.showConsole = !SceneManagerScript.Instance.showConsole;

            if (input.actions["OpenUI"].WasReleasedThisFrame()) UI_Manager.Instance.ToggleNetSettings();

            // Check if using Gamepad or not
            if (input.currentControlScheme == "Gamepad") isUsingGamepad = true; else isUsingGamepad = false;
        }
    }

    // Movement
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

    public bool GetRunInput()
    {
        return isRunning;
    }

    public bool GetJumpInput()
    {
        return isJumping;
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
            controller.Move(Vector3.LerpUnclamped(controller.transform.position, _position, interpolationSpeed * Time.deltaTime) - controller.transform.position);
        }
    }

    public void SetRotation(Quaternion _rot)
    {
        playerBody.transform.rotation = Quaternion.LerpUnclamped(playerBody.transform.rotation, _rot, interpolationSpeed * Time.deltaTime);
    }

    public void SetFacing(float angle)
    {
        Vector3 rot = transform.eulerAngles;
        rot.y = angle;
        transform.eulerAngles = rot;

        GetComponent<PlayerOrbitCamera>().CameraSetView(angle);
    }

    public void TeleportToSpawnPos()
    {
        controller.enabled = false;
        transform.position = GetComponent<PlayerStats>().spawnPos;
        controller.enabled = true;
    }

    #endregion
}