using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOrbitCamera : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook cmCamera;
    [SerializeField] Camera playerCamera;
    Vector2 camBaseAxis;

    [Header("Sensivity")]
    public Vector2 mouseSens;
    public Vector2 gamepadSens;

    [SerializeField] bool cameraReseting = false;

    [SerializeField] Transform bodyTransform;

    [SerializeField] AudioListener playerAudioListener;

    private void Start()
    {
        camBaseAxis.Set(bodyTransform.rotation.eulerAngles.y, cmCamera.m_YAxis.Value);

        if (GetComponent<PlayerNetworking>().isOwnByThisInstance) { cmCamera.Priority = 20; }
    }

    private void Update()
    {
        // Sensivity
        if (!GetComponent<PlayerMovement>().isUsingGamepad)
        {
            cmCamera.m_XAxis.m_MaxSpeed = mouseSens.x;
            cmCamera.m_YAxis.m_MaxSpeed = mouseSens.y;
        }
        else
        {
            cmCamera.m_XAxis.m_MaxSpeed = gamepadSens.x;
            cmCamera.m_YAxis.m_MaxSpeed = gamepadSens.y;
        }

        camBaseAxis.x = bodyTransform.rotation.eulerAngles.y;

        // Audio Stereo
        playerAudioListener.transform.rotation = playerCamera.transform.rotation;
    }

    public Transform GetCameraTransform()
    {
        return playerCamera.transform;
    }

    #region Player Input Actions

    void OnCamReset(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
        {
            if (value.isPressed != cameraReseting && value.isPressed == true)
            {
                cmCamera.m_XAxis.Value = camBaseAxis.x;
                cmCamera.m_YAxis.Value = camBaseAxis.y;
            }

            cameraReseting = value.isPressed;
        }
    }

    // Network Data
    public void SetCamRot(Vector3 _camRot)
    {
        //cmCamera.m_XAxis.Value = _camRot.x;
        //cmCamera.m_YAxis.Value = _camRot.y;
        playerCamera.transform.forward = _camRot;
    }

    public Vector3 GetCamRot()
    {
        return playerCamera.transform.forward;
    }

    #endregion
}