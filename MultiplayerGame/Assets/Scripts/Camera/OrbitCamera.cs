using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] Transform cameraOrbitObject;

    [Header("Camera Propierties")]
    public float mouseSens = 100f;

    [SerializeField]
    private float cameraMaxDistance = 20.0f;

    private float cameraMinDist = 5.0f;
    private float cameraDistance;

    [Header("Camera Vertical Limits")]
    [SerializeField] float maxHeight = 30.0f;
    [SerializeField] float minHeight = -60.0f;

    /// Rotation Angles
    private float mAngleH = 0.0f;
    private float mAngleV = -30.0f;

    /// Camera Default Direction
    private Vector3 CamBaseAxis = Vector3.back;

    /// Distance for collisions
    private float CollisionReturnDis = 0.5f;

    bool cameraReseting = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cameraDistance = cameraMaxDistance;
    }

    void Update()
    {
        if(cameraOrbitObject == null)
        {
            Debug.LogError("Camera Target not defined");
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime;

        mAngleH += (Mathf.Clamp(mouseX, -1.0f, 1.0f) * mouseSens);
        mAngleV += (Mathf.Clamp(mouseY, -1.0f, 1.0f) * mouseSens);

        mAngleV = Mathf.Clamp(mAngleV, minHeight, maxHeight);

        cameraDistance = Mathf.Clamp(cameraDistance, cameraMinDist, cameraMaxDistance);

        Quaternion animRotation = Quaternion.Euler(-mAngleV, mAngleH, 0.0f);
        Quaternion camYRotation = Quaternion.Euler(0.0f, mAngleH, 0.0f);
        transform.rotation = animRotation;

        Vector3 lookatpos = cameraOrbitObject.position + camYRotation * Vector3.one;
        Vector3 camdir = animRotation * CamBaseAxis;
        camdir.Normalize();
        transform.position = lookatpos + camdir * cameraDistance;

        // Calculate camera points after collision
        RaycastHit rayhit;
        bool hit = Physics.Raycast(lookatpos, camdir, out rayhit, cameraDistance);
        if (hit)
        {
            // Block character collision
            bool charControl = rayhit.collider as CharacterController;
            if (!charControl)
            {
                Vector3 modifypos = rayhit.normal * CollisionReturnDis * 2.0f;
                transform.position = rayhit.point + modifypos;

                float distance = Vector3.Distance(transform.position, lookatpos);
                distance = Mathf.Clamp(distance, cameraMinDist, cameraMaxDistance);
                transform.position = lookatpos + camdir * distance + modifypos;
            }
        }
    }
    // Camera Reset View (WIP)

    // La camara al resetearse tiene que alinearse con adonde mira el player en Y y ponerse en X = 0
    public void ResetCamera(bool reset)
    {
        cameraReseting = reset;
    }
}