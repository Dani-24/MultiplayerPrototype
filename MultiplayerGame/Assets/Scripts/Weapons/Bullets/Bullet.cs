using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public string teamTag;

    private Rigidbody rb;
    private Renderer rend;

    #region Propierties

    [Header("Propierties")]
    public float speed = 10f;
    public float range = 5f;
    public float DMG = 35f;
    public float customGravity = -9.81f;

    #endregion

    #region Paint

    [Header("Painting")]
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    #endregion

    [Header("Other")]

    [SerializeField] float minYaxis = -20;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {
        rb.velocity = transform.forward * speed;
        float acc = (speed * speed) / (2 * range);
        customGravity = acc / -9.81f;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = SceneManagerScript.Instance.GetTeamColor(teamTag);
        }

        gameObject.tag = teamTag + "Bullet";
    }

    private void FixedUpdate()
    {
        rb.velocity += new Vector3(0, customGravity, 0);

        if (transform.position.y < minYaxis)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Paintable p = other.GetComponent<Paintable>();
        if (p != null)
        {
            // Se deberia cambiar a que pinte lo mas cercano (como las bombas, o solo pintara lo q choque primero en puntos con diversos objetos)
            Vector3 pos = other.ClosestPointOnBounds(transform.position);
            PaintManager.instance.paint(p, pos, radius, hardness, strength, rend.material.color);
        }
        Destroy(gameObject);
    }

    // Just to be secure
    private void OnTriggerStay(Collider other)
    {
        Destroy(gameObject);
    }
}