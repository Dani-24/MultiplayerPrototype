using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public string sceneName;
    public GameState gameState;

    [Header("Scene Transitioning")]
    [SerializeField] string sceneToLoad;
    [SerializeField] bool loadScene = false;
    [SerializeField] float timeToLoad = 5.0f;

    [Header("Debug")]
    public bool showConsole = false;
    [SerializeField] GameObject debugConsole;

    [SerializeField] bool deleteAllNotOwnPlayers = false;

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
    [SerializeField] GameObject playerGOAtScene;
    public List<GameObject> playersOnScene = new List<GameObject>();

    [Header("Teams")]
    public List<string> teamTags = new List<string>();

    public List<GameObject> alphaTeamMembers = new List<GameObject>();
    public List<GameObject> betaTeamMembers = new List<GameObject>();
    public List<GameObject> gammaTeamMembers = new List<GameObject>();

    public int maxPlayersPerTeam = 4;

    #endregion

    [Header("For net")]
    public bool cleanPaint = false;
    [SerializeField] GameObject sceneRoot;

    [SerializeField] bool useSpawnPoints = false;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float alphaFaceAngle = 0;
    [SerializeField] float betaFaceAngle = 180;

    [Header("Current Scene Online GameObjects")]
    public List<NetGameObject> netGOs;

    [Header("Weapons Available")]
    public GameObject[] mainWeapons;
    public GameObject[] subWeapons;

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
        if (loadScene) return;

        if (colorPairs.Count > 0 && !useTheseDebugColors)
        {
            int rand = Random.Range(0, colorPairs.Count);

            alphaTeamColor = colorPairs[rand].color1;
            betaTeamColor = colorPairs[rand].color2;
        }

        // Add Base Player to players List
        if (playerGOAtScene != null)
        {
            if (ConnectionManager.Instance.IsConnected() && ConnectionManager.Instance.ownPlayerNetID != -1) // When changing scene while connected
            {
                playerGOAtScene.GetComponent<PlayerNetworking>().networkID = ConnectionManager.Instance.ownPlayerNetID;
                playerGOAtScene.GetComponent<PlayerStats>().ChangeTag(ConnectionManager.Instance.ownTeamTagOnSceneChange);
                playerGOAtScene.GetComponent<PlayerArmament>().ChangeWeapon(ConnectionManager.Instance.ownPlayerPck.mainWeapon);
                playerGOAtScene.GetComponent<PlayerArmament>().ChangeSubWeapon(ConnectionManager.Instance.ownPlayerPck.subWeapon);
                UI_Manager.Instance.gameplayMenuCreated = false;
            }
            playersOnScene.Add(playerGOAtScene);
        }

        int num = 0;
        foreach (NetGameObject n in netGOs)
        {
            n.GOid = num;
            num++;
        }
    }

    void Update()
    {
        // Set your spawn position
        if (useSpawnPoints)
        {
            useSpawnPoints = false;
            for (int i = 0; i < ConnectionManager.Instance.playerPackages.Count; i++)
            {
                if (ConnectionManager.Instance.playerPackages[i].netID == playerGOAtScene.GetComponent<PlayerNetworking>().networkID)
                {
                    playerGOAtScene.GetComponent<PlayerStats>().spawnPos = spawnPoints[i].position;
                    playerGOAtScene.GetComponent<PlayerMovement>().TeleportToSpawnPos();

                    if (playerGOAtScene.GetComponent<PlayerStats>().teamTag == "Alpha")
                    {
                        playerGOAtScene.GetComponent<PlayerMovement>().SetFacing(alphaFaceAngle);
                    }
                    else
                    {
                        playerGOAtScene.GetComponent<PlayerMovement>().SetFacing(betaFaceAngle);
                    }
                    break;
                }
            }
        }

        if (loadScene)
        {
            if (timeToLoad > 0)
            {
                timeToLoad -= Time.deltaTime;
            }
            else
            {
                loadScene = false;
                ChangeSceneAsync(sceneToLoad);
            }
            return;
        }

        if (debugConsole != null)
        {
            debugConsole.SetActive(showConsole);
        }

        // DEBUG
        if (deleteAllNotOwnPlayers)
        {
            DeleteAllNotOwnedPlayers();
            deleteAllNotOwnPlayers = false;
        }

        sceneName = SceneManager.GetActiveScene().name;

        if (cleanPaint)
        {
            sceneRoot.BroadcastMessage("CleanPaint");
            sceneRoot.BroadcastMessage("CleanPaint");
            sceneRoot.BroadcastMessage("CleanPaint");
            cleanPaint = false;
        }
    }

    #region Players Management

    public GameObject CreateNewPlayer(bool own, Vector3 _position)
    {
        GameObject newP = Instantiate(playerGOAtScene, _position, transform.rotation);

        newP.GetComponent<PlayerNetworking>().isOwnByThisInstance = own;

        playersOnScene.Add(newP);

        Debug.Log("Created new player at: " + _position);

        return newP;
    }

    public void DeletePlayer(GameObject player)
    {
        Debug.Log("Deleting Player " + player.name);

        DeleteFromTeam(player);
        playersOnScene.Remove(player);
        Destroy(player);
    }

    public void DeleteAllNotOwnedPlayers()
    {
        for (int i = 0; i < playersOnScene.Count; i++)
        {
            if (!playersOnScene[i].GetComponent<PlayerNetworking>().isOwnByThisInstance)
            {
                DeletePlayer(playersOnScene[i]);
                break;
            }
        }
        if (playersOnScene.Count > 1)
        {
            DeleteAllNotOwnedPlayers();
        }
    }

    public GameObject GetOwnPlayerInstance()
    {
        return playerGOAtScene;
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

    public void SetColors(Color _alphaTeam, Color _betaTeam)
    {
        alphaTeamColor = _alphaTeam;
        betaTeamColor = _betaTeam;
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

    public void TeamsForBattle()
    {
        alphaTeamMembers.Clear();
        betaTeamMembers.Clear();

        for (int i = 0; i < playersOnScene.Count; i++)
        {
            SetTeam(playersOnScene[i]);
            for (var j = 0; j < ConnectionManager.Instance.playerPackages.Count; j++)
            {
                if (playersOnScene[i].GetComponent<PlayerNetworking>().networkID == ConnectionManager.Instance.playerPackages[j].netID)
                {
                    ConnectionManager.Instance.playerPackages[i].teamTag = playersOnScene[i].GetComponent<PlayerStats>().teamTag;
                    return;
                }
            }
        }
    }

    #endregion

    #region Scene Manager

    public void ChangeScene(string sceneToChange)
    {
        UI_Manager.Instance.CloseAll();
        SceneManager.LoadScene(sceneToChange);
    }

    public void ChangeSceneAsync(string sceneToChange)
    {
        SceneManager.LoadSceneAsync(sceneToChange);
    }

    public void ChangeSceneConnected(string sceneToChange)
    {
        UI_Manager.Instance.CloseAll();
        ConnectionManager.Instance.ownTeamTagOnSceneChange = ConnectionManager.Instance.ownPlayerPck.teamTag;
        SceneManager.LoadScene(sceneToChange);
    }

    #endregion

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

    public enum GameState
    {
        Title,
        Gameplay,
        Settings,
        Loading
    }
}
