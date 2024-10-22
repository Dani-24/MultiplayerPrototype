using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [HideInInspector] public string teamTag;

    public bool isShotByOwnPlayer;

    public string weaponName;

    #region Propierties

    [Header("Weapon Stats")]
    public float cadence = 1f;
    public float shootDMG = 35f;
    public float bulletSpeed = 10f;
    public float weaponRange = 4f;
    public float rng = 0f;
    public float jumpRng = 0f;

    [Tooltip("Frames until first shot when not shooting")]
    public float firstShootCooldown;
    protected float actualShootFrame = 0;

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
    [SerializeField] protected float sprayDropMeshRadius = 1.0f;
    [Tooltip("Paint radius from that spray")]
    [SerializeField] protected float sprayPaintRadius = 1.0f;

    [SerializeField] protected bool linearPaint = false;
    [SerializeField] protected float dropletsDistance = 1;

    #endregion

    #region Debug

    [Header("Position Corrections")]
    public Transform spawnBulletPosition;

    [Tooltip("Offset to match the reticle")]
    public float shootingVerticalOffset = 0.5f;

    [Header("Debug Info")]
    public float actualBulletCost;
    protected bool isShooting = false;

    [Tooltip("Direction in which is the weapon aiming to shoot")]
    protected Vector3 wpAimDirection;

    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public GameObject bulletDropletPrefab;

    // Counter entre tiro y tiro
    protected float shootCooldown = 0f;

    #endregion

    protected AudioSource audioS;

    [SerializeField] protected List<Renderer> rend = new List<Renderer>();

    public Sprite weaponSprite;

    private void FixedUpdate()
    {
        // =========== ROTACI�N DEL ARMA ===========
        if (isShooting) transform.rotation = Quaternion.LookRotation(wpAimDirection);
        else transform.rotation = Quaternion.LookRotation(transform.forward);
    }
}
