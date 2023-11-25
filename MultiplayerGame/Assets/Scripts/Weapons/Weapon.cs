using UnityEngine;

public class Weapon : MonoBehaviour
{
    [HideInInspector] public string teamTag;

    public string weaponName;

    #region Propierties

    [Header("Weapon Stats")]
    public float cadence = 1f;
    public float shootDMG = 35f;
    public float bulletSpeed = 10f;
    public float weaponRange = 4f;
    public float rng = 0f;

    [Tooltip("% from the total ink that shooting once costs")]
    public float shootCost;

    public float moveSpeedMultiplier = 1.0f;

    #endregion

    #region Painting

    [Header("Painting (Bullet)")]
    public float pRadius = 1;
    public float pStrength = 1;
    public float pHardness = 1;

    [Tooltip("WIP")] // !!!
    public bool paintOwnFeet = true;
    [Tooltip("WIP")] // !!!
    public float ownPaintRadius = 1;

    #endregion

    #region Debug

    [Header("Position Corrections")]
    public Transform spawnBulletPosition;

    [Tooltip("Offset to match the reticle")]
    public float shootingVerticalOffset = 0.5f;

    [Header("Debug Info")]
    public bool isShooting = false;

    [Tooltip("Direction in which is the weapon aiming to shoot")]
    public Vector3 wpAimDirection;

    [Header("Weapon meshes")]
    public GameObject weaponMesh;
    public GameObject bulletPrefab;

    // Counter entre tiro y tiro
    [SerializeField] protected float shootCooldown = 0f;

    #endregion

    protected AudioSource audioS;
}
