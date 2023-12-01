using UnityEngine;

public class Shooter : Weapon
{
    private void Start()
    {
        audioS = GetComponent<AudioSource>();
        Random.InitState(0);

        if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
        {
            audioS.spatialBlend = 0;
        }
    }

    void Update()
    {
        isShooting = GetComponentInParent<PlayerArmament>().weaponShooting;

        // =========== ROTACIÓN DEL ARMA ===========

        if (isShooting)
        {
            wpAimDirection = GetComponentInParent<PlayerArmament>().aimDirection;
            wpAimDirection.y += shootingVerticalOffset;

            weaponMesh.transform.rotation = Quaternion.LookRotation(wpAimDirection);
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
            Quaternion aimDirQ = Quaternion.LookRotation(wpAimDirection);

            Vector3 aimDirVec = aimDirQ.eulerAngles;

            if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
            {
                GetComponentInParent<PlayerNetworking>().weaponRngState = Random.state;
            }
            else
            {
                Random.state = GetComponentInParent<PlayerNetworking>().weaponRngState;
            }

            // RNG
            if (GetComponentInParent<PlayerMovement>().isGrounded)
            {
                aimDirVec.y += Random.Range(-rng, rng);
            }
            else
            {
                aimDirVec.y += Random.Range(-jumpRng, jumpRng);
            }

            GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

            bullet.GetComponent<Bullet>().teamTag = teamTag;
            bullet.GetComponent<Bullet>().speed = bulletSpeed;
            bullet.GetComponent<Bullet>().range = weaponRange;
            bullet.GetComponent<Bullet>().DMG = shootDMG;
            bullet.GetComponent<Bullet>().radius = pRadius;
            bullet.GetComponent<Bullet>().hardness = pHardness;
            bullet.GetComponent<Bullet>().strength = pStrength;
            bullet.GetComponent<Bullet>().meshScale = 1;

            for (int i = 0; i < sprayDropletsNum; i++)
            {
                GameObject sprayDrop = Instantiate(bulletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

                sprayDrop.GetComponent<Bullet>().teamTag = teamTag;
                sprayDrop.GetComponent<Bullet>().speed = bulletSpeed;
                sprayDrop.GetComponent<Bullet>().range = Random.Range(0, weaponRange);
                sprayDrop.GetComponent<Bullet>().DMG = 0;
                sprayDrop.GetComponent<Bullet>().radius = sprayPaintRadius;
                sprayDrop.GetComponent<Bullet>().hardness = pHardness;
                sprayDrop.GetComponent<Bullet>().strength = pStrength;
                sprayDrop.GetComponent<Bullet>().meshScale = sprayDropRadius;
            }

            // Cost ink
            GetComponentInParent<PlayerStats>().ink -= shootCost;

            // Sound
            audioS.volume = Random.Range(0.9f, 1.0f);
            audioS.pitch = Random.Range(0.9f, 1.1f);
            audioS.PlayOneShot(audioS.clip);
        }
    }
}