using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerArmament : MonoBehaviour
{
    [HideInInspector] public Vector3 aimDirection;

    [Tooltip("Point from where the weapon / sub weapon / special is used")]
    [SerializeField] GameObject weaponSpawnPoint;

    [Header("Weapon")]
    public GameObject weaponToUse;
    public bool weaponShooting = false;

    public GameObject currentWeapon;
    [HideInInspector] public int currentWeaponId = 0;

    [SerializeField] bool createdWeapon = false;

    [HideInInspector] public Random.State weaponRngState;

    [Header("SubWeapon")]
    public GameObject subWeapon;
    [HideInInspector] public int currentSubWeaponId = 0;
    public bool subWeaponShooting = false;

    bool chargingSub = false;

    [Header("Special Weapon (Not Implemented YET)")]
    public GameObject specialWeapon;
    public bool specialWeaponShooting = false;

    void Update()
    {
        // WEAPON
        if (!createdWeapon || currentWeapon.GetComponent<Weapon>().weaponName != weaponToUse.GetComponent<Weapon>().weaponName)
        {
            SetWeapon();
        }

        // AIMING
        aimDirection = GetComponentInParent<PlayerOrbitCamera>().GetCameraTransform().forward;

        // SUB WEAPON
        if (subWeaponShooting)
        {
            ChargeSub();
        }
        else if (chargingSub)
        {
            ThrowSub();
        }

        // Update weapon tag
        if (currentWeapon != null)
        {
            currentWeapon.GetComponent<Weapon>().teamTag = GetComponent<PlayerStats>().teamTag;
        }
    }

    #region Main Weapon

    void SetWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }

        currentWeapon = Instantiate(weaponToUse, weaponSpawnPoint.transform);
        currentWeapon.GetComponent<Weapon>().teamTag = GetComponent<PlayerStats>().teamTag;
        GetComponent<PlayerMovement>().weaponSpeedMultiplier = currentWeapon.GetComponent<Weapon>().moveSpeedMultiplier;
        createdWeapon = true;
    }

    public void ChangeWeapon(int id)
    {
        if (currentWeapon != SceneManagerScript.Instance.mainWeapons[id])
        {
            weaponToUse = SceneManagerScript.Instance.mainWeapons[id];
            currentWeaponId = id;
        }
    }

    #endregion

    #region Sub Weapon

    void ChargeSub()
    {
        chargingSub = true;
    }

    void ThrowSub()
    {
        if (GetComponent<PlayerStats>().ink >= subWeapon.GetComponent<SubWeapon>().throwCost)
        {
            // Direction & Position for the new gameObject
            Vector3 aimTo = Quaternion.LookRotation(aimDirection).eulerAngles;
            aimTo.x += subWeapon.GetComponent<SubWeapon>().aimYOffset;

            Vector3 pos = weaponSpawnPoint.transform.position;

            // Select Bomb
            GameObject bombToThrow = Instantiate(subWeapon, pos, Quaternion.Euler(aimTo));

            bombToThrow.GetComponent<SubWeapon>().teamTag = GetComponent<PlayerStats>().teamTag;

            gameObject.GetComponent<PlayerStats>().ink -= subWeapon.GetComponent<SubWeapon>().throwCost;
        }
        chargingSub = false;
    }

    public void ChangeSubWeapon(int id)
    {
        if (subWeapon != SceneManagerScript.Instance.subWeapons[id])
        {
            subWeapon = SceneManagerScript.Instance.subWeapons[id];
            currentSubWeaponId = id;
        }
    }

    #endregion

    #region Player Input Actions

    void OnFire(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance && GetComponent<PlayerStats>().playerInputEnabled)
            weaponShooting = value.isPressed;
    }

    void OnSubFire(InputValue value)
    {
        if (GetComponent<PlayerNetworking>().isOwnByThisInstance && GetComponent<PlayerStats>().playerInputEnabled)
            subWeaponShooting = value.isPressed;
    }

    // Network
    public void SetFire(bool _shoot)
    {
        weaponShooting = _shoot;
    }

    public void SetSubFire(bool _shoot)
    {
        subWeaponShooting = _shoot;
    }

    #endregion
}
