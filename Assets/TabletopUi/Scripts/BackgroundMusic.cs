using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{

    private IEnumerable<AudioClip> backgroundMusic;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int currentTrackNumber;
    private System.Random random;
	// Use this for initialization
	void Awake ()
	{
	    backgroundMusic = ResourcesManager.GetBackgroundMusic();
	    random=new System.Random();
	}

    void Start()
    { PlayClip(0); }
	
	
	void Update () {
	    if (!audioSource.isPlaying)
	    {
	        PlayNextClip();
	    }
    }

    public void PlayNextClip()
    {
        currentTrackNumber++;
        if (currentTrackNumber == backgroundMusic.Count())
            currentTrackNumber = 0;
        PlayClip(currentTrackNumber);
    }

    public void PlayClip(int trackNumber)
    {
        audioSource.Stop();
        var clip = backgroundMusic.ElementAt(trackNumber);
        audioSource.PlayOneShot(clip);
        Debug.Log(clip.name);
    }
    public  void PlayRandomClip()
	    {
        PlayClip(random.Next(0, backgroundMusic.Count()));
        }

    public void SetMute(bool mute)
    {
        audioSource.mute=mute;
    }
	
}
