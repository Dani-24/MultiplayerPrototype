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
    public float jumpRng = 0f;

    [Tooltip("% from the total ink that shooting once costs")]
    public float shootCost;

    public float moveSpeedMultiplier = 1.0f;

    #endregion

    #region Painting

    [Header("Painting (Bullet)")]
    public float pRadius = 1;
    public float pStrength = 1;
    public float pHardness = 1;

    [Header("Painting (Droplets)")]
    [Tooltip("Painting bullets (no dmg just paint)")]
    [SerializeField] protected int sprayDropletsNum = 0;
    [Tooltip("Radius from that bullet (mesh)")]
    [SerializeField] protected float sprayDropRadius = 1.0f;
    [Tooltip("Paint radius from that spray")]
    [SerializeField] protected float sprayPaintRadius = 1.0f;

    [Header("Painting (Own Character Position)")]
    [Tooltip("Paint own feet when shooting")]
    public bool paintOwnFeet = true;
    [Tooltip("Probability to paint your own feet each shoot")][Range(0.0f, 1.0f)] public float paintProbability = 1f;
    public float ownPaintRadius = 1;

    #endregion

    #region Debug

    [Header("Position Corrections")]
    public Transform spawnBulletPosition;

    [Tooltip("Offset to match the reticle")]
    public float shootingVerticalOffset = 0.5f;

    [Header("Debug Info")]
    protected bool isShooting = false;

    [Tooltip("Direction in which is the weapon aiming to shoot")]
    protected Vector3 wpAimDirection;

    [Header("Weapon meshes")]
    public GameObject weaponMesh;
    public GameObject bulletPrefab;

    // Counter entre tiro y tiro
    protected float shootCooldown = 0f;

    #endregion

    protected AudioSource audioS;
}
