using UnityEngine;

public class Blaster : Weapon
{
    [Header("Gunshot Special Propierties")]
    [SerializeField] float oneShotRadius;
    [SerializeField] float splashRadius;

    [SerializeField] float splashMaxDmg;
    [SerializeField] float splashMinDmg;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
        Random.InitState(0);

        if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
        {
            audioS.spatialBlend = 0;
        }

        if (bulletDropletPrefab == null) bulletDropletPrefab = bulletPrefab;
    }

    void Update()
    {
        isShooting = GetComponentInParent<PlayerArmament>().weaponShooting;

        // Aiming Rotation
        wpAimDirection = GetComponentInParent<PlayerArmament>().aimDirection;
        wpAimDirection.y += shootingVerticalOffset;

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

        MaterialsFromTeamColor();
    }

    private void FixedUpdate()
    {
        // =========== ROTACIÓN DEL ARMA ===========

        if (isShooting)
        {
            weaponMesh.transform.rotation = Quaternion.LookRotation(wpAimDirection);
        }
        else
        {
            weaponMesh.transform.rotation = Quaternion.LookRotation(transform.forward);
        }
    }

    void MaterialsFromTeamColor()
    {
        if (rend.Count > 0)
        {
            foreach (Renderer r in rend)
            {
                r.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);
            }
        }
    }

    void Shoot()
    {
        if (GetComponentInParent<PlayerStats>().ink >= shootCost)
        {
            if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
            {
                GetComponentInParent<PlayerArmament>().weaponRngState = Random.state;
            }
            else
            {
                Random.state = GetComponentInParent<PlayerArmament>().weaponRngState;
            }

            Vector3 aimDirVec = Quaternion.LookRotation(wpAimDirection).eulerAngles;

            // Horizontal RNG
            if (GetComponentInParent<PlayerMovement>().isGrounded) aimDirVec.y += Random.Range(-rng, rng); else aimDirVec.y += Random.Range(-jumpRng, jumpRng);

            GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

            bullet.GetComponent<ExplosiveBullet>().teamTag = teamTag;
            bullet.GetComponent<ExplosiveBullet>().speed = bulletSpeed;
            bullet.GetComponent<ExplosiveBullet>().range = weaponRange;
            bullet.GetComponent<ExplosiveBullet>().DMG = shootDMG;
            bullet.GetComponent<ExplosiveBullet>().pRadius = pRadius;
            bullet.GetComponent<ExplosiveBullet>().pHardness = pHardness;
            bullet.GetComponent<ExplosiveBullet>().pStrength = pStrength;
            bullet.GetComponent<ExplosiveBullet>().meshScale = 1;
            bullet.GetComponent<ExplosiveBullet>().oneShotRadius = oneShotRadius;
            bullet.GetComponent<ExplosiveBullet>().splashRadius = splashRadius;
            bullet.GetComponent<ExplosiveBullet>().splashMaxDmg = splashMaxDmg;
            bullet.GetComponent<ExplosiveBullet>().splashMinDmg = splashMinDmg;

            GameObject mainSprayDrop = Instantiate(bulletDropletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

            mainSprayDrop.GetComponent<DefaultBullet>().teamTag = teamTag;
            mainSprayDrop.GetComponent<DefaultBullet>().speed = bulletSpeed;
            mainSprayDrop.GetComponent<DefaultBullet>().range = weaponRange;
            mainSprayDrop.GetComponent<DefaultBullet>().DMG = 0;
            mainSprayDrop.GetComponent<DefaultBullet>().pRadius = pRadius;
            mainSprayDrop.GetComponent<DefaultBullet>().pHardness = pHardness;
            mainSprayDrop.GetComponent<DefaultBullet>().pStrength = pStrength;
            mainSprayDrop.GetComponent<DefaultBullet>().meshScale = sprayDropRadius;

            // Ink droplets
            for (int j = 0; j < sprayDropletsNum; j++)
            {
                GameObject sprayDrop = Instantiate(bulletDropletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

                sprayDrop.GetComponent<DefaultBullet>().teamTag = teamTag;
                sprayDrop.GetComponent<DefaultBullet>().speed = bulletSpeed;
                sprayDrop.GetComponent<DefaultBullet>().range = Random.Range(0, weaponRange);
                sprayDrop.GetComponent<DefaultBullet>().DMG = 0;
                sprayDrop.GetComponent<DefaultBullet>().pRadius = sprayPaintRadius;
                sprayDrop.GetComponent<DefaultBullet>().pHardness = pHardness;
                sprayDrop.GetComponent<DefaultBullet>().pStrength = pStrength;
                sprayDrop.GetComponent<DefaultBullet>().meshScale = sprayDropRadius;
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
