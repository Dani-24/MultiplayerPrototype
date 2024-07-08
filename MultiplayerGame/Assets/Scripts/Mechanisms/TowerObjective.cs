using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerObjective : MonoBehaviour
{
    public List<GameObject> playersOnTower = new List<GameObject>();
    [SerializeField] Transform[] checkpoints;
    [SerializeField] int nextCheckpointId = 0;
    int startCheckpointId = 0;
    [SerializeField] Transform startCheckpoint;

    [Header("Propierties")]
    [SerializeField] TowerState state = TowerState.Resting;
    [SerializeField] string teamOnTower;

    [SerializeField] float speed = 1.0f;
    [SerializeField] float backingSpeed = 2.0f;
    Vector3 dir = Vector3.zero;

    [SerializeField] float timeToBack = 5.0f;

    [Header("Multiplayer")]
    [Tooltip("Travel from 0 to 1")] public float alphaRecord;
    [Tooltip("Travel from 0 to 1")] public float betaRecord;
    [Tooltip("Travel from 0 to 1")] public float alphaTravelDist;
    [Tooltip("Travel from 0 to 1")] public float betaTravelDist;
    [SerializeField] float alphaTotalDist = 0;
    [SerializeField] float betaTotalDist = 0;
    float alphaCurrentDist = 0;
    float betaCurrentDist = 0;

    [SerializeField] AudioClip alphaAudio;
    [SerializeField] AudioClip betaAudio;
    [SerializeField] AudioClip backwardsAudio;
    AudioSource audioSource;
    float audioOriginalVolume;

    [SerializeField] List<MeshRenderer> meshesWithTeamColor = new();

    [Header("Debug")]
    [SerializeField] float towerToTargetDist = 0.1f;
    [SerializeField] float backCont = 0f;

    [Header("Net GOs")]
    [SerializeField] NetGameObject netNextCP;
    [SerializeField] NetGameObject netPosX;
    [SerializeField] NetGameObject netPosY;
    [SerializeField] NetGameObject netPosZ;

    [SerializeField] float interpolationSpeed = 1.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioOriginalVolume = audioSource.volume;

        // Start ID
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == startCheckpoint)
            {
                nextCheckpointId = startCheckpointId = i;
                break;
            }
        }

        DrawTowerPath();
        RecalculatePathLength();
    }

    void Update()
    {
        #region Check Players on tower

        if (playersOnTower.Count <= 0)
        {
            if (state != TowerState.Backing) state = TowerState.Resting;
        }
        else
        {
            int alphas = 0;
            for (int i = 0; i < playersOnTower.Count; i++)
            {
                if (playersOnTower[i] != null && playersOnTower[i].GetComponent<PlayerStats>().teamTag == "Alpha") alphas++; else alphas--;
            }

            // Start Moving if all players are from the same team
            if (Mathf.Abs(alphas) == playersOnTower.Count)
            {
                if (teamOnTower != playersOnTower[0].GetComponent<PlayerStats>().teamTag)
                {
                    teamOnTower = playersOnTower[0].GetComponent<PlayerStats>().teamTag;
                    ChangeCheckpoint();
                }
                state = TowerState.Moving;

                if (teamOnTower == "Alpha") audioSource.clip = alphaAudio; else audioSource.clip = betaAudio;
            }
            else
            {
                state = TowerState.Resting;
            }
        }

        #endregion

        TowerStates();

        dir = (checkpoints[nextCheckpointId].position - transform.position).normalized;

        // Update Checkpoints
        if (Vector3.Distance(transform.position, checkpoints[nextCheckpointId].position) < towerToTargetDist) ChangeCheckpoint();

        // Progress
        CalcProgress();

        if (GameManagerScript.Instance.matchState != GameManagerScript.MatchState.playing) audioSource.volume = 0; else audioSource.volume = audioOriginalVolume;

        // Netcode
        if (!netNextCP.connectedToServer)
        {
            netNextCP.netValue = nextCheckpointId;
            netPosX.netValue = transform.position.x;
            netPosY.netValue = transform.position.y;
            netPosZ.netValue = transform.position.z;
        }
        else
        {
            nextCheckpointId = (int)netNextCP.netValue;
        }

        // Check players deaths
        for (int i = 0; i < playersOnTower.Count; i++)
        {
            if (playersOnTower[i] != null && playersOnTower[i].GetComponent<PlayerStats>().lifeState != PlayerStats.LifeState.alive)
            {
                playersOnTower[i].transform.SetParent(null);
                playersOnTower.Remove(playersOnTower[i]);
                break;
            }
        }
    }

    void TowerStates()
    {
        switch (state)
        {
            case TowerState.Resting:

                foreach (MeshRenderer mr in meshesWithTeamColor)
                    mr.material.color = Color.white;

                audioSource.Stop();

                if (Vector3.Distance(transform.position, checkpoints[startCheckpointId].position) < towerToTargetDist)
                {
                    break;
                }

                // Start Backing
                if (nextCheckpointId != startCheckpointId)
                {
                    if (backCont >= timeToBack)
                    {
                        backCont = 0;

                        if (nextCheckpointId < startCheckpointId)
                        {
                            if (teamOnTower == "Alpha") nextCheckpointId--;
                            teamOnTower = "Alpha";
                        }
                        else
                        {
                            if (teamOnTower == "Beta") nextCheckpointId++;
                            teamOnTower = "Beta";
                        }

                        state = TowerState.Backing;
                        ChangeCheckpoint();
                    }
                    else
                    {
                        backCont += Time.deltaTime;
                    }
                }

                break;
            case TowerState.Moving:

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();

                    foreach (MeshRenderer mr in meshesWithTeamColor)
                        mr.material.color = SceneManagerScript.Instance.GetTeamColor(teamOnTower);
                }
                backCont = 0;

                break;
            case TowerState.Backing:

                if (!audioSource.isPlaying && audioSource.clip != backwardsAudio)
                {
                    audioSource.clip = backwardsAudio;
                    audioSource.Play();
                }

                // End Backing
                if (Vector3.Distance(transform.position, checkpoints[startCheckpointId].position) < towerToTargetDist)
                {
                    state = TowerState.Resting;
                }
                break;
        }
    }

    void ChangeCheckpoint()
    {
        if (teamOnTower == "Alpha" && checkpoints[nextCheckpointId] != checkpoints[checkpoints.Length - 1])
        {
            nextCheckpointId++;

            if (state == TowerState.Backing) return;

            state = TowerState.Moving;
            if (audioSource.clip != alphaAudio) audioSource.clip = alphaAudio;

        }
        else if (teamOnTower == "Beta" && checkpoints[nextCheckpointId] != checkpoints[0])
        {
            nextCheckpointId--;

            if (state == TowerState.Backing) return;

            state = TowerState.Moving;
            if (audioSource.clip != betaAudio) audioSource.clip = betaAudio;
        }
        else
        {
            state = TowerState.Resting;
        }
    }

    void CalcProgress()
    {
        alphaCurrentDist = 0;
        betaCurrentDist = 0;

        // Current dist for each team
        if (teamOnTower == "Alpha")
        {
            for (int i = startCheckpointId; i < nextCheckpointId; i++)
            {
                alphaCurrentDist += Vector3.Distance(checkpoints[i].position, checkpoints[i + 1].position);
            }
            alphaCurrentDist -= Vector3.Distance(transform.position, checkpoints[nextCheckpointId].position);
        }
        else
        {
            for (int i = startCheckpointId; i > nextCheckpointId; i--)
            {
                betaCurrentDist += Vector3.Distance(checkpoints[i].position, checkpoints[i - 1].position);
            }
            betaCurrentDist -= Vector3.Distance(transform.position, checkpoints[nextCheckpointId].position);
        }

        // Result
        alphaTravelDist = Mathf.Clamp01(alphaCurrentDist / alphaTotalDist);
        betaTravelDist = Mathf.Clamp01(betaCurrentDist / betaTotalDist);

        // Update Record
        if (alphaTravelDist > alphaRecord) alphaRecord = alphaTravelDist;
        if (betaTravelDist > betaRecord) betaRecord = betaTravelDist;
    }

    private void FixedUpdate()
    {
        // Move Tower
        if (state != TowerState.Resting)
        {
            if (state != TowerState.Backing)
                transform.Translate(dir * speed * Time.deltaTime);
            else
                transform.Translate(dir * backingSpeed * Time.deltaTime);
        }

        if (netNextCP.connectedToServer)
            transform.position = Vector3.LerpUnclamped(transform.position, new Vector3(netPosX.netValue, netPosY.netValue, netPosZ.netValue), interpolationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(transform);

        playersOnTower.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);

        playersOnTower.Remove(other.gameObject);
    }

    public enum TowerState
    {
        Moving,
        Resting,
        Backing
    }

    public void DrawTowerPath()
    {
        // Checkpoints Line between them
        LineRenderer lr;
        for (int i = 0; i < checkpoints.Length - 1; i++)
        {
            lr = checkpoints[i].GetComponent<LineRenderer>();

            lr.SetPosition(0, checkpoints[i].position);
            lr.SetPosition(1, checkpoints[i + 1].position);
        }

        lr = checkpoints[checkpoints.Length - 1].GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    public void RecalculatePathLength()
    {
        alphaTotalDist = betaTotalDist = 0;

        // Total dist Alpha
        for (int i = startCheckpointId; i < checkpoints.Length - 1; i++)
        {
            alphaTotalDist += Vector3.Distance(checkpoints[i].position, checkpoints[i + 1].position);
        }

        // Total dist Beta
        for (int i = startCheckpointId; i > 0; i--)
        {
            betaTotalDist += Vector3.Distance(checkpoints[i].position, checkpoints[i - 1].position);
        }
    }
}
