using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : Weapon
{
    // My name is Gun, but you can call me Gus

    [Header("Weapon meshes")]
    public GameObject weaponMesh;
    public GameObject bulletPrefab;

    void Start()
    {
        
    }

    void Update()
    {
        if (shootCooldown >= 0.0f)
        {
            shootCooldown -= Time.deltaTime;
        }
        else if(isShooting)
        {
            Shoot();
            shootCooldown = 1 / cadence;
        }

        aimDirection = GetComponentInParent<PlayerMovement>().cam.transform.forward;

        weaponMesh.transform.LookAt(new Vector3(aimDirection.x + 90, aimDirection.y, aimDirection.z));
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));

        bullet.GetComponent<Bullet>().speed = bulletSpeed;
        bullet.GetComponent<Bullet>().DMG = shootDMG;
    }
}