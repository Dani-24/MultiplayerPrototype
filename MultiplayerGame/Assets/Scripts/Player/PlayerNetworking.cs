using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworking : MonoBehaviour
{
    public bool isOwnByClient;
    [SerializeField] List<GameObject> gameObjectsToHideIfNotOwned = new List<GameObject>();

    [SerializeField] AudioListener audioListener;

    void Start()
    {
        HideGameObjects(isOwnByClient);
    }

    void Update()
    {
        if (isOwnByClient) { audioListener.enabled = true; } else { audioListener.enabled = false; }
    }

    void HideGameObjects(bool hide)
    {
        for (int i = 0; i < gameObjectsToHideIfNotOwned.Count; i++)
        {
            gameObjectsToHideIfNotOwned[i].SetActive(hide);
        }
    }
}