using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{

    private IEnumerable<AudioClip> backgroundMusic;
    [SerializeField] private AudioSource audioSource;
    private System.Random random;
	// Use this for initialization
	void Awake ()
	{
	    backgroundMusic = ResourcesManager.GetBackgroundMusic();
	    random=new System.Random();
	}

    void Start()
    { PlayRandomClip(); }
	
	
	void Update () {
	    if (!audioSource.isPlaying)
	    {
            PlayRandomClip();
            
	    }
    }
    public  void PlayRandomClip()
	    {
            audioSource.Stop();
            audioSource.PlayOneShot(backgroundMusic.ElementAt(random.Next(0, backgroundMusic.Count())));
        }
	
}
