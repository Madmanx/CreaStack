using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;
    public AudioClip sound_C;

    private float basePitch = 1;

    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayComboSound()
    {
        audioSource.pitch = basePitch + (GameManager.Instance.Combo/3) ;
        audioSource.PlayOneShot(sound_C);
    }

    public void PlayDefeatSound()
    {
        audioSource.pitch = 0.5f;
        audioSource.PlayOneShot(sound_C);
    }
}
