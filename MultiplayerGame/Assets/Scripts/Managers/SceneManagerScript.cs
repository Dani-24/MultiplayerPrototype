using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagerScript : MonoBehaviour
{
    public string sceneName;
    public GameState gameState;

    [Header("Scene Transitioning")]
    [SerializeField] string sceneToLoad;
    [SerializeField] bool loadScene = false;
    [SerializeField] float timeToLoad = 5.0f;
    bool isAlreadyChangingScene = false;

    [SerializeField] Animator transitionAnimator;
    [SerializeField] float transitionTime;
    [SerializeField] Image[] transitionsRects;

    [SerializeField] Color loadedRuntimeColor = Color.white;
    [SerializeField] bool smtgLoaded = false;

    [Header("Debug")]
    public bool showConsole = false;
    [SerializeField] GameObject debugConsole;

    [SerializeField] bool deleteAllNotOwnPlayers = false;
    [SerializeField] float InkColorThreshold = 5;

    [Header("Save Data")]
    [SerializeField] bool deleteSavedDataOnStart = false;
    [SerializeField] bool saveOnExit = false;

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
    [SerializeField] int targetFPS = 60;

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

    [SerializeField] AudioMixer audioMixer;

    #region Instance

    private static SceneManagerScript _instance;
    public static SceneManagerScript Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && Instance != null)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    #endregion

    void Start()
    {
        Application.targetFrameRate = targetFPS;

        LoadRuntimeData();

        if (loadScene)
        {
            transitionsRects[0].color = transitionsRects[1].color = transitionsRects[2].color = Color.black;
            return;
        }

        if (colorPairs.Count > 0 && !useTheseDebugColors)
        {
            int rand = Random.Range(0, colorPairs.Count);

            alphaTeamColor = colorPairs[rand].color1;
            betaTeamColor = colorPairs[rand].color2;

            transitionsRects[0].color = alphaTeamColor;
            transitionsRects[2].color = gammaTeamColor;

            if (smtgLoaded)
                transitionsRects[1].color = loadedRuntimeColor;
            else
                transitionsRects[1].color = betaTeamColor;
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
                        playerGOAtScene.GetComponent<PlayerMovement>().SetFacing(alphaFaceAngle);
                    else
                        playerGOAtScene.GetComponent<PlayerMovement>().SetFacing(betaFaceAngle);
                    break;
                }
            }
        }

        if (loadScene)
        {
            if (timeToLoad > 0)
                timeToLoad -= Time.deltaTime;
            else
            {
                loadScene = false;
                ChangeSceneAsync(sceneToLoad);
            }
            return;
        }

        if (debugConsole != null)
            debugConsole.SetActive(showConsole);

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
            cleanPaint = false;
        }

        if (deleteSavedDataOnStart)
        {
            deleteSavedDataOnStart = false;
            DeleteSavedData();
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
            return alphaTeamColor;
        else if (tag == teamTags[1])
            return betaTeamColor;
        else
            return gammaTeamColor;
    }

    public string GetTeamFromColor(Color color)
    {
        if (ColorComparer(color, alphaTeamColor) <= InkColorThreshold)
            return teamTags[0];
        else if (ColorComparer(color, betaTeamColor) <= InkColorThreshold)
            return teamTags[1];
        else
            return teamTags[2];
    }

    float ColorComparer(Color a, Color b)
    {
        float deltaR = Mathf.Abs(a.r - b.r);
        float deltaG = Mathf.Abs(a.g - b.g);
        float deltaB = Mathf.Abs(a.b - b.b);

        float totalDifference = (deltaR + deltaG + deltaB) / 3f;

        //Debug.Log("<color=green>Colorin colorado</color> " + totalDifference * 100f);

        return totalDifference * 100f;
    }

    public void SetColors(Color _alphaTeam, Color _betaTeam)
    {
        alphaTeamColor = _alphaTeam;
        betaTeamColor = _betaTeam;
    }

    public string GetRivalTag(string tag)
    {
        if (tag == teamTags[0])
            return teamTags[1];
        else if (tag == teamTags[1])
            return teamTags[0];
        else
            return teamTags[2];
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
            alphaTeamMembers.Remove(go);
        else if (go.tag == teamTags[1])
            betaTeamMembers.Remove(go);
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

    public void ChangeSceneAsync(string sceneToChange)
    {
        SceneManager.LoadSceneAsync(sceneToChange);
    }

    public void ChangeScene(string sceneToChange, bool preserveTag = false)
    {
        if (isAlreadyChangingScene) return;

        isAlreadyChangingScene = true;

        UI_Manager.Instance.CloseAll();

        if (preserveTag) ConnectionManager.Instance.ownTeamTagOnSceneChange = ConnectionManager.Instance.ownPlayerPck.teamTag;

        SaveData();
        SaveRuntimeData();

        transitionsRects[0].color = alphaTeamColor;
        transitionsRects[1].color = betaTeamColor;
        transitionsRects[2].color = gammaTeamColor;

        StartCoroutine(LoadScene(sceneToChange));
    }

    IEnumerator LoadScene(string sceneToChange)
    {
        transitionAnimator.SetTrigger("start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneToChange);

        LoadData();

        isAlreadyChangingScene = false;
    }

    #endregion

    #region Saving

    public void SaveData()
    {
        SaveData data = new();

        // Asignar cosas a data
        data.name = ConnectionManager.Instance.userName;

        // Equipment
        data.mainW = GetOwnPlayerInstance().GetComponent<PlayerArmament>().currentWeaponId;
        data.secW = GetOwnPlayerInstance().GetComponent<PlayerArmament>().currentSubWeaponId;

        // Sens
        data.mouseSens[0] = GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().mouseSens.x;
        data.mouseSens[1] = GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().mouseSens.y;

        data.padSens[0] = GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().gamepadSens.x;
        data.padSens[1] = GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().gamepadSens.y;

        // Graphics
        data.quality = QualitySettings.GetQualityLevel();
        //data.windowMode;
        //data.resolution;

        // Audio
        audioMixer.GetFloat("masterV", out float vol);
        data.masterV = vol;

        audioMixer.GetFloat("musicV", out vol);
        data.musicV = vol;

        audioMixer.GetFloat("sfxV", out vol);
        data.sfxV = vol;

        // Save
        SaveManagerScript.SaveGame(data);
    }

    public void LoadData()
    {
        // Load
        SaveData data = SaveManagerScript.LoadGame();

        if (data == null)
            return;

        // Process Data
        if (gameState == GameState.Title)
            UI_Manager.Instance.userName = data.name;

        // Aquí lo de Save pero al reves

        GetOwnPlayerInstance().GetComponent<PlayerArmament>().ChangeWeapon(data.mainW);
        GetOwnPlayerInstance().GetComponent<PlayerArmament>().ChangeSubWeapon(data.secW);

        // Sens
        GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().mouseSens.x = data.mouseSens[0];
        GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().mouseSens.y = data.mouseSens[1];

        GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().gamepadSens.x = data.padSens[0];
        GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().gamepadSens.y = data.padSens[1];

        // Graphics
        QualitySettings.SetQualityLevel(data.quality);
        //data.windowMode;
        //data.resolution;

        float vol = data.masterV;
        audioMixer.SetFloat("masterV", vol);

        vol = data.musicV;
        audioMixer.SetFloat("musicV", vol);

        vol = data.sfxV;
        audioMixer.SetFloat("sfxV", vol);
    }

    public void DeleteSavedData()
    {
        SaveManagerScript.DeleteSavedGame();
    }

    public void SaveRuntimeData()
    {
        RuntimeData data = new();
        data.savedColor[0] = betaTeamColor.r;
        data.savedColor[1] = betaTeamColor.g;
        data.savedColor[2] = betaTeamColor.b;

        SaveManagerScript.SaveRuntimeData(data);
    }

    public void LoadRuntimeData()
    {
        RuntimeData data = SaveManagerScript.LoadRuntimeData();

        if (data != null)
        {
            loadedRuntimeColor.r = data.savedColor[0];
            loadedRuntimeColor.g = data.savedColor[1];
            loadedRuntimeColor.b = data.savedColor[2];

            smtgLoaded = true;
        }
        else
            smtgLoaded = false;
    }

    private void OnApplicationQuit()
    {
        if (saveOnExit) SaveData();

        SaveManagerScript.DeleteRuntimeData();
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
