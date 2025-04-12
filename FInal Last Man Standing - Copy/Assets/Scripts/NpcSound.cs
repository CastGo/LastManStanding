using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSound : MonoBehaviour
{
    public float playRadius;
    private float distanceToPlayer;
    private Transform player;

    public AudioClip sfxSound;
    private AudioSource audios;

    void Start()
    {
        audios = GetComponent<AudioSource>();
        player = GameObject.FindAnyObjectByType<PlayerController>().transform;
    }

    void Update()
    {
        distanceToPlayer = Vector3.Distance(player.position, transform.position);
    }

    public void PlayAudio()
    {
        if (distanceToPlayer <= playRadius)
        {
            audios.PlayOneShot(sfxSound);
        }
    }
}
