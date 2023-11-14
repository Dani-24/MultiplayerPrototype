using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public string sceneName;

    [Header("Color combinations")]
    [SerializeField] public List<ColorPair> colorPairs = new List<ColorPair>();

    [Header("This Game Colors")]
    [SerializeField] Color alphaTeamColor;
    [SerializeField] Color betaTeamColor;
    [Tooltip("This team is for tag errors")][SerializeField] Color gammaTeamColor;

    [SerializeField] bool useTheseDebugColors = false;

    public List<string> teamTags = new List<string>();

    public List<GameObject> alphaTeamMembers = new List<GameObject>();
    public List<GameObject> betaTeamMembers = new List<GameObject>();

    public int maxPlayersPerTeam = 4;

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

    void Start()
    {
        if (colorPairs.Count > 0 && !useTheseDebugColors)
        {
            int rand = Random.Range(0, colorPairs.Count);

            alphaTeamColor = colorPairs[rand].color1;
            betaTeamColor = colorPairs[rand].color2;
        }
    }

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
            Debug.Log("Error getting tag");
            return gammaTeamColor;
        }
    }

    public string GetRivalTag(string tag)
    {
        if(tag == teamTags[0])
        {
            return teamTags[1];
        }
        else if(tag == teamTags[1])
        {
            return teamTags[0];
        }
        else
        {
            Debug.Log("Error getting rival tag");
            return "";
        }
    }

    [Tooltip("Randomly assigns a team if preference != teamtags")]
    public string SetTeam(GameObject go, string preference = "none")
    {
        bool teamAssigned = false;

        if(preference == teamTags[0])
        {
            go.tag = teamTags[0];
            alphaTeamMembers.Add(go);
            teamAssigned = true;
        }
        else if(preference == teamTags[1])
        {
            go.tag = teamTags[0];
            betaTeamMembers.Add(go);
            teamAssigned = true;
        }

        while (!teamAssigned)
        {
            int rng = Random.Range(0, 2);

            if(rng == 0)
            {
                if(alphaTeamMembers.Count < 4) { 
                    go.tag = teamTags[0];
                    alphaTeamMembers.Add(go); 
                    teamAssigned = true;
                    return go.tag;
                }
            }
            else
            {
                if (betaTeamMembers.Count < 4)
                {
                    go.tag = teamTags[1];
                    betaTeamMembers.Add(go); 
                    teamAssigned = true;
                    return go.tag;
                }
            }
        }

        Debug.Log("Error by Assigning team tag to gameobject");
        return teamTags[2];
    }

    public void DeleteFromTeam(GameObject go)
    {
        if(go.tag == teamTags[0])
        {
            alphaTeamMembers.Remove(go);
        }
        else if (go.tag == teamTags[1])
        {
            betaTeamMembers.Remove(go);
        }
    }

    #endregion

    public void ChangeScene(string sceneToChange)
    {
        SceneManager.LoadScene(sceneToChange);
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
