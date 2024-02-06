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

                rb.velocity = new Vector3(Mathf.LerpUnclamped(rb.velocity.x, 0, fallSpeedMultiplier * Time.deltaTime), Mathf.LerpUnclamped(rb.velocity.y, customGravity, fallSpeedMultiplier / 2 * Time.deltaTime), Mathf.LerpUnclamped(rb.velocity.z, 0, fallSpeedMultiplier * Time.deltaTime));

                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        audioSource.clip = null;
        if (isShotByOwnPlayer)
        {
            if (other.CompareTag(SceneManagerScript.Instance.GetRivalTag(teamTag)) && this.CompareTag(teamTag + "Bullet"))
            {
                audioSource.volume = 1;
                audioSource.clip = hitPlayerSFX;
                audioSource.spatialBlend = 0;
                audioSource.pitch = 1;

                if (other.GetComponent<PlayerStats>())
                    other.GetComponent<PlayerStats>().OnDMGReceive(weaponShootingThis, DMG, ConnectionManager.Instance.userName);
                else if (other.GetComponent<Dummy>())
                    other.GetComponent<Dummy>().OnDMGReceive(weaponShootingThis, DMG, ConnectionManager.Instance.userName);
            }
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, pRadius);

        bool isPaintable = false;

        foreach (Collider collider in colliders)
        {
            Paintable p = collider.gameObject.GetComponent<Paintable>();
            if (p != null)
            {
                Vector3 pos = other.ClosestPointOnBounds(transform.position);
                PaintManager.instance.Paint(p, pos, pRadius, pHardness, pStrength, rend.material.color);
                isPaintable = true;
            }
        }

        if (audioSource.clip == null)
        {
            if (isPaintable) audioSource.clip = hitPaintableSurfaceSFX; else audioSource.clip = hitUnpaintableSurfaceSFX;

            audioSource.volume = 0.15f;
            audioSource.spatialBlend = 1;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }

        audioSource.Play();
        audioSource.transform.parent = null;
        audioSource.GetComponent<BulletAudioAutomorision>().pendingToDelete = true;
        Destroy(gameObject);
    }

    public enum BulletState
    {
        Shoot,
        Fall
    }
}