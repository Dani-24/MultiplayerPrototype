using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string teamTag;

    protected Rigidbody rb;
    protected Renderer rend;

    public string weaponShootingThis;
    public bool isShotByOwnPlayer;

    #region Propierties

    [Header("Propierties")]
    public float speed = 10f;
    public float range = 5f;
    public float DMG = 35f;

    protected Vector3 initPos;

    public float meshScale = 1f;
    protected Vector3 meshScaleVec;

    #endregion

    #region Paint

    [Header("Painting")]
    public float pRadius = 1;
    public float pStrength = 1;
    public float pHardness = 1;

    [Header("Linear Painting")]
    public bool linearPainting = false;
    public float dropletsDistance;
    public float dropletPaintRadius;
    public float dropletMeshScale;
    public GameObject bulletDroplet;
    Vector3 lastDropletPos = Vector3.zero;

    #endregion

    [Header("Transforms")]
    [Tooltip("Y pos where bullet gets destroyed")][SerializeField] protected float minYaxis = -20;
    [SerializeField] protected Transform meshTransform;

    [Header("Audio")]
    public AudioClip hitPlayerSFX;
    public AudioClip hitPaintableSurfaceSFX;
    public AudioClip hitUnpaintableSurfaceSFX;

    public AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Delete if fall from the map
        if (transform.position.y < minYaxis)
            Destroy(gameObject);

        if (linearPainting)
        {
            if (lastDropletPos == Vector3.zero) lastDropletPos = initPos;

            if (Vector3.Distance(lastDropletPos, transform.position) >= dropletsDistance)
            {
                GameObject mainSprayDrop = Instantiate(bulletDroplet, transform.position, Quaternion.identity);

                mainSprayDrop.GetComponent<DefaultBullet>().teamTag = teamTag;
                mainSprayDrop.GetComponent<DefaultBullet>().speed = 1;
                mainSprayDrop.GetComponent<DefaultBullet>().range = 0;
                mainSprayDrop.GetComponent<DefaultBullet>().DMG = 0;
                mainSprayDrop.GetComponent<DefaultBullet>().pRadius = dropletPaintRadius;
                mainSprayDrop.GetComponent<DefaultBullet>().pHardness = pHardness;
                mainSprayDrop.GetComponent<DefaultBullet>().pStrength = pStrength;
                mainSprayDrop.GetComponent<DefaultBullet>().meshScale = dropletMeshScale;

                lastDropletPos = transform.position;
            }
        }
    }
}
