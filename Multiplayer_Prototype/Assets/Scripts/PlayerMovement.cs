using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [SerializeField]
    private Vector2 movementInput;

    [SerializeField]
    private float playerSpeed = 1.0f;

    [SerializeField]
    private bool jumped = false;

    private float speedY;

    [SerializeField]
    private float jumpHeight = 1.0f;

    void Start()
    {
        controller = GetComponentInChildren<CharacterController>();
    }

    void Update()
    {
        // =============== Movement

        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        controller.Move(move * Time.deltaTime * playerSpeed);
    

        // =============== Jump

        if(jumped)
        {
            speedY += Mathf.Sqrt(jumpHeight * -3.0f * -9.81f);
        }

        speedY += -9.81f * Time.deltaTime;
        controller.SimpleMove(new Vector3(0,speedY * Time.deltaTime,0));
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
    }
}
