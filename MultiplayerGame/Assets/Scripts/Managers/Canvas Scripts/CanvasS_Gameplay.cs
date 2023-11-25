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

    void Start()
    {
        playerGameObject = SceneManagerScript.Instance.GetOwnPlayerInstance();
    }

    void Update()
    {
        if(playerGameObject != null)
        {
            lifeSlider.value = playerGameObject.GetComponent<PlayerStats>().HP;
            inkSlider.value = playerGameObject.GetComponent<PlayerStats>().ink;
            inkSliderImg.color = SceneManagerScript.Instance.GetTeamColor(playerGameObject.GetComponent<PlayerStats>().teamTag);
        }
    }
}
