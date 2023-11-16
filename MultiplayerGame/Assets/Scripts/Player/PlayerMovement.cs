using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;
    PlayerInput input;

    [SerializeField] private GameObject playerBody;

    #region Horizontal Movement Propierties

    [Header("Movement")]
    [SerializeField] private Vector2 moveInput;

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float runSpeed = 20.0f;
    [SerializeField] private float slowSpeed = 5.0f;
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
    [SerializeField] float groundedFallSpeed = -3.0f;

    [SerializeField] bool isGrounded;
    float originalStepOffset;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isJumping = false;

    [SerializeField] float groundCheckDist = 0.1f;
    public LayerMask groundLayer;

    #endregion

    public bool isUsingGamepad;

    [Header("Ground Color Check")]
    [SerializeField] Texture debugTexture;
    [SerializeField] Texture2D debugTexture2d;
    [SerializeField] GameObject debugGameObjectHit;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        originalStepOffset = controller.stepOffset;
    }

    void Update()
    {
        // Input
        if (input.actions["ShowConsole"].WasReleasedThisFrame())
        {
            SceneManagerScript.Instance.showConsole = !SceneManagerScript.Instance.showConsole;
        }

        if (input.currentControlScheme == "Gamepad" )
        {
            isUsingGamepad = true;
        }
        else
        {
            isUsingGamepad = false;
        }

        // Camera Rotation & applying it to the player model
        Vector3 forward = GetComponent<OrbitCamera>().affectedCamera.transform.forward;
        Vector3 right = GetComponent<OrbitCamera>().affectedCamera.transform.right;

        forward.y = right.y = 0;

        forward = forward.normalized;
        right = right.normalized;

        Vector3 forwardRelativeVerticalInput = moveInput.y * forward;
        Vector3 rigthRelativeHorizontalInput = moveInput.x * right;

        Vector3 moveDir = forwardRelativeVerticalInput + rigthRelativeHorizontalInput;

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

        if (moveInput.x != 0 || moveInput.y != 0)
        {
            // Made so controller smoothly moves to its rotation while not shooting anything
            if(GetComponent<PlayerArmament>().weaponShooting || GetComponent<PlayerArmament>().subWeaponShooting)
            {
                controller.Move(moveDir * Time.deltaTime * moveSpeed);
            }
            else if (!isRunning)
            {
                controller.Move(playerBody.transform.forward /*moveDir*/ * Time.deltaTime * moveSpeed);
            }
            else
            {
                controller.Move(playerBody.transform.forward /*moveDir*/ * Time.deltaTime * runSpeed);
            }
        }

        #region Ground/Jump Checking

        fallSpeed += gravity * gravityMultiplier * Time.deltaTime;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDist, groundLayer);

        if(isGrounded)
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

        #endregion

        controller.Move(new Vector3(0, fallSpeed * Time.deltaTime, 0));

        //CheckGroundPaint();
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    #region Ground Paint

    void CheckGroundPaint()
    {
        // Lanzar un raycast hacía abajo y mirar si es suelo pintable

        RaycastHit hit;

        if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity, groundLayer))
        {
            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();

            if (hitRenderer != null)
            {
                Vector2 uvCoord = hit.textureCoord;
                Material material = hitRenderer.material;

                //Texture2D texture = material.mainTexture as Texture2D;

                Texture texture = material.GetTexture("_MaskTexture");

                debugGameObjectHit = hit.collider.gameObject;
                debugTexture = texture;

                // Crea un nuevo objeto Texture2D
                Texture2D texture2D = null; //TextureToText2D(texture);

                debugTexture2d = texture2D;

                if (texture2D != null)
                {
                    Color pixelColor = texture2D.GetPixelBilinear(uvCoord.x, uvCoord.y);

                    Debug.Log("Color: " + pixelColor);

                    // Comprobar el color del suelo y mirar si es aliado o enemigo

                    // Si es color aliado con shift puedes correr + rapido y recargas rapido

                    // Si es color enemigo te mueves mas lento, recibes un pelin de daño y usar shift te frena mas

                    if (pixelColor == SceneManagerScript.Instance.GetTeamColor(GetComponent<PlayerStats>().teamTag))
                    {
                        Debug.Log("Ally Ink");
                    }
                    else
                    {
                        Debug.Log("Enemy Ink");
                    }
                }
                else
                {
                    Debug.Log("Texture null");
                }
            }
        }
    }

    Texture2D TextureToText2D(Texture source)
    {
        // !!! Dentro de lo que cabe funciona pero Unity muere en el proceso

        RenderTexture renderTex = RenderTexture.GetTemporary(
               source.width,
               source.height,
               0,
               RenderTextureFormat.Default,
        RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    #endregion

    #region Player Input Actions

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        isJumping = value.isPressed;
    }

    void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }

    #endregion
}
