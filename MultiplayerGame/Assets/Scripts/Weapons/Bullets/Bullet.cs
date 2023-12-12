using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string teamTag;

    private Rigidbody rb;
    private Renderer rend;

    #region Propierties

    [Header("Propierties")]
    public float speed = 10f;
    public float range = 5f;
    public float DMG = 35f;
    [SerializeField] float customGravity = -9.8f;
    [SerializeField] float fallSpeedMultiplier = 1f;

    Vector3 initPos;

    public float meshScale = 1f;
    Vector3 meshScaleVec;

    #endregion

    #region Paint

    [Header("Painting")]
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    #endregion

    [Header("Other")]

    [SerializeField] BulletState bulletState;

    [Tooltip("Y pos where bullet gets destroyed")][SerializeField] float minYaxis = -20;
    [SerializeField] Transform meshTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

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

        // Delete if fall from the map
        if (transform.position.y < minYaxis)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            Paintable p = collider.gameObject.GetComponent<Paintable>();
            if (p != null)
            {
                Vector3 pos = other.ClosestPointOnBounds(transform.position);
                PaintManager.instance.Paint(p, pos, radius, hardness, strength, rend.material.color);
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