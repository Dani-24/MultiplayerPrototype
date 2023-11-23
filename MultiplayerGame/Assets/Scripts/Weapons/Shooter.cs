using UnityEngine;

public class Shooter : Weapon
{
    // My name is Gun, but you can call me Gus

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
    }

    void Update()
    {
        isShooting = GetComponentInParent<PlayerArmament>().weaponShooting;

        // =========== ROTACIÓN DEL ARMA ===========

        if (isShooting)
        {
            wpAimDirection = GetComponentInParent<PlayerArmament>().aimDirection;
            wpAimDirection.y += shootingVerticalOffset;

            weaponMesh.transform.rotation = Quaternion.LookRotation(wpAimDirection, GetComponentInParent<PlayerOrbitCamera>().affectedCamera.transform.up);
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
        if (GetComponentInParent<PlayerStats>().ink >= shootCost)
        {
            // RNG
            wpAimDirection.x += Random.Range(-rng, rng);

            GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(wpAimDirection, GetComponentInParent<PlayerOrbitCamera>().affectedCamera.transform.up));

            bullet.GetComponent<Bullet>().teamTag = teamTag;
            bullet.GetComponent<Bullet>().speed = bulletSpeed;
            bullet.GetComponent<Bullet>().range = weaponRange;
            bullet.GetComponent<Bullet>().DMG = shootDMG;
            bullet.GetComponent<Bullet>().radius = pRadius;
            bullet.GetComponent<Bullet>().hardness = pHardness;
            bullet.GetComponent<Bullet>().strength = pStrength;

            // Cost ink
            GetComponentInParent<PlayerStats>().ink -= shootCost;

            // Sound
            audioS.volume = Random.Range(0.9f, 1.0f);
            audioS.pitch = Random.Range(0.9f, 1.1f);
            audioS.PlayOneShot(audioS.clip);
        }
    }
}