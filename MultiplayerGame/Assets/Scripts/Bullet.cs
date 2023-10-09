using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;

    public float speed = 10f;

    public float travelDistance = 5f;

    public float DMG = 35f;

    public float customGravity = -9.81f;

    private Renderer rend;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {        
        rb.velocity = transform.forward * speed;

        float acc = (speed * speed) / (2 * travelDistance);

        customGravity = acc / -9.81f;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            if (tag == "Bullet")
            {
                rend.material.color = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneManager>().allyColor;
            }
            else
            {
                rend.material.color = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneManager>().enemyColor;
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity += new Vector3(0, customGravity, 0);

        if(transform.position.y < -20)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Bullet" && other.tag != "EnemyBullet")
        {
            Paintable p = other.GetComponent<Paintable>();
            if(p != null)
            {
                Vector3 pos = other.ClosestPointOnBounds(transform.position);
                PaintManager.instance.paint(p, pos, radius, hardness, strength, rend.material.color);
            }
            Destroy(gameObject);
        }
    }
}