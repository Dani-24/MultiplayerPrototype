using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public float cadence = 1f;

    public float shootDMG = 35f;

    public float bulletSpeed = 10f;

    public float weaponRange = 4f;

    public float rng = 5f;

    [Header("Spawn Position")]
    public Transform spawnBulletPosition;

    [Header("Debug Info")]
    public bool isShooting = false;

    // Counter entre tiro y tiro
    [SerializeField]
    protected float shootCooldown = 0f;

    //[HideInInspector]
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

    void OnFire(InputValue value)
    {
        isShooting = value.isPressed;
    }
}
