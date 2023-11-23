using System.Collections.Generic;
using UnityEngine;

public class Jukebox : MonoBehaviour
{
    AudioSource aSource;

    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();
    int song;

    void Start()
    {
        aSource = GetComponent<AudioSource>();
        song = Random.Range(0, audioClips.Count);
        PlaySong(song);
    }

    private void Update()
    {
        if(song >= audioClips.Count) song = 0;
    }

    void PlaySong(int num)
    {
        aSource.clip = audioClips[num];
        aSource.Play();
        song++;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlaySong(song);
    }
}
