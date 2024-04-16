using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterDummy : Weapon
{
    [Header("Shooter Additional propierties")]
    [SerializeField][Tooltip("For Burst shooters")] int bulletsPerShot = 1;
    [SerializeField] float burstBulletsSpeedReduction = 2.5f;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();

        if (bulletDropletPrefab == null) bulletDropletPrefab = bulletPrefab;

        actualBulletCost = shootCost;
    }

    void Update()
    {
        isShooting = GetComponentInParent<Dummy>().weaponShooting;

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

        MaterialsFromTeamColor();
    }

    void MaterialsFromTeamColor()
    {
        if (rend.Count > 0)
        {
            foreach (Renderer r in rend)
                r.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);
        }
    }

    void Shoot()
    {
        Vector3 aimDirVec = transform.rotation.eulerAngles;
        // RNG
        aimDirVec.y += Random.Range(-rng, rng);

        GameObject bullet = Instantiate(bulletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));
        bullet.GetComponent<DefaultBullet>().isShotByOwnPlayer = true;
        bullet.GetComponent<DefaultBullet>().weaponShootingThis = weaponName;
        bullet.GetComponent<DefaultBullet>().teamTag = teamTag;
        bullet.GetComponent<DefaultBullet>().speed = bulletSpeed;
        bullet.GetComponent<DefaultBullet>().range = weaponRange;
        bullet.GetComponent<DefaultBullet>().DMG = shootDMG;
        bullet.GetComponent<DefaultBullet>().pRadius = pRadius;
        bullet.GetComponent<DefaultBullet>().pHardness = pHardness;
        bullet.GetComponent<DefaultBullet>().pStrength = pStrength;
        bullet.GetComponent<DefaultBullet>().meshScale = 1;

        for (int i = 0; i < bulletsPerShot - 1; i++)
        {
            GameObject bulletb = Instantiate(bulletPrefab, spawnBulletPosition.transform.position, Quaternion.Euler(aimDirVec));

            bulletb.GetComponent<DefaultBullet>().teamTag = teamTag;
            bulletb.GetComponent<DefaultBullet>().speed = bulletSpeed - burstBulletsSpeedReduction * i;
            bulletb.GetComponent<DefaultBullet>().range = weaponRange;
            bulletb.GetComponent<DefaultBullet>().DMG = shootDMG;
            bulletb.GetComponent<DefaultBullet>().pRadius = pRadius;
            bulletb.GetComponent<DefaultBullet>().pHardness = pHardness;
            bulletb.GetComponent<DefaultBullet>().pStrength = pStrength;
            bulletb.GetComponent<DefaultBullet>().meshScale = 1;
        }

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

        // Sound
        audioS.volume = Random.Range(0.9f, 1.0f);
        audioS.pitch = Random.Range(0.9f, 1.1f);
        audioS.PlayOneShot(audioS.clip);
    }
}
