using System.Collections.Generic;
using UnityEngine;

public class Bomb : SubWeapon
{
    Animator anim;
    Rigidbody rb;

    [SerializeField] GameObject explosionObject;
    [SerializeField] AudioClip explosionSFX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Set Velocity & Gravity needed to match the range
        rb.velocity = transform.forward * speed;
        float acc = (speed * speed) / (2 * range);
        customGravity = acc / -9.81f;

        // Set Bomb Color
        if (rend.Count > 0)
        {
            foreach (Renderer r in rend)
                r.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);
        }

        gameObject.tag = teamTag + "Bomb";
    }

    private void FixedUpdate()
    {
        rb.velocity += new Vector3(0, customGravity, 0);

        if (transform.position.y < minYaxis)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (instantExplosion) OnExplosion();
        else anim.SetTrigger("megumin");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death")) Destroy(gameObject);
    }

    public void OnExplosion()
    {
        // Visual Effect
        GameObject explo = Instantiate(explosionObject, transform.position, transform.rotation);
        explo.GetComponent<AudioSource>().clip = explosionSFX;
        explo.GetComponent<AudioSource>().Play();
        explo.GetComponent<Explosive>().maxRadius = affectedRadius * 1.5f;
        explo.GetComponent<Renderer>().material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);

        // Lethal Radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, affectedRadius);

        List<DMGPackage> affectedPlayers = new();

        foreach (Collider hit in colliders)
        {
            if (isShotByOwnPlayer && hit.CompareTag(SceneManagerScript.Instance.GetRivalTag(teamTag)) && this.CompareTag(teamTag + "Bomb"))
            {
                float dist = Vector3.Distance(transform.position, hit.ClosestPointOnBounds(transform.position)) / affectedRadius;
                float dmgDealt = dmgCurve.Evaluate(dist) * dmg;

                if (hit.GetComponent<PlayerStats>())
                {
                    DMGPackage dmg = new DMGPackage
                    {
                        dmg = dmgDealt,
                        cause = weaponName,
                        dealer = ConnectionManager.Instance.userName,
                        receiverID = hit.GetComponent<PlayerNetworking>().networkID
                    };
                    affectedPlayers.Add(dmg);
                }
                else if (hit.GetComponent<Dummy>())
                    hit.GetComponent<Dummy>().OnDMGReceive(weaponName, dmgDealt, ConnectionManager.Instance.userName);
            }

            if (affectedPlayers.Count > 0)
            {
                Package dmgPckg = ConnectionManager.Instance.WritePackage(Pck_type.DMG);
                dmgPckg.dMGPackages = affectedPlayers;
                ConnectionManager.Instance.SendPackage(dmgPckg);
            }

            // Paint only objects affected by lethal dmg area???
            Paintable p = hit.GetComponent<Paintable>();
            if (p != null)
            {
                Vector3 pos = hit.ClosestPointOnBounds(transform.position);
                PaintManager.instance.Paint(p, pos, paintRadius, hardness, strength, rend[0].material.color);
            }
        }

        Destroy(gameObject);
    }
}
