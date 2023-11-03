using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName;

    [Header("Weapon Stats")]
    public float cadence = 1f;

    public float shootDMG = 35f;

    public float bulletSpeed = 10f;

    public float weaponRange = 4f;

    public float rng = 0f;

    [Tooltip("% from the total ink that shooting once costs")]
    public float shootCost;

    [Header("Position Corrections")]
    public Transform spawnBulletPosition;

    public float verticalShootingOffset;

    [Header("Debug Info")]
    public bool isShooting = false;

    // Counter entre tiro y tiro
    [SerializeField]
    protected float shootCooldown = 0f;

    [HideInInspector]
    public Vector3 aimDirection;

    [Header("Weapon meshes")]
    public GameObject weaponMesh;
    public GameObject bulletPrefab;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
