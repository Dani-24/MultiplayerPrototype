using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public float HP = 100.0f;
    float maxHP;

    public bool isDead = false;

    // time since last shot taken como cooldown???

    [Header("Ink")]
    public float ink = 100.0f;
    float inkCapacity;

    public float inkReloadSpeed = 1f;
    public float inkReloadSpeedOnInk = 5f;
    public bool onInk = false;

    // time since last shot como cooldown????

    [Header("Other")]
    [SerializeField]
    Vector3 spawnPos = Vector3.zero;

    private CharacterController controller;

    [SerializeField] float minYaxis = -20;

    [Header("UI Things")]
    public Slider inkSlider;
    public Image inkSliderImg;

    public Slider HPSlider;

    [Header("Debug")]
    public bool infiniteInk = false;
    public bool infiniteHP = false;

    void Start()
    {
        maxHP = HP;
        inkCapacity = ink;

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if(transform.position.y < minYaxis || HP <= 0) isDead = true;

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
        inkSliderImg.color = SceneManagerScript.Instance.allyColor;

        // Reloading
        ReloadInk();

        if(infiniteHP ) { HP = maxHP; }
        if(infiniteInk) { ink = inkCapacity; }
    }

    void ReloadInk()
    {
        if(ink < inkCapacity && !GetComponent<PlayerMovement>().weaponShooting /*&& !GetComponent<PlayerMovement>().subWeaponShooting*/)
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

        if(ink > inkCapacity) { ink = inkCapacity; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            HP -= other.gameObject.GetComponent<Bullet>().DMG;
        }
    }
}
