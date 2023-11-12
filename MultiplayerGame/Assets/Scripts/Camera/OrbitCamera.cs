using Unity.VisualScripting;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] Transform cameraOrbitObject;
    [SerializeField] Transform playerRotation;

    [Header("Camera Propierties")]
    public float mouseSens = 100f;

    [SerializeField] float camSpeed = 1f;

    [SerializeField]
    private float cameraMaxDist = 20.0f;
    [SerializeField]
    private float cameraMinDist = 5.0f;

    private float cameraDistance;

    [Tooltip("Horizontal Rotation Angle")]
    [SerializeField] private float camRotX = 0.0f;
    [Tooltip("Vertical Rotation Angle")]
    [SerializeField] private float camRotY = 0.0f;

    float camRotDefaultY;

    /// Camera Default Direction
    private Vector3 CamBaseAxis = Vector3.back;

    [Tooltip("Distance for collisions")]
    [SerializeField] private float CollisionReturnDis = 0.5f;

    [Header("Camera Vertical Limits")]
    [SerializeField] float maxHeight = 30.0f;
    [SerializeField] float minHeight = -60.0f;

    [Header("Debug Info")]
    public bool cameraReseting = false;

    [Header("Reticle")]
    [SerializeField] GameObject reticleUI;

    [SerializeField] float reticleMinY = 23.0f;
    [SerializeField] float reticleMaxY = 110.0f;

    [SerializeField] float reticleScale = 1f;

    bool returningFromHit = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cameraDistance = cameraMaxDist;
        camRotDefaultY = camRotY;
    }

    void LateUpdate()
    {
        if(cameraOrbitObject == null)
        {
            Debug.LogError("Camera Target not defined");
            return;
        }

        // Obtener Input
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime;

        // Aplicar sensibilidad a los ejes
        camRotX += (Mathf.Clamp(mouseX, -1.0f, 1.0f) * mouseSens);
        camRotY += (Mathf.Clamp(mouseY, -1.0f, 1.0f) * mouseSens);

        // Limitar altura max y min del eje Y
        camRotY = Mathf.Clamp(camRotY, minHeight, maxHeight);

        cameraDistance = Mathf.Clamp(cameraDistance, cameraMinDist, cameraMaxDist);

        // Set Camera Rotation
        Quaternion animRotation = Quaternion.Euler(-camRotY, camRotX, 0.0f);
        Quaternion camYRotation = Quaternion.Euler(0.0f, camRotX, 0.0f);
        transform.rotation = animRotation;

        // Set Camera Position
        Vector3 lookatpos = new Vector3(cameraOrbitObject.position.x /*+ (camYRotation * Vector3.one).x*/, cameraOrbitObject.position.y /*+ (camYRotation * Vector3.one).y*/,cameraOrbitObject.position.z /*+ (camYRotation * Vector3.one).z*/);
        Vector3 camdir = animRotation * CamBaseAxis;
        camdir.Normalize();

        // Calculate camera points after collision
        RaycastHit rayhit;
        bool hit = Physics.Raycast(lookatpos, camdir, out rayhit, cameraDistance);

        //transform.position = new Vector3(Mathf.Lerp(transform.position.x, lookatpos.x + camdir.x * cameraDistance, camSpeed * Time.deltaTime), Mathf.Lerp(transform.position.x, lookatpos.x + camdir.x * cameraDistance, camSpeed * Time.deltaTime), Mathf.Lerp(transform.position.x, lookatpos.x + camdir.x * cameraDistance, camSpeed * Time.deltaTime));

        transform.position = lookatpos + camdir * cameraDistance;

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
    }

    [Tooltip("Player Input for camera reset")]
    public void ResetCamera(bool reset)
    {
        if (reset != cameraReseting && reset == true)
        {
            camRotX = playerRotation.transform.rotation.eulerAngles.y;
            camRotY = camRotDefaultY;
        }

        cameraReseting = reset;
    }
}