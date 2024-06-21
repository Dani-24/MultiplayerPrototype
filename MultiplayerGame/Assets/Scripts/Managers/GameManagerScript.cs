using System;
using TMPro;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public float timerCount;

    public GameMode gameMode;
    [SerializeField] int alphaScore;
    [SerializeField] int betaScore;

    [Header("UI")]
    [SerializeField] TMP_Text timer;
    [SerializeField] TMP_Text screenMsg;
    [SerializeField] TMP_Text alphaScoreText;
    [SerializeField] TMP_Text betaScoreText;

    [Header("Match States")]
    [Tooltip("Time in sec. that each states takes")] public MatchState matchState = MatchState.waiting;
    [SerializeField] MatchStateTimes[] matchTimes;

    [Header("Net")]
    [SerializeField] NetGameObject timerNetGo;
    [SerializeField] NetGameObject matchStateNetGo;

    float hostSetColorCont = 0;

    [Header("Posibles GameModes")]
    //[SerializeField] Combatcentrik;
    public TowerObjective tower;

    #region Instance

    private static GameManagerScript _instance;
    public static GameManagerScript Instance { get { return _instance; } }

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
        alphaScoreText.text = ":D";
        screenMsg.text = "Waiting to start ...";
        betaScoreText.text = "):";

        timerCount = matchTimes[0].time;
    }

    void Update()
    {

        // HACER AQUI QUE EN EL WAITING SEA PANTALLA DE CARGA O ANIMACION DE ALGO Y ESPERE A QUE TODOS LA HAGAN

        // SE DEBERIA ASEGURAR Q SE ESPERE A TODOS LOS CLIENTS ANTES DE EMPEZAR

        switch (matchState)
        {
            case MatchState.waiting:
                CameraManager.Instance.pauseUpdate = true;
                SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().playerInputEnabled = false;
                
                if (!timerNetGo.connectedToServer && timerCount <= 0)
                {
                    matchState = MatchState.playing;
                    timerCount = matchTimes[1].time;
                }

                if (ConnectionManager.Instance.isHosting)
                {
                    if (hostSetColorCont < 0)
                    {
                        Package cPck = ConnectionManager.Instance.WritePackage();
                        cPck.connPck = new();
                        cPck.connPck.setColor = true;
                        cPck.connPck.alphaColor = SceneManagerScript.Instance.GetTeamColor("Alpha");
                        cPck.connPck.betaColor = SceneManagerScript.Instance.GetTeamColor("Beta");

                        ConnectionManager.Instance.SendPackage(cPck);
                        hostSetColorCont = 1;
                    }
                    else
                    {
                        hostSetColorCont -= Time.deltaTime;
                    }
                }

                break;
            case MatchState.playing:
                CameraManager.Instance.pauseUpdate = false;

                if (timerCount <= 10)
                {
                    int timerInt = (int)timerCount;
                    screenMsg.text = timerInt.ToString();
                }
                else screenMsg.text = "";

                if (!timerNetGo.connectedToServer)
                {
                    if (timerCount <= 0 || alphaScore >= 99 || betaScore >= 99)
                    {
                        matchState = MatchState.finish;
                        timerCount = matchTimes[2].time;
                    }
                }

                break;
            case MatchState.finish:
                CameraManager.Instance.pauseUpdate = true;
                SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().playerInputEnabled = false;
                screenMsg.text = "FINISH";

                if (!timerNetGo.connectedToServer && timerCount <= 0)
                {
                    matchState = MatchState.results;
                    timerCount = matchTimes[3].time;
                }

                break;
            case MatchState.results:
                CameraManager.Instance.pauseUpdate = true;
                SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().playerInputEnabled = false;

                if (alphaScore == betaScore)
                {
                    screenMsg.text = "Results \n Draw";
                }
                else if (alphaScore > betaScore)
                {
                    screenMsg.text = "Results \n Team Alpha WON";
                    for (int i = 0; i < SceneManagerScript.Instance.alphaTeamMembers.Count; i++)
                    {
                        for (int j = 0; j < ConnectionManager.Instance.playerPackages.Count; j++)
                        {
                            if (SceneManagerScript.Instance.alphaTeamMembers[i].GetComponent<PlayerNetworking>().networkID == ConnectionManager.Instance.playerPackages[j].netID)
                            {
                                screenMsg.text += "\n" + ConnectionManager.Instance.playerPackages[j].userName;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    screenMsg.text = "Results \n Team Beta WON";
                    for (int i = 0; i < SceneManagerScript.Instance.betaTeamMembers.Count; i++)
                    {
                        for (int j = 0; j < ConnectionManager.Instance.playerPackages.Count; j++)
                        {
                            if (SceneManagerScript.Instance.betaTeamMembers[i].GetComponent<PlayerNetworking>().networkID == ConnectionManager.Instance.playerPackages[j].netID)
                            {
                                screenMsg.text += "\n" + ConnectionManager.Instance.playerPackages[j].userName;
                                break;
                            }
                        }
                    }
                }

                if (!timerNetGo.connectedToServer && timerCount <= 0) SceneManagerScript.Instance.ChangeScene("000_Lobby", true);

                break;
        }

        switch (gameMode)
        {
            case GameMode.combatcentrik:
                break;
            case GameMode.towah:
                alphaScore = (int)(tower.alphaRecord * 100);
                betaScore = (int)(tower.betaRecord * 100);
                break;
        }

        if (!timerNetGo.connectedToServer)
        {
            timerNetGo.netValue = timerCount;
            matchStateNetGo.netValue = Convert.ToInt32(matchState);

            timerCount -= Time.deltaTime;
        }
        else
        {
            matchState = (MatchState)Enum.Parse(typeof(MatchState), matchStateNetGo.netValue.ToString());
            timerCount = timerNetGo.netValue;
        }

        TimeSpan time = TimeSpan.FromSeconds(timerCount);
        timer.text = time.ToString("mm':'ss");

        if(matchState != MatchState.waiting)
        {
            alphaScoreText.text = "Alpha Score: " + alphaScore;
            betaScoreText.text = "Beta Score: " + betaScore;
        }
    }

    public enum MatchState
    {
        waiting,
        playing,
        finish,
        results
    }

    public enum GameMode
    {
        combatcentrik,
        towah
    }

    [System.Serializable]
    public class MatchStateTimes
    {
        public MatchStateTimes(MatchState _state, float _time)
        {
            state = _state;
            time = _time;
        }

        public MatchState state;
        public float time;
    }
}
