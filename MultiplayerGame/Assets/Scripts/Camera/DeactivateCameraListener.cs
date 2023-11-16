using UnityEngine;

public class DeactivateCameraListener : MonoBehaviour
{
    AudioListener audioListener;

    void Start()
    {
        audioListener = GetComponent<AudioListener>();
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("MainCamera"))
        {
            audioListener.enabled = false;
        }
    }
}
