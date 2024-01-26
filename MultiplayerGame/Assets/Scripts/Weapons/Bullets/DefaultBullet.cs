using UnityEngine;

public class DefaultBullet : Bullet
{
    [SerializeField] float customGravity = -9.8f;
    [SerializeField] float fallSpeedMultiplier = 1f;

    [Header("Other")]
    [SerializeField] protected BulletState bulletState;

    public void Start()
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

        bulletState = BulletState.Shoot;
    }

    private void FixedUpdate()
    {
        switch (bulletState)
        {
            case BulletState.Shoot:

                // Range
                if (Vector3.Distance(transform.position, initPos) > range)
                {
                    rb.velocity = Vector3.zero;
                    bulletState = BulletState.Fall;
                }

                break;
            case BulletState.Fall:

                rb.velocity = new Vector3(Mathf.LerpUnclamped(rb.velocity.x, 0, fallSpeedMultiplier * Time.deltaTime), Mathf.LerpUnclamped(rb.velocity.y, customGravity, fallSpeedMultiplier/2 * Time.deltaTime), Mathf.LerpUnclamped(rb.velocity.z, 0, fallSpeedMultiplier * Time.deltaTime));

                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pRadius);

        foreach (Collider collider in colliders)
        {
            Paintable p = collider.gameObject.GetComponent<Paintable>();
            if (p != null)
            {
                Vector3 pos = other.ClosestPointOnBounds(transform.position);
                PaintManager.instance.Paint(p, pos, pRadius, pHardness, pStrength, rend.material.color);
            }
        }
        Destroy(gameObject);
    }

    public enum BulletState
    {
        Shoot,
        Fall
    }
}