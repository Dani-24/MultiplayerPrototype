using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    #region Resources

    [Header("Gameobjects needed")]
    [SerializeField] Transform cameraOrbitPoint;
    [SerializeField] Transform playerBodyRotation;

    public Camera affectedCamera;

    #endregion

    #region Propierties

    Vector2 camAxis;
    Vector3 CamBaseAxis = Vector3.back;

    [Header("Sensivity")]
    public Vector2 mouseSens;
    public Vector2 gamepadSens;

    [Header("Camera Propierties")]
    [SerializeField] float camSpeed = 1f;

    float cameraDistance;
    [SerializeField] float cameraMaxDist = 20.0f;
    [SerializeField] float cameraMinDist = 5.0f;

    float camRotDefaultY = -5f;
    [SerializeField] Vector2 camRot;

    [Header("Camera Vertical Limits")]
    [SerializeField] float maxHeight = 30.0f;
    [SerializeField] float minHeight = -60.0f;

    [Header("Collisions")]
    [SerializeField] float CollisionDistance = 0.5f;

    #endregion

    #region Reticle Configuration

    [Header("Reticle")]
    [SerializeField] GameObject reticleUI;

    [SerializeField] float reticleMinY = 23.0f;
    [SerializeField] float reticleMaxY = 110.0f;
    [SerializeField] float reticleScale = 1f;

    #endregion

    #region Debug

    [Header("Debug Info")]
    public bool cameraReseting = false;
    bool returningFromHit = false;

    #endregion

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cameraDistance = cameraMaxDist;
        camRotDefaultY = camRot.y;
    }

    void LateUpdate()
    {
        if (cameraOrbitPoint == null)
        {
            Debug.LogError("Camera Target not defined");
            return;
        }

        #region Camera Input

        float mouseX = camAxis.x * Time.deltaTime;
        float mouseY = camAxis.y * Time.deltaTime;

        // Aplicar sensibilidad a los ejes
        if (!GetComponent<PlayerMovement>().isUsingGamepad)
        {
            camRot.x += (Mathf.Clamp(mouseX, -1.0f, 1.0f) * mouseSens.x);
            camRot.y += (Mathf.Clamp(mouseY, -1.0f, 1.0f) * mouseSens.y);
        }
        else
        {
            camRot.x += (Mathf.Clamp(mouseX, -1.0f, 1.0f) * gamepadSens.x);
            camRot.y += (Mathf.Clamp(mouseY, -1.0f, 1.0f) * gamepadSens.y);
        }

        // Limitar altura max y min del eje Y
        camRot.y = Mathf.Clamp(camRot.y, minHeight, maxHeight);

        cameraDistance = Mathf.Clamp(cameraDistance, cameraMinDist, cameraMaxDist);

        #endregion

        // Set Camera Rotation
        Quaternion animRotation = Quaternion.Euler(-camRot.y, camRot.x, 0.0f);
        Quaternion camYRotation = Quaternion.Euler(0.0f, camRot.x, 0.0f);
        affectedCamera.transform.rotation = animRotation;

        // Set Camera Position
        Vector3 lookatpos = new Vector3(cameraOrbitPoint.position.x, cameraOrbitPoint.position.y, cameraOrbitPoint.position.z);
        Vector3 camdir = animRotation * CamBaseAxis;
        camdir.Normalize();

        #region Camera Collisions with Terrain

        // Calculate camera points after collision
        RaycastHit rayhit;
        bool hit = Physics.Raycast(lookatpos, camdir, out rayhit, cameraDistance);

        affectedCamera.transform.position = lookatpos + camdir * cameraDistance;

        //if (hit)
        //{
        //    returningFromHit = true;

        //    // Block character collision
        //    bool charControl = rayhit.collider as CharacterController;
        //    if (!charControl)
        //    {
        //        Vector3 modifypos = rayhit.normal * CollisionReturnDis * 2.0f;

        //        transform.position = Vector3.LerpUnclamped(transform.position, rayhit.point + modifypos, camSpeed * Time.deltaTime);

        //        //transform.position = rayhit.point + modifypos;

        //        float distance = Vector3.Distance(transform.position, lookatpos);
        //        distance = Mathf.Clamp(distance, cameraMinDist, cameraMaxDist);

        //        transform.position = Vector3.LerpUnclamped(transform.position, lookatpos + camdir * distance + modifypos, camSpeed * Time.deltaTime);

        //        //transform.position = lookatpos + camdir * distance + modifypos;
        //    }
        //}
        //else
        //{
        //    if (returningFromHit)
        //    {
        //        transform.position = Vector3.LerpUnclamped(transform.position, lookatpos + camdir * cameraDistance, camSpeed * Time.deltaTime);

        //        if (Vector3.Distance(transform.position, lookatpos + camdir * cameraDistance) < 0.25f)
        //        {
        //            returningFromHit = false;
        //        }
        //    }
        //    else
        //    {
        //        transform.position = lookatpos + camdir * cameraDistance;
        //    }
        //}

        #endregion
    }

    #region Player Input Actions

    void OnCamReset(InputValue value)
    {
        if (value.isPressed != cameraReseting && value.isPressed == true)
        {
            camRot.x = playerBodyRotation.transform.rotation.eulerAngles.y;
            camRot.y = camRotDefaultY;
        }

        cameraReseting = value.isPressed;
    }

    void OnCamera(InputValue value)
    {
        camAxis = value.Get<Vector2>();
    }

    #endregion
}