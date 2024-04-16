using Unity.VisualScripting;
using UnityEngine;

public class Minigun : Weapon
{
    [Header("Minigun propierties")]
    [SerializeField] float costIncrease;
    [SerializeField] float costRecoverSpeed = 1.0f;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
        Random.InitState(0);

        if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
            audioS.spatialBlend = 0;

        if (bulletDropletPrefab == null) bulletDropletPrefab = bulletPrefab;

        actualBulletCost = shootCost;

        isShotByOwnPlayer = GetComponentInParent<PlayerNetworking>().isOwnByThisInstance;
    }

    void Update()
    {
        isShooting = GetComponentInParent<PlayerArmament>().weaponShooting;

        // Aiming Rotation
        wpAimDirection = GetComponentInParent<PlayerArmament>().aimDirection;
        wpAimDirection.y += shootingVerticalOffset;

        // ====== Disparar ======
        if (shootCooldown >= 0.0f)
            shootCooldown -= Time.deltaTime;
        else if (isShooting)
        {
            if (actualShootFrame >= firstShootCooldown)
            {
                Shoot();
                shootCooldown = 1 / cadence;
            }
            else
                actualShootFrame++;
        }
        else
            actualShootFrame = 0;

        if (!isShooting && actualBulletCost > shootCost) actualBulletCost -= costIncrease * costRecoverSpeed * Time.deltaTime;

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
        if (GetComponentInParent<PlayerStats>().ink >= actualBulletCost)
        {
            Quaternion aimDirQ = Quaternion.LookRotation(wpAimDirection);

            Vector3 aimDirVec = aimDirQ.eulerAngles;

            if (GetComponentInParent<PlayerNetworking>().isOwnByThisInstance)
            {
                GetComponentInParent<PlayerArmament>().weaponRngState = Random.state;
            }
            else
            {
                Random.state = GetComponentInParent<PlayerArmament>().weaponRngState;
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
            bullet.GetComponent<DefaultBullet>().isShotByOwnPlayer = isShotByOwnPlayer;
            bullet.GetComponent<DefaultBullet>().weaponShootingThis = weaponName;
            bullet.GetComponent<DefaultBullet>().teamTag = teamTag;
            bullet.GetComponent<DefaultBullet>().speed = bulletSpeed;
            bullet.GetComponent<DefaultBullet>().range = weaponRange;
            bullet.GetComponent<DefaultBullet>().DMG = shootDMG;
            bullet.GetComponent<DefaultBullet>().pRadius = pRadius;
            bullet.GetComponent<DefaultBullet>().pHardness = pHardness;
            bullet.GetComponent<DefaultBullet>().pStrength = pStrength;
            bullet.GetComponent<DefaultBullet>().meshScale = 1;

            // Ink droplets
            for (int i = 0; i < sprayDropletsNum; i++)
            {
                GameObject sprayDrop = Instantiate(bulletDropletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

                sprayDrop.GetComponent<DefaultBullet>().teamTag = teamTag;
                sprayDrop.GetComponent<DefaultBullet>().speed = bulletSpeed;
                sprayDrop.GetComponent<DefaultBullet>().range = Random.Range(0, weaponRange);
                sprayDrop.GetComponent<DefaultBullet>().DMG = 0;
                sprayDrop.GetComponent<DefaultBullet>().pRadius = sprayPaintRadius;
                sprayDrop.GetComponent<DefaultBullet>().pHardness = pHardness;
                sprayDrop.GetComponent<DefaultBullet>().pStrength = pStrength;
                sprayDrop.GetComponent<DefaultBullet>().meshScale = sprayDropMeshRadius;
            }

            // Cost ink
            GetComponentInParent<PlayerStats>().ink -= actualBulletCost;

            // Sound
            audioS.volume = Random.Range(0.9f, 1.0f);
            audioS.pitch = Random.Range(0.9f, 1.1f);
            audioS.PlayOneShot(audioS.clip);

            actualBulletCost += costIncrease;
        }
    }
}
