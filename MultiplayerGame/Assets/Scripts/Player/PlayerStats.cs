using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public string teamTag;

    [Header("Debug")]
    [SerializeField] bool infiniteInk = false;
    [SerializeField] bool infiniteHP = false;
    [SerializeField] string presetTeam;

    [Header("HP")]
    public float HP = 100.0f;
    [HideInInspector] public float maxHP;

    public bool isDead = false;

    [SerializeField][Tooltip("Time without taking dmg needed to start regen HP")][Range(0f, 3f)] float recoveryTime = 1.5f;
    [SerializeField][Range(1f, 5f)] float regenHPSpeed = 2f;
    float lastFrameHP;
    float regenCount;

    [Header("Ink")]
    public float ink = 100.0f;
    float inkCapacity;

    [SerializeField][Range(1f, 20f)] float inkReloadSpeed = 1f;
    [SerializeField][Range(1f, 20f)] float inkReloadSpeedOnInk = 5f;
    public bool onInk = false;

    [Header("Other")]

    public bool playerInputEnabled;

    public Vector3 spawnPos = Vector3.zero;

    private CharacterController controller;

    [SerializeField][Range(-25f, 0f)] float minYaxis = -20;

    [SerializeField] MeshRenderer teamColorGO;

    void Start()
    {
        lastFrameHP = maxHP = HP;
        inkCapacity = ink;

        controller = GetComponent<CharacterController>();

        teamTag = SceneManagerScript.Instance.SetTeam(gameObject, presetTeam);
    }

    void Update()
    {
        // Check Color
        teamColorGO.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);

        // Check Death
        if (transform.position.y < minYaxis || HP <= 0) isDead = true;

        if (GetComponent<PlayerNetworking>().isOwnByThisInstance)
        {
            for (int i = 0; i < ConnectionManager.Instance.playerPackages.Count; i++)
            {
                if (ConnectionManager.Instance.playerPackages[i].netID == GetComponent<PlayerNetworking>().networkID)
                {
                    if (i % 2 == 0) ChangeTag("Alpha"); else ChangeTag("Beta");
                    break;
                }
            }
        }

        if (isDead)
        {
            transform.parent = null;

            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(Vector3.zero));
            controller.enabled = true;

            isDead = false;
            HP = maxHP;
            ink = inkCapacity;
        }

        // Healing
        if (HP != maxHP)
        {
            RegenHealth();
        }

        if (HP > maxHP) { HP = maxHP; }

        // Reloading
        ReloadInk();

        // Debug
        if (infiniteHP) { HP = maxHP; }
        if (infiniteInk) { ink = inkCapacity; }

        // Net
        if (!GetComponent<PlayerNetworking>().isOwnByThisInstance) { infiniteInk = true; }
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

    void RegenHealth()
    {
        if (HP == lastFrameHP)
        {
            regenCount -= Time.deltaTime;

            if (regenCount <= 0)
            {
                HP += regenHPSpeed * Time.deltaTime;
            }
        }
        else
        {
            regenCount = recoveryTime;
        }

        lastFrameHP = HP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(SceneManagerScript.Instance.GetRivalTag(teamTag) + "Bullet") && GetComponent<PlayerNetworking>().isOwnByThisInstance)
        {
            HP -= other.gameObject.GetComponent<DefaultBullet>().DMG;
        }

        if (other.CompareTag("teamChanger"))
        {
            ChangeTag(SceneManagerScript.Instance.GetRivalTag(teamTag));
        }

        if (other.CompareTag("Death"))
        {
            isDead = true;
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
