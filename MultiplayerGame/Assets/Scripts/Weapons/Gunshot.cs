using UnityEngine;

public class Gunshot : Weapon
{

    [Header("Gunshot Special Propierties")]
    [SerializeField] int pellets;
    [Tooltip("Read Only")] public float dmgPerPellet;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
        Random.InitState(0);

        if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
        {
            audioS.spatialBlend = 0;
        }

        dmgPerPellet = shootDMG / pellets;

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

            for (int i = 0; i < pellets; i++)
            {
                // Vertical RNG
                if (GetComponentInParent<PlayerMovement>().isGrounded) aimDirVec.x += Random.Range(-rng / 2, rng / 2); else aimDirVec.x += Random.Range(-jumpRng / 2, jumpRng / 2);

                // Horizontal RNG
                if (GetComponentInParent<PlayerMovement>().isGrounded) aimDirVec.y += Random.Range(-rng, rng); else aimDirVec.y += Random.Range(-jumpRng, jumpRng);

                GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

                bullet.GetComponent<DefaultBullet>().teamTag = teamTag;
                bullet.GetComponent<DefaultBullet>().speed = bulletSpeed;
                bullet.GetComponent<DefaultBullet>().range = weaponRange;
                bullet.GetComponent<DefaultBullet>().DMG = shootDMG / pellets;
                bullet.GetComponent<DefaultBullet>().pRadius = pRadius;
                bullet.GetComponent<DefaultBullet>().pHardness = pHardness;
                bullet.GetComponent<DefaultBullet>().pStrength = pStrength;
                bullet.GetComponent<DefaultBullet>().meshScale = 1;

                // Ink droplets (Per pellet)
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
