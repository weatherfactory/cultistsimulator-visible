using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{

    protected IEnumerable<AudioClip> backgroundMusic;
    protected IEnumerable<AudioClip> impendingDoomMusic;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] public int currentTrackNumber;
    private System.Random random;
	// Use this for initialization
	void Awake ()
	{
	    backgroundMusic = ResourcesManager.GetBackgroundMusic();
	    impendingDoomMusic = ResourcesManager.GetImpendingDoomMusic();
	    random=new System.Random();
	}

    void Start()
    {
        PlayNextClip();
    }
	
	
	void Update () {
	    if (!audioSource.isPlaying)
	    {
	        PlayNextClip();
	    }
    }

    public virtual void PlayNextClip()
    {
        //currentTrackNumber++;
        //if (currentTrackNumber == backgroundMusic.Count())
        //    currentTrackNumber = 0;
        //PlayClip(currentTrackNumber);

        PlayClip(random.Next(1, backgroundMusic.Count()), backgroundMusic); //never plays the first clip
    }

    public void PlayClip(int trackNumber,IEnumerable<AudioClip> clips)
    {
        audioSource.Stop();
        var clip = clips.ElementAt(trackNumber);
        audioSource.PlayOneShot(clip);
        Debug.Log(clip.name);
    }
    public  void PlayRandomClip()
	    {
        PlayClip(random.Next(0, backgroundMusic.Count()),backgroundMusic);
        }

    public void SetMute(bool mute)
    {
        audioSource.mute=mute;
    }

    public void PlayImpendingDoom()
    {
       PlayClip(0,impendingDoomMusic);
      
    }
}
