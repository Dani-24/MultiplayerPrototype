using UnityEngine;

public class BulletAudioAutomorision : MonoBehaviour
{
    public bool pendingToDelete = false;
    public int timer = 0;

    void Update()
    {
        if (pendingToDelete)
        {
            Destroy(gameObject, timer);
        }
    }
}
