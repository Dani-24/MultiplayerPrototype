using UnityEngine;

public class ShooterDummy : Weapon
{
    void Update()
    {
        isShooting = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().weaponShooting;

        // =========== ROTACIÓN DEL ARMA ===========

        if (isShooting)
        {
            aimDirection = transform.forward;
            weaponMesh.transform.rotation = Quaternion.LookRotation(aimDirection, transform.up);
        }
        else
        {
            weaponMesh.transform.rotation = Quaternion.LookRotation(transform.forward);
        }

        if (shootCooldown >= 0.0f)
        {
            shootCooldown -= Time.deltaTime;
        }
        else if (isShooting)
        {
            Shoot();
            shootCooldown = 1 / cadence;
        }
    }

    void Shoot()
    {
        // Shooting direction
        aimDirection.y += verticalShootingOffset;

        // RNG
        aimDirection.x += Random.Range(-rng, rng);

        GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));

        bullet.tag = "EnemyBullet";
        bullet.GetComponent<Bullet>().speed = bulletSpeed;
        bullet.GetComponent<Bullet>().travelDistance = weaponRange;
        bullet.GetComponent<Bullet>().DMG = shootDMG;
    }
}
