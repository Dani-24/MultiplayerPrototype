using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] float maxViewDist = 25f;
    [SerializeField] Transform cameraOrbitObject;

    public float mouseSens = 100f;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, maxViewDist);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        cameraOrbitObject.Rotate(Vector3.up * mouseX);
    }
}
