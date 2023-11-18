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

    GameObject currentWeapon;
    bool createdWeapon = false;

    [Header("SubWeapon")]
    public GameObject subWeapon;
    public bool subWeaponShooting = false;

    bool chargingSub = false;

    [Header("Special Weapon (Not Implemented YET)")]
    public GameObject specialWeapon;
    public bool specialWeaponShooting = false;

    void Update()
    {
        if (!SceneManagerScript.Instance.GetComponent<UI_Manager>().showUI)
        {
            // AIMING
            if (GetComponentInParent<OrbitCamera>().affectedCamera != null)
            {
                aimDirection = GetComponentInParent<OrbitCamera>().affectedCamera.transform.forward;
            }

            // WEAPON
            if (!createdWeapon || currentWeapon.GetComponent<Weapon>().weaponName != weaponToUse.GetComponent<Weapon>().weaponName)
            {
                CreateWeapon();
            }

            // SUB WEAPON
            if (subWeaponShooting)
            {
                ChargeSub();
            }
            else if (chargingSub)
            {
                ThrowSub();
            }
        }
        else
        {
            weaponShooting = false; subWeaponShooting = false; chargingSub = false;
        }
    }

    #region Main Weapon

    void CreateWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }

        currentWeapon = Instantiate(weaponToUse, weaponSpawnPoint.transform);
        currentWeapon.GetComponent<Weapon>().teamTag = GetComponent<PlayerStats>().teamTag;
        createdWeapon = true;
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

    #endregion

    #region Player Input Actions

    void OnFire(InputValue value)
    {
        weaponShooting = value.isPressed;
    }

    void OnSubFire(InputValue value)
    {
        subWeaponShooting = value.isPressed;
    }

    #endregion
}
