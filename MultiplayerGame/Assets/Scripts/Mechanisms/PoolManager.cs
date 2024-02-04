using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] bool changeWaterLevel = false;
    bool done = false;

    [SerializeField]
    GameObject[] GameObjectsToBroadcast;

    [SerializeField] List<AudioSource> audioSources = new List<AudioSource>();

    NetGameObject netObject;

    [Header("Change Conditions")]
    [SerializeField][Range(0, 1)] float puntuationNeeded = 0.5f;
    [SerializeField] float timerNeeded = 120;

    private void Start()
    {
        netObject = GetComponent<NetGameObject>();
    }

    private void Update()
    {
        if (changeWaterLevel && !done) ChangeWaterLevel();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha0)) changeWaterLevel = true;
#endif

        // Netcode
        if (!netObject.connectedToServer)
        {
            if (changeWaterLevel) netObject.netValue = 1; else netObject.netValue = 0;
        }
        else
        {
            if (netObject.netValue == 1) changeWaterLevel = true; else changeWaterLevel = false;
        }

        // Match states
        if (GameManagerScript.Instance.matchState == GameManagerScript.MatchState.playing)
        {
            if (GameManagerScript.Instance.gameMode == GameManagerScript.GameMode.towah)
            {
                if (GameManagerScript.Instance.tower.alphaRecord >= puntuationNeeded || GameManagerScript.Instance.tower.betaRecord >= puntuationNeeded)
                {
                    changeWaterLevel = true;
                }
            }

            if (GameManagerScript.Instance.timerCount < timerNeeded)
            {
                changeWaterLevel = true;
            }
        }

        if (changeWaterLevel && GameManagerScript.Instance.gameMode == GameManagerScript.GameMode.towah)
        {
            GameManagerScript.Instance.tower.DrawTowerPath();
            GameManagerScript.Instance.tower.RecalculatePathLength();
        }
    }

    public void ChangeWaterLevel()
    {
        for (int i = 0; i < GameObjectsToBroadcast.Length; ++i)
        {
            GameObjectsToBroadcast[i].BroadcastMessage("ChangeWaterLevel");
        }

        foreach (var audioSource in audioSources)
        {
            audioSource.Play();
        }

        done = true;
    }
}
