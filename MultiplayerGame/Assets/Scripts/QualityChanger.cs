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
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            QualitySettings.SetQualityLevel(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            QualitySettings.SetQualityLevel(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            QualitySettings.SetQualityLevel(2);
        }
    }
}
