using UnityEngine;
using UnityEngine.UI;

public class CanvasS_Gameplay : MonoBehaviour
{

    [SerializeField] Slider lifeSlider;
    [SerializeField] Slider inkSlider;
    [SerializeField] Image inkSliderImg;

    [Header("Reticle")]
    [SerializeField] GameObject reticleUI;

    //[SerializeField] float reticleMinY = 23.0f;
    //[SerializeField] float reticleMaxY = 110.0f;
    //[SerializeField] float reticleScale = 1f;

    [SerializeField] GameObject playerGameObject;

    CanvasGroup canvasGroup;

    #region Instance

    private static CanvasS_Gameplay _instance;
    public static CanvasS_Gameplay Instance { get { return _instance; } }

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
        playerGameObject = SceneManagerScript.Instance.GetOwnPlayerInstance();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        UI_Manager.Instance.gameplayMenuCreated = true;
    }

    void Update()
    {
        if(playerGameObject != null)
        {
            lifeSlider.value = playerGameObject.GetComponent<PlayerStats>().HP;
            inkSlider.value = playerGameObject.GetComponent<PlayerStats>().ink;
            inkSliderImg.color = SceneManagerScript.Instance.GetTeamColor(playerGameObject.GetComponent<PlayerStats>().teamTag);
        }
        else
        {
            playerGameObject = SceneManagerScript.Instance.GetOwnPlayerInstance();
        }

        if(canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += 0.5f * Time.deltaTime;
        }
    }
}
