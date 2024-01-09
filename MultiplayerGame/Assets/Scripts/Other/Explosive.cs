using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float maxRadius;
    public float explosion_speed;

    AudioSource audioS;

    bool deleteGo = false;

    void Start()
    {
        audioS = GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(maxRadius, maxRadius, maxRadius), explosion_speed * Time.deltaTime);

        if (maxRadius - transform.localScale.x < 0.1f)
        {
            deleteGo = true;
            GetComponent<Renderer>().enabled = false;
        }

        if(deleteGo = true && !audioS.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }
}
