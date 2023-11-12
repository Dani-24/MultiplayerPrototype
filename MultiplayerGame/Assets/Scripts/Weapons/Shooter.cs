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
            aimDirection.y += shootingVerticalOffset;

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
            // RNG
            aimDirection.x += Random.Range(-rng, rng);

            GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, GetComponentInParent<PlayerMovement>().cam.transform.up));

            bullet.GetComponent<Bullet>().speed = bulletSpeed;
            bullet.GetComponent<Bullet>().travelDistance = weaponRange;
            bullet.GetComponent<Bullet>().DMG = shootDMG;
            bullet.GetComponent<Bullet>().radius = pRadius;
            bullet.GetComponent<Bullet>().hardness = pHardness;
            bullet.GetComponent<Bullet>().strength = pStrength;

            // Cost ink
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>().ink -= shootCost;
        }
    }
}