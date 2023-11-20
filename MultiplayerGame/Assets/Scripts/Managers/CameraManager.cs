using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameState gameState;

    [SerializeField] Camera titleCamera;
    [SerializeField] Camera mapCamera;

    bool cameraMissing = false;

    void Start()
    {
        try
        {
            titleCamera = GameObject.FindGameObjectWithTag("TitleCam").GetComponent<Camera>();
            mapCamera = GameObject.FindGameObjectWithTag("MapCam").GetComponent<Camera>();
        }
        catch
        {
            Debug.Log("Some Camera is missing");
            cameraMissing = true;
        }
    }

    void Update()
    {
        if (!cameraMissing)
        {
            switch (gameState)
            {
                case GameState.Title:
                    titleCamera.gameObject.SetActive(true);
                    mapCamera.gameObject.SetActive(false);

                    // Don't show UIs on UIs
                    GetComponent<UI_Manager>().showUI = false;
                    break;
                case GameState.Gameplay:
                    titleCamera.gameObject.SetActive(false);
                    mapCamera.gameObject.SetActive(true);

                    break;
            }
        }
    }

    private void LateUpdate()
    {
        if (SceneManagerScript.Instance.playersOnScene.Count == 0)
        {
            gameState = GameState.Title;
        }
        else if (Camera.main != null)
        {
            gameState = GameState.Gameplay;
        }
    }

    public enum GameState
    {
        Title,
        Gameplay
    }
}
