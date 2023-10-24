using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Weapon : MonoBehaviour
{
    public string weaponName;

    [Header("Weapon Stats")]
    public float cadence = 1f;

    public float shootDMG = 35f;

    public float bulletSpeed = 10f;

    public float weaponRange = 4f;

    public float rng = 0f;

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
