using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string teamTag;

    protected Rigidbody rb;

    protected Renderer rend;

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

    #endregion

    [Header("Transforms")]
    [Tooltip("Y pos where bullet gets destroyed")][SerializeField] protected float minYaxis = -20;
    [SerializeField] protected Transform meshTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        // Delete if fall from the map
        if (transform.position.y < minYaxis)
        {
            Destroy(gameObject);
        }
    }
}
