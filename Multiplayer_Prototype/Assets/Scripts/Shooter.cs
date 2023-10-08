using UnityEngine;

public class Shooter : Weapon
{
    // My name is Gun, but you can call me Gus

    void Start()
    {
        
    }

    void Update()
    {
        if (shootCooldown >= 0.0f)
        {
            shootCooldown -= Time.deltaTime;
        }
        else if (isShooting)
        {
            Shoot();
            shootCooldown = 1 / cadence;
        }

        aimDirection = GetComponentInParent<PlayerMovement>().cam.transform.forward;

        // =========== ROTACIÓN DEL ARMA ===========

        Quaternion weaponRot = Quaternion.LookRotation(aimDirection, GetComponentInParent<PlayerMovement>().cam.transform.up);

        //float maxVerticalRot = 75f;
        //float maxHorizontalRot = 20f;

        //Vector3 weaponRotEuler = weaponRot.eulerAngles;

        //weaponRotEuler.x = Mathf.Clamp(weaponRotEuler.x, 360 - maxVerticalRot, maxVerticalRot);
        //weaponRotEuler.y = Mathf.Clamp(weaponRotEuler.y, 360 - maxHorizontalRot, maxHorizontalRot);

        weaponMesh.transform.rotation = weaponRot; //Quaternion.Euler(weaponRotEuler);
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));

        bullet.GetComponent<Bullet>().speed = bulletSpeed;
        bullet.GetComponent<Bullet>().travelDistance = weaponRange;
        bullet.GetComponent<Bullet>().DMG = shootDMG;
    }
}