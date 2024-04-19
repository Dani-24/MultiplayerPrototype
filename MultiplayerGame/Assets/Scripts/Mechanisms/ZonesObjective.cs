using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;
using UnityEngine.Rendering;

public class ZonesObjective : MonoBehaviour
{
    [SerializeField] float tickSpeed = 3f;
    [SerializeField] int penalty = 3 / 4;
    [SerializeField] ZoneStates zoneState = ZoneStates.Idle;

    [SerializeField] int alphaCounter = 100;
    [SerializeField] int betaCounter = 100;

    [SerializeField] int alphaPenalty = 0;
    [SerializeField] int betaPenalty = 0;

    [SerializeField] int alphaPushCounter = 0;
    [SerializeField] int betaPushCounter = 0;

    [SerializeField] string teamInControl;

    [SerializeField][Tooltip("Percent needed by the losing team to stop the count")] float pToPauseTheZone = 0.5f;
    [SerializeField][Tooltip("Percent needed by the losing team to get the control")] float pToCapTheZone = 0.85f;
    [SerializeField] float alphaControl;
    [SerializeField] float betaControl;

    [Header("Ground Paint")]
    [SerializeField][Tooltip("Color is checked from 0 to 1 by this distance")] float groundCheckQuality = 0.1f;
    [SerializeField] Color groundColor;
    RenderTexture maskT;
    [SerializeField] Texture2D texture;

    void Start()
    {
        texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
    }

    void Update()
    {
        CheckZonePaint();

        switch (zoneState)
        {
            case ZoneStates.Idle:   // Decide the first team to get the zone

                if (alphaControl >= pToCapTheZone)
                {
                    teamInControl = SceneManagerScript.Instance.teamTags[0];
                    zoneState = ZoneStates.Scoring;
                }
                else if (betaControl >= pToCapTheZone)
                {
                    teamInControl = SceneManagerScript.Instance.teamTags[1];
                    zoneState = ZoneStates.Scoring;
                }

                break;
            case ZoneStates.Contested:  // Fight for the zone control

                if (alphaControl >= pToCapTheZone && teamInControl != SceneManagerScript.Instance.teamTags[0])
                {
                    zoneState = ZoneStates.Scoring;

                    betaPenalty += betaPushCounter * penalty;

                    teamInControl = SceneManagerScript.Instance.teamTags[0];
                }
                else if (alphaControl >= pToCapTheZone && teamInControl == SceneManagerScript.Instance.teamTags[0])
                    zoneState = ZoneStates.Scoring;

                if (betaControl >= pToCapTheZone && teamInControl != SceneManagerScript.Instance.teamTags[1])
                {
                    zoneState = ZoneStates.Scoring;

                    alphaPenalty += alphaPushCounter * penalty;

                    teamInControl = SceneManagerScript.Instance.teamTags[1];
                }
                else if (betaControl >= pToCapTheZone && teamInControl == SceneManagerScript.Instance.teamTags[1])
                    zoneState = ZoneStates.Scoring;

                break;
            case ZoneStates.Scoring:    // Scoring Points

                if (alphaControl > pToPauseTheZone && teamInControl != SceneManagerScript.Instance.teamTags[0] ||
                    betaControl > pToPauseTheZone && teamInControl != SceneManagerScript.Instance.teamTags[1])
                    zoneState = ZoneStates.Contested;

                float score = Time.deltaTime * tickSpeed;

                if (teamInControl == SceneManagerScript.Instance.teamTags[0])
                {
                    if (alphaPenalty > 0)
                        alphaPenalty -= (int)score;
                    else
                    {
                        alphaCounter -= (int)score;
                        alphaPushCounter += (int)score;
                    }
                }
                else
                {
                    if (betaPenalty > 0)
                        betaPenalty -= (int)score;
                    else
                    {
                        betaCounter -= (int)score;
                        betaPushCounter += (int)score;
                    }
                }

                break;
        }

    }

    void CheckZonePaint()   // Pendiente de revisar. Lo suyo seria reducir la textura a muy pocos pixeles y sacar de ahi el color
    {
        //int alpha = 0;
        //int beta = 0;
        //int none = 0;

        //for (float i = 0; i < 1; i += groundCheckQuality)
        //{
        //    for (float j = 0; j < 1; j += groundCheckQuality)
        //    {
        //        Paintable paintComponent = GetComponent<Paintable>();

        //        maskT = (RenderTexture)paintComponent.getRenderer().material.GetTexture(paintComponent.maskTextureID);
        //        RenderTexture.active = maskT;
        //        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        //        texture.Apply();

        //        groundColor = texture.GetPixelBilinear(i, j);

        //        string color = SceneManagerScript.Instance.GetTeamFromColor(groundColor);

        //        if (color == SceneManagerScript.Instance.teamTags[0])
        //            alpha++;
        //        else if (color == SceneManagerScript.Instance.teamTags[1])
        //            beta++;
        //        else
        //            none++;
        //    }
        //}

        //int total = alpha + beta + none;

        //alphaControl = alpha / total;
        //betaControl = beta / total;

        Paintable paintComponent = GetComponent<Paintable>();

        maskT = (RenderTexture)paintComponent.getRenderer().material.GetTexture(paintComponent.maskTextureID);
        RenderTexture.active = maskT;
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture.Apply();

        texture.Reinitialize(2,2);

        groundColor = AverageColorFromTexture(texture);

    }

    Color32 AverageColorFromTexture(Texture2D tex)
    {
        Color32[] texColors = tex.GetPixels32();

        int total = texColors.Length;

        float r = 0;
        float g = 0;
        float b = 0;

        for (int i = 0; i < total; i++)
        {
            r += texColors[i].r;
            g += texColors[i].g;
            b += texColors[i].b;
        }

        return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 0);
    }
}

[System.Serializable]
public enum ZoneStates
{
    Idle,
    Contested,
    Scoring
}