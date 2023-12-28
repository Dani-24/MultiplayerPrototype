using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] bool changeWaterLevel = false;
    bool done = false;

    [SerializeField] GameObject TerrainAlpha;
    [SerializeField] GameObject TerrainBeta;
    [SerializeField] GameObject Water;

    private void Update()
    {
        if (changeWaterLevel && !done || Input.GetKeyDown(KeyCode.Alpha0)) ChangeWaterLevel();
    }

    public void ChangeWaterLevel()
    {
        if (TerrainAlpha != null) TerrainAlpha.BroadcastMessage("ChangeWaterLevel");
        if (TerrainBeta != null) TerrainBeta.BroadcastMessage("ChangeWaterLevel");
        if (Water != null) Water.BroadcastMessage("ChangeWaterLevel");
        done = true;
    }
}
