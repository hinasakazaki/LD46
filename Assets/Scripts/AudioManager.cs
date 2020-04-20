using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip main_track;
    public AudioClip[] walkList = new AudioClip[4];
    public AudioClip ding;
    public AudioClip[] paperList = new AudioClip[2];
    public AudioSource SFX;
    public AudioClip shatter;
    public AudioClip happy;
    
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<AudioSource>().clip = main_track;
        this.GetComponent<AudioSource>().loop = true;
        this.GetComponent<AudioSource>().Play();
    }

    public void Walk()
    {
        SFX.PlayOneShot(walkList[Random.Range(0, walkList.Length)], 0.7f);
    }

    public void ItemSound()
    {
        SFX.PlayOneShot(ding, 0.7f);
    }

    public void HappySound()
    {
        SFX.PlayOneShot(happy, 0.7f);
    }

    public void SadSound()
    {
        SFX.PlayOneShot(shatter, 0.7f);
    }

    public void LetterSound()
    {
        SFX.PlayOneShot(paperList[Random.Range(0, paperList.Length)], 1.5f);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
