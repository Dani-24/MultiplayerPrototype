using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [Header("Color combinations")]
    [SerializeField] public List<ColorPair> colorPairs = new List<ColorPair>();

    [Header("This Game Colors")]
    public Color allyColor;
    public Color enemyColor;

    [SerializeField] bool useTheseDebugColors = false;

    void Start()
    {
        if (colorPairs.Count > 0 && !useTheseDebugColors)
        {
            int rand = Random.Range(0, colorPairs.Count);

            allyColor = colorPairs[rand].color1;
            enemyColor = colorPairs[rand].color2;
        }
    }
    void Update()
    {
        
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
