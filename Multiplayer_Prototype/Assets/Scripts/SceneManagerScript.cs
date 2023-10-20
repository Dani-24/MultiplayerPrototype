using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void ChangeScene(string sceneToChange)
    {
        SceneManager.LoadScene(sceneToChange);
    }
}
