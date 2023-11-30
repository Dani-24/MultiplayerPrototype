using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<CinemachineVirtualCamera> sceneCameras = new List<CinemachineVirtualCamera>();

    public static CinemachineVirtualCamera activeCamera;

    void Start()
    {
        
    }

    void Update()
    {

    }
}
