using UnityEngine;

public class Shooter : Weapon
{
    // My name is Gun, but you can call me Gus

    void Start()
    {
        
    }

    void Update()
    {
        isShooting = GetComponentInParent<PlayerMovement>().weaponShooting;

        // =========== ROTACIÓN DEL ARMA ===========

        if (isShooting)
        {
            aimDirection = GetComponentInParent<PlayerMovement>().cam.transform.forward;
            weaponMesh.transform.rotation = Quaternion.LookRotation(aimDirection, GetComponentInParent<PlayerMovement>().cam.transform.up);
        }
        else
        {
            weaponMesh.transform.rotation = Quaternion.LookRotation(transform.forward);
        }

        // ====== Disparar ======

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
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>().ink >= shootCost)
        {
            // Shooting direction
            aimDirection.y += verticalShootingOffset;

            // RNG
            aimDirection.x += Random.Range(-rng, rng);

            GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));

            bullet.GetComponent<Bullet>().speed = bulletSpeed;
            bullet.GetComponent<Bullet>().travelDistance = weaponRange;
            bullet.GetComponent<Bullet>().DMG = shootDMG;

            // Cost ink
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>().ink -= shootCost;
        }
    }
}