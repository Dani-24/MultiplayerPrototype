using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetClientName : MonoBehaviour
{
    TMP_InputField nameField;
    [SerializeField] ClientSockets clientScript;

    [SerializeField] List<GameObject> goToEnable;

    private void Start()
    {
        nameField = GetComponent<TMP_InputField>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrWhiteSpace(nameField.text))
        {
            clientScript.nickname = nameField.text;

            for(int i = 0; i < goToEnable.Count; i++)
            {
                goToEnable[i].SetActive(true);
            }
            this.gameObject.SetActive(false);
        }
    }
}
