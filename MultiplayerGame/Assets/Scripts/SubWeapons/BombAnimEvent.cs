using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BombAnimEvent : MonoBehaviour
{
    [SerializeField] UnityEvent functionToUse;

    public void OnAnimEnd()
    {
        functionToUse.Invoke();
    }
}
