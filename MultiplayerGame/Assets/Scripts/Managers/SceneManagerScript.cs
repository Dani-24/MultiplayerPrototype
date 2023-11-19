using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public string sceneName;

    [Header("Debug")]
    public bool showConsole = false;
    [SerializeField] GameObject debugConsole;

    [SerializeField] bool addNewOwnPlayer = false;
    [SerializeField] float rngSpawnDist = 15f;

    #region Colors Propierties

    [Header("Color combinations")]
    [SerializeField] public List<ColorPair> colorPairs = new List<ColorPair>();

    [Header("Game Colors")]
    [SerializeField] Color alphaTeamColor;
    [SerializeField] Color betaTeamColor;
    [Tooltip("This team is for tag errors")][SerializeField] Color gammaTeamColor;

    [SerializeField] bool useTheseDebugColors = false;

    #endregion

    #region Players & Teams

    [Header("Players")]
    [SerializeField] GameObject playerPrefab;
    public List<GameObject> playersOnScene = new List<GameObject>();

    [Header("Teams")]
    public List<string> teamTags = new List<string>();

    public List<GameObject> alphaTeamMembers = new List<GameObject>();
    public List<GameObject> betaTeamMembers = new List<GameObject>();
    public List<GameObject> gammaTeamMembers = new List<GameObject>();

    public int maxPlayersPerTeam = 4;

    #endregion

    #region Instance

    private static SceneManagerScript _instance;
    public static SceneManagerScript Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    void Start()
    {
        if (colorPairs.Count > 0 && !useTheseDebugColors)
        {
            int rand = Random.Range(0, colorPairs.Count);

            alphaTeamColor = colorPairs[rand].color1;
            betaTeamColor = colorPairs[rand].color2;
        }
    }

    void Update()
    {
        if (debugConsole != null)
        {
            debugConsole.SetActive(showConsole);
        }

        // DEBUG
        if (addNewOwnPlayer)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.x += Random.Range(-rngSpawnDist, rngSpawnDist);
            spawnPos.z += Random.Range(-rngSpawnDist, rngSpawnDist);

            CreateNewPlayer(true, spawnPos);
            addNewOwnPlayer = false;
        }
    }

    #region Players Management

    public GameObject CreateNewPlayer(bool own, Vector3 asignedPos)
    {
        GameObject newP = Instantiate(playerPrefab, asignedPos, transform.rotation);

        newP.GetComponent<PlayerNetworking>().isOwnByThisInstance = own;

        playersOnScene.Add(newP);
        return newP;
    }

    public void DeletePlayer(GameObject player)
    {
        DeleteFromTeam(player);
        playersOnScene.Remove(player);
        Destroy(player);
    }

    public GameObject GetOwnPlayerInstance()
    {
        for(int i = 0; i < playersOnScene.Count; i++)
        {
            if (playersOnScene[i].GetComponent<PlayerNetworking>().isOwnByThisInstance)
            {
                return playersOnScene[i];
            }
        }

        Debug.Log("There is no Player Own by this Instance");
        return null;
    }

    #endregion

    #region Teams Management

    public Color GetTeamColor(string tag)
    {
        if (tag == teamTags[0])
        {
            return alphaTeamColor;
        }
        else if (tag == teamTags[1])
        {
            return betaTeamColor;
        }
        else
        {
            return gammaTeamColor;
        }
    }

    public string GetRivalTag(string tag)
    {
        if (tag == teamTags[0])
        {
            return teamTags[1];
        }
        else if (tag == teamTags[1])
        {
            return teamTags[0];
        }
        else
        {
            return teamTags[2];
        }
    }

    [Tooltip("Randomly assigns a team if preference != teamtags")]
    public string SetTeam(GameObject go, string preference = "none")
    {
        DeleteFromTeam(go);
        bool teamAssigned = false;

        #region Select with Preference

        if (preference == teamTags[0])
        {
            teamAssigned = TeamAssigner(go, alphaTeamMembers, 0);
            return go.tag;
        }
        else if (preference == teamTags[1])
        {
            teamAssigned = TeamAssigner(go, betaTeamMembers, 1);
            return go.tag;
        }

        #endregion

        while (!teamAssigned)
        {
            // If Alpha && Beta is full -> Team = Gamma
            if (alphaTeamMembers.Count >= maxPlayersPerTeam && betaTeamMembers.Count >= maxPlayersPerTeam)
            {
                teamAssigned = TeamAssigner(go, gammaTeamMembers, 2);
                return teamTags[2];
            }

            // Assign automatically to the team with less players
            if (alphaTeamMembers.Count > betaTeamMembers.Count && betaTeamMembers.Count < maxPlayersPerTeam)
            {
                teamAssigned = TeamAssigner(go, betaTeamMembers, 1);
                return go.tag;
            }
            else if (alphaTeamMembers.Count < betaTeamMembers.Count && alphaTeamMembers.Count < maxPlayersPerTeam)
            {
                teamAssigned = TeamAssigner(go, alphaTeamMembers, 0);
                return go.tag;
            }

            // Assign Team Randomly if they have the same number of players
            int rng = Random.Range(0, 2);

            if (rng == 0)
            {
                if (alphaTeamMembers.Count < maxPlayersPerTeam)
                {
                    teamAssigned = TeamAssigner(go, alphaTeamMembers, 0);
                    return go.tag;
                }
            }
            else
            {
                if (betaTeamMembers.Count < maxPlayersPerTeam)
                {
                    teamAssigned = TeamAssigner(go, betaTeamMembers, 1);
                    return go.tag;
                }
            }
        }

        Debug.Log("Error by Assigning team tag to gameobject");
        return "";
    }

    // Team Num are 0 for Alpha & 1 for Beta
    bool TeamAssigner(GameObject go, List<GameObject> teamList, int teamNum)
    {
        go.tag = teamTags[teamNum];
        teamList.Add(go);
        return true;
    }

    public void DeleteFromTeam(GameObject go)
    {
        if (go.tag == teamTags[0])
        {
            alphaTeamMembers.Remove(go);
        }
        else if (go.tag == teamTags[1])
        {
            betaTeamMembers.Remove(go);
        }
    }

    #endregion

    #region Scene Manager

    public void ChangeScene(string sceneToChange)
    {
        SceneManager.LoadScene(sceneToChange);
    }

    public void ChangeSceneAsync(string sceneToChange)
    {
        SceneManager.LoadSceneAsync(sceneToChange);
    }

    #endregion

    public void ShowUI(bool show)
    {
        GetComponent<UI_Manager>().showUI = show;
    }

    [System.Serializable]
    public struct ColorPair
    {
        public Color color1;
        public Color color2;

        public ColorPair(Color c1, Color c2)
        {
            color1 = c1;
            color2 = c2;
        }
    }
}
