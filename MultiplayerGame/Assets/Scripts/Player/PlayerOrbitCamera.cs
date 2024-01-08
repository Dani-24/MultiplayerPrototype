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

    [SerializeField] CinemachineInputProvider inputProviderCM;

    private void Start()
    {
        // Default Rotation Values
        cmCamera.m_XAxis.Value = bodyTransform.rotation.eulerAngles.y;
        cmCamera.m_YAxis.Value = 0.5f;
        camBaseAxis.Set(cmCamera.m_XAxis.Value, cmCamera.m_YAxis.Value);

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

        // Inputs Enabled/Disabled
        if(GetComponent<PlayerNetworking>().isOwnByThisInstance && GetComponent<PlayerStats>().playerInputEnabled)
        {
            inputProviderCM.enabled = true;
        }
        else
        {
            inputProviderCM.enabled = false;
        }
    }

    public Transform GetCameraTransform()
    {
        return playerCamera.transform;
    }

    public void ChangeSens(bool mouse, bool xAxis, float value)
    {
        if (mouse)
        {
            if (xAxis)
            {
                mouseSens.x = value;
            }
            else
            {
                mouseSens.y = value;
            }
        }
        else
        {
            if (xAxis)
            {
                gamepadSens.x = value;
            }
            else
            {
                gamepadSens.y = value;
            }
        }
    }

    #region Player Input Actions

    void OnCamReset(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance && GetComponent<PlayerStats>().playerInputEnabled)
        {
            if (value.isPressed != cameraReseting && value.isPressed == true)
            {
                CameraResetView();
            }

            cameraReseting = value.isPressed;
        }
    }

    public void CameraResetView()
    {
        cmCamera.m_XAxis.Value = camBaseAxis.x;
        cmCamera.m_YAxis.Value = camBaseAxis.y;
    }

    public void CameraSetView(float value)
    {
        cmCamera.m_XAxis.Value = value;
    }

    // Network Data
    public void SetCamRot(Vector3 _camRot)
    {
        playerCamera.transform.forward = _camRot;
    }

    public Vector3 GetCamRot()
    {
        return playerCamera.transform.forward;
    }

    #endregion
}