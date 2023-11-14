using System.Collections.Generic;
using UnityEngine;

public class Bomb : SubWeapon
{
    Animator anim;
    Rigidbody rb;

    List<Collider> bigDmgColliders = new List<Collider>();

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
            {
                r.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);
            }
        }

        gameObject.tag = teamTag + "Bomb";
    }

    private void FixedUpdate()
    {
        rb.velocity += new Vector3(0, customGravity, 0);

        if (transform.position.y < minYaxis)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (instantExplosion)
        {
            OnExplosion();
        }
        else
        {
            anim.SetTrigger("megumin");
        }
    }

    public void OnExplosion()
    {
        // Lethal Radius

        // AÑADIR Q CUANDO HITTEE AL PLAYER LANCE 2 RAYCAST, UNO A LA CABEZA Y OTRO A LOS PIES, SI ESTAN EN EL AREA DE EXPLOSION Y EL RAYCAST NO HA CHOCADO CON ALGUNA COVERTURA YENDO A ESOS PUNTOS DESDE LA BOMBA
        // ENTONCES DAÑARÁ AL PLAYER, SI NO NO

        Collider[] colliders = Physics.OverlapSphere(transform.position, lethalRadius);

        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Player") && this.CompareTag(teamTag + "Bomb"))
            {
                hit.GetComponent<PlayerStats>().HP -= dmg;
            }

            // Paint only objects affected by lethal dmg area???
            Paintable p = hit.GetComponent<Paintable>();
            if (p != null)
            {
                Vector3 pos = hit.ClosestPointOnBounds(transform.position);
                PaintManager.instance.paint(p, pos, paintRadius, hardness, strength, rend[0].material.color);
            }

            bigDmgColliders.Add(hit);
        }

        // Splash Radius

        colliders = Physics.OverlapSphere(transform.position, nonLethalRadius);

        foreach (Collider hit in colliders)
        {
            if (!bigDmgColliders.Contains(hit))
            {
                if (hit.CompareTag("Player") && this.CompareTag("EnemyBomb"))
                {
                    hit.GetComponent<PlayerStats>().HP -= splashDmg;
                }
            }
        }

        bigDmgColliders.Clear();

        Destroy(gameObject);
    }
}
