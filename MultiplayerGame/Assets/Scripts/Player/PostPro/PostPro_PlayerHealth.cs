using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostPro_PlayerHealth : MonoBehaviour
{
    Volume volume;
    Vignette vignetteEffect;

    [SerializeField] float intensity = 0;
    [SerializeField] float maxIntensity = 0.4f;

    void Start()
    {
        volume = GetComponent<Volume>();

        volume.profile.TryGet(out vignetteEffect);
    }

    void Update()
    {
        vignetteEffect.color.value = SceneManagerScript.Instance.GetTeamColor(SceneManagerScript.Instance.GetRivalTag(GetComponentInParent<PlayerStats>().teamTag));

        float intense = 1 - Mathf.Clamp01(GetComponentInParent<PlayerStats>().HP / GetComponentInParent<PlayerStats>().maxHP);

        intensity = Mathf.Lerp(0, maxIntensity, intense);

        vignetteEffect.intensity.value = intensity;
    }
}
