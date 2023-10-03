using UnityEngine;
using System.Threading;
using System.Net;

public class somethinThreading : MonoBehaviour
{
    [SerializeField]
    string url;

    [SerializeField]
    string fileName;

    public void PressButton()
    {
        Thread testThread = new Thread(DoSmtgWithThread);
        testThread.Start();
    }

    void DoSmtgWithThread()
    {
        var webClient = new WebClient();
        webClient.DownloadFile(url,fileName);
    }
}
