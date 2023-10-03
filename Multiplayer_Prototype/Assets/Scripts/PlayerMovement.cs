using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    void Start()
    {
        controller = GetComponentInChildren<CharacterController>();
    }

    void Update()
    {
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
    }
}
