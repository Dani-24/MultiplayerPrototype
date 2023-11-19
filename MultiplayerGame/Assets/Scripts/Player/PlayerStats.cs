using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public string teamTag;

    [Header("Debug")]
    public bool infiniteInk = false;
    public bool infiniteHP = false;
    public string presetTeam;

    [Header("HP")]
    public float HP = 100.0f;
    float maxHP;

    public bool isDead = false;

    [Header("Ink")]
    public float ink = 100.0f;
    float inkCapacity;

    public float inkReloadSpeed = 1f;
    public float inkReloadSpeedOnInk = 5f;
    public bool onInk = false;

    [Header("Other")]
    [SerializeField]
    Vector3 spawnPos = Vector3.zero;

    private CharacterController controller;

    [SerializeField] float minYaxis = -20;

    [SerializeField] MeshRenderer teamColorGO;

    [Header("UI Things")]
    public Slider inkSlider;
    public Image inkSliderImg;

    public Slider HPSlider;

    void Start()
    {
        maxHP = HP;
        inkCapacity = ink;

        controller = GetComponent<CharacterController>();

        teamTag = SceneManagerScript.Instance.SetTeam(gameObject, presetTeam);
    }

    void Update()
    {
        teamTag = gameObject.tag;

        // Check Color
        teamColorGO.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);

        // Check Death
        if (transform.position.y < minYaxis || HP <= 0) isDead = true;

        if (isDead)
        {
            controller.enabled = false;
            transform.position = spawnPos;
            transform.rotation = Quaternion.Euler(Vector3.zero);
            controller.enabled = true;

            isDead = false;
            HP = maxHP;
            ink = inkCapacity;
        }

        // Update UI
        inkSlider.value = ink;
        HPSlider.value = HP;
        inkSliderImg.color = SceneManagerScript.Instance.GetTeamColor(teamTag);

        // Reloading
        ReloadInk();

        // Debug
        if (infiniteHP) { HP = maxHP; }
        if (infiniteInk) { ink = inkCapacity; }
    }

    void ReloadInk()
    {
        if (ink < inkCapacity && !GetComponent<PlayerArmament>().weaponShooting /*&& !GetComponent<PlayerMovement>().subWeaponShooting*/)
        {
            if (onInk)
            {
                ink += inkReloadSpeedOnInk * Time.deltaTime;
            }
            else
            {
                ink += inkReloadSpeed * Time.deltaTime;
            }
        }

        if (ink > inkCapacity) { ink = inkCapacity; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(SceneManagerScript.Instance.GetRivalTag(teamTag) + "Bullet"))
        {
            HP -= other.gameObject.GetComponent<Bullet>().DMG;
        }
    }

    public void ChangeTag(string newTag)
    {
        if (newTag != teamTag)
        {
            teamTag = SceneManagerScript.Instance.SetTeam(gameObject, newTag);
        }
    }
}
