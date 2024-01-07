using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    List<CinemachineFreeLook> sceneCameras = new List<CinemachineFreeLook>();

    [Tooltip("Current displaying camera")]
    [SerializeField] CinemachineFreeLook currentCam;

    [Header("Scene Cameras")]

    [SerializeField] CinemachineBrain brain;

    [Tooltip("Camera to display at start")]
    [SerializeField] CinemachineFreeLook startCam;

    [Tooltip("Title Screen Camera")]
    public CinemachineFreeLook titleCamera;

    [Tooltip("3rd Person Player Camera")]
    public CinemachineFreeLook playerCamera;

    #region Instance

    private static CameraManager _instance;
    public static CameraManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    void Start()
    {
        if (titleCamera != null) sceneCameras.Add(titleCamera);
        if (playerCamera != null) sceneCameras.Add(playerCamera);

        if (UI_Manager.Instance.alreadyShownTitle) currentCam = playerCamera; else currentCam = startCam;

        for (int i = 0; i < sceneCameras.Count; i++)
        {
            if (sceneCameras[i] == currentCam)
            {
                sceneCameras[i].Priority = 20;
            }
            else
            {
                sceneCameras[i].Priority = 10;
            }
        }
    }

    private void Update()
    {
        if (currentCam != null)
        {
            if (currentCam == playerCamera && !brain.IsBlending && SceneManagerScript.Instance.gameState == SceneManagerScript.GameState.Gameplay)
            {
                SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().playerInputEnabled = true;
            }
            else
            {
                SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().playerInputEnabled = false;
            }
        }
    }

    public void SwitchCamera(CinemachineFreeLook camera)
    {
        currentCam = camera;
        currentCam.Priority = 20;

        for (int i = 0; i < sceneCameras.Count; i++)
        {
            if (sceneCameras[i] != currentCam)
            {
                sceneCameras[i].Priority = 10;
            }
        }
    }
}
