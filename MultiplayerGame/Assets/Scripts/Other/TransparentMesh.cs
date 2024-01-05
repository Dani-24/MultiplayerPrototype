using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentMesh : MonoBehaviour
{
    [Header("This Script allows transparency when the this mesh is between the player and the camera")]
    public bool okThanks;

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        
    }
}
