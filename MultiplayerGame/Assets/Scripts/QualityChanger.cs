using UnityEngine;

public class QualityChanger : MonoBehaviour
{
    public string[] names;

    void Start()
    {
        names = QualitySettings.names;
    }

    void Update()
    {

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
                QualitySettings.SetQualityLevel(0);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                QualitySettings.SetQualityLevel(1);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                QualitySettings.SetQualityLevel(2);

            if (Input.GetKeyDown(KeyCode.F1))
                Application.targetFrameRate = 10;
            if (Input.GetKeyDown(KeyCode.F2))
                Application.targetFrameRate = 20;
            if (Input.GetKeyDown(KeyCode.F3))
                Application.targetFrameRate = 30;
            if (Input.GetKeyDown(KeyCode.F4))
                Application.targetFrameRate = 60;
            if (Input.GetKeyDown(KeyCode.F5))
                Application.targetFrameRate = 900;
        }
#endif

    }
}
