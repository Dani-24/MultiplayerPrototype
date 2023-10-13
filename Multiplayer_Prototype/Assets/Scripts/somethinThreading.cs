using UnityEngine;
using System.Threading;
using System.Net;

public class somethinThreading : MonoBehaviour
{
    [SerializeField]
    string url;

    [SerializeField]
    string fileName;

    private WebClient webClient;

    public void PressButton()
    {
        Thread testThread = new Thread(DoSmtgWithThread);
        testThread.Start();
    }

    void DoSmtgWithThread()
    {
        Debug.Log("Downloading " + url + " as " + fileName);
        webClient = new WebClient();
        webClient.DownloadFile(url,fileName);
    }
}
