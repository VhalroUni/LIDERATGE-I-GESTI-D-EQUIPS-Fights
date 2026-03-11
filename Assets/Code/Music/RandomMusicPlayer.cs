using System.Collections.Generic;
using UnityEngine;

public class RandomMusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;          // AudioSource que reproducirá la música
    public List<AudioClip> songs = new List<AudioClip>();  // Lista de canciones (.wav)

    void Start()
    {
        PlayRandomSong();
    }

    void Update()
    {
        // Si no se está reproduciendo nada, reproducir otra canción aleatoria
        if (!audioSource.isPlaying)
        {
            PlayRandomSong();
        }
    }

    void PlayRandomSong()
    {
        if (songs.Count == 0) return;

        int randomIndex = Random.Range(0, songs.Count);
        audioSource.clip = songs[randomIndex];
        audioSource.Play();
    }
}