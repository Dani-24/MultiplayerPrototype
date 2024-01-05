using System;
using System.Collections.Generic;
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

    [Header("Debug")]
    [SerializeField] float towerToTargetDist = 0.1f;
    [SerializeField] float backCont = 0f;

    void Start()
    {
        // Start ID
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == startCheckpoint)
            {
                nextCheckpointId = startCheckpointId = i;
                break;
            }
        }

        //// Checkpoints Line between them
        //for (int i = 0; i < checkpoints.Length; i++)
        //{
        //    LineRenderer lr = checkpoints[i].GetComponent<LineRenderer>();

        //    lr.SetPosition(0, transform.position);
        //}

        // Total dist Alpha
        for (int i = nextCheckpointId; i < checkpoints.Length - 1; i++)
        {
            alphaTotalDist += Vector3.Distance(checkpoints[i].position, checkpoints[i + 1].position);
        }

        // Total dist Beta
        for (int i = nextCheckpointId; i > 0; i--)
        {
            betaTotalDist += Vector3.Distance(checkpoints[i].position, checkpoints[i - 1].position);
        }
    }

    void Update()
    {
        #region Check Players on tower
        if (playersOnTower.Count <= 0)
        {
            if (state != TowerState.Backing)
            {
                state = TowerState.Resting;
            }
        }
        else
        {
            int alphas = 0;
            for (int i = 0; i < playersOnTower.Count; i++)
            {
                if (playersOnTower[i].GetComponent<PlayerStats>().teamTag == "Alpha") alphas++; else alphas--;
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

        for (var i = 0; i < checkpoints.Length - 1; i++)
        {
            Debug.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
        }
    }

    void TowerStates()
    {
        switch (state)
        {
            case TowerState.Resting:

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
                        if (teamOnTower == "Alpha") teamOnTower = "Beta"; else teamOnTower = "Alpha";
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
                break;
            case TowerState.Backing:

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

            if (state != TowerState.Backing) state = TowerState.Moving;

        }
        else if (teamOnTower == "Beta" && checkpoints[nextCheckpointId] != checkpoints[0])
        {
            nextCheckpointId--;
            if (state != TowerState.Backing) state = TowerState.Moving;
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
        for (int i = startCheckpointId; i < nextCheckpointId; i++)
        {
            alphaCurrentDist += Vector3.Distance(checkpoints[i].position, checkpoints[i + 1].position);
        }
        alphaCurrentDist -= Vector3.Distance(transform.position, checkpoints[nextCheckpointId].position);

        for (int i = startCheckpointId; i > nextCheckpointId; i--)
        {
            betaCurrentDist += Vector3.Distance(checkpoints[i].position, checkpoints[i - 1].position);
        }
        betaCurrentDist -= Vector3.Distance(transform.position, checkpoints[nextCheckpointId].position);

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
            transform.Translate(dir * speed * Time.deltaTime);
        }
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
}
