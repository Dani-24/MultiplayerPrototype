using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBullet : Bullet
{
    [Header("Explosive Propierties")]
    public float explosionRadius;
    public AnimationCurve dmgCurve;

    [SerializeField] GameObject explosionObject;
    [SerializeField] AudioClip explosionSFX;

    void Start()
    {
        rb.velocity = transform.forward * speed;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);
        }

        meshScaleVec.Set(meshScale, meshScale, meshScale);

        meshTransform.localScale = meshScaleVec;

        gameObject.tag = teamTag + "Bullet";

        initPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, initPos) > range)
            OnExplosion();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (whoShotThis != null && other.GetComponent<CharacterController>() == whoShotThis) return;
        OnExplosion();
    }

    public void OnExplosion()
    {
        // Vfx
        GameObject explo = Instantiate(explosionObject, transform.position, transform.rotation);
        explo.GetComponent<AudioSource>().clip = explosionSFX;
        explo.GetComponent<AudioSource>().Play();
        explo.GetComponent<Explosive>().maxRadius = explosionRadius * 1.5f;
        explo.GetComponent<Renderer>().material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        List<DMGPackage> affectedPlayers = new();

        foreach (Collider hit in colliders)
        {
            // Hit
            if (isShotByOwnPlayer && hit.CompareTag(SceneManagerScript.Instance.GetRivalTag(teamTag)) && this.CompareTag(teamTag + "Bullet"))
            {
                float dist = Vector3.Distance(transform.position, hit.ClosestPointOnBounds(transform.position)) / explosionRadius;
                float dmgDealt = dmgCurve.Evaluate(dist) * DMG;

                if (hit.GetComponent<PlayerStats>())
                {
                    DMGPackage dmg = new DMGPackage
                    {
                        dmg = dmgDealt,
                        cause = weaponShootingThis,
                        dealer = ConnectionManager.Instance.userName,
                        receiverID = hit.GetComponent<PlayerNetworking>().networkID
                    };
                    affectedPlayers.Add(dmg);
                }
                else if (hit.GetComponent<Dummy>())
                    hit.GetComponent<Dummy>().OnDMGReceive(weaponShootingThis, dmgDealt, ConnectionManager.Instance.userName);
            }

            if (affectedPlayers.Count > 0)
            {
                Package dmgPckg = ConnectionManager.Instance.WritePackage(Pck_type.DMG);
                dmgPckg.dMGPackages = affectedPlayers;
                ConnectionManager.Instance.SendPackage(dmgPckg);
            }

            // Main Bullet Paint
            Paintable p = hit.GetComponent<Paintable>();
            if (p != null)
            {
                Vector3 pos = hit.ClosestPointOnBounds(transform.position);
                PaintManager.instance.Paint(p, pos, pRadius, pHardness, pStrength, rend.material.color);
            }
        }

        // End
        Destroy(gameObject);
    }
}
