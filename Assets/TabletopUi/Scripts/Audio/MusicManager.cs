using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : AudioManager {

    [System.Serializable]
    public class MusicCombo {
        public string name;
        public string clipLocation;
        public float volume = 1f;
    }

    // Singleton
    private static MusicManager instance = null;
    public static MusicManager Instance { get { return instance; } }

    // Fade Settings
    [SerializeField] string resourcesPath = "Music/";
    [SerializeField] string startingTrack = "";
    [SerializeField] float fadeInTime = 2f;
    [SerializeField] float fadeOutTime = 2f;

    // Audio Data
    [Header("Music Tracks")]
    [SerializeField]
    private MusicCombo[] music;	
	private Dictionary<string, MusicCombo> musicMapped = new Dictionary<string, MusicCombo>();
	protected AudioSource audioSourceMusic;

	private string currentTrack = "";
	private string lastTrack = "";
	private string nextTrack = "";

    // Initalization
    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        audioSourceMusic = gameObject.AddComponent<AudioSource>();
		audioSourceMusic.loop = true;
		audioSourceMusic.playOnAwake = false;
		
		foreach (MusicCombo musicItem in music) {
            if (musicMapped.ContainsKey(musicItem.name))
                Debug.LogError("There is already a sound with the name " + musicItem.name + " in the list. Please check your SoundManager settings!");

            musicMapped[musicItem.name] = musicItem;
        }

        if (startingTrack != "")
            StartTrack(startingTrack);
    }

    // Delivery of Audio Clips & Sources

    private MusicCombo GetMusic(string trackname) {
        MusicCombo combo;

        if (musicMapped.TryGetValue(trackname, out combo))
            return combo;

        return null;
    }

    private AudioClip GetClip(MusicCombo combo) {
        if (combo == null)
            return null;

        AudioClip clip = Resources.Load<AudioClip>(resourcesPath + combo.clipLocation);

        return clip;
    }

    // Public Playback Methods

    public static void PlayMusic(string trackName) {
		if (Instance == null)
			return;

		Instance.StartTrack(trackName);
	}

	public static void StopMusic() {
		if (Instance == null)
			return;

		Instance.StopTrack();
	}

	public static bool IsCurrentlyPlaying() {
		if (Instance == null)
			return false;

		return Instance.currentTrack != "";
	}

    // internal handling methods for playback

    private void StartTrack(string trackName) {
		if (!isOn || (audioSourceMusic.isPlaying && trackName == currentTrack) || GetMusic(trackName) == null)
			return;

		StopAllCoroutines();

		if (audioSourceMusic.isPlaying) {
			nextTrack = trackName;
			FadeOutTrack(currentTrack);
		} 
		else {
			FadeInTrack(trackName);
		}
	}

    private void StopTrack() {
        if (currentTrack == "")
            return;
        
		StopAllCoroutines();
		FadeOutTrack(currentTrack);
	}

    // Do Fade Ins

    private void FadeInTrack(string trackName, float duration = -1f) {
		var music = GetMusic(trackName);

		if (music == null)
			return;

		lastTrack = currentTrack;
		currentTrack = trackName;
		StartCoroutine(DoFadeInSpeaker(trackName, music, audioSourceMusic, duration));
	}

	private IEnumerator DoFadeInSpeaker(string trackName, MusicCombo music, AudioSource source, float duration) {
		float time = 0f;
		float startValue = 0f;

		source.clip = GetClip(music);
		source.volume = startValue;
		source.Play();

		if (duration < 0f)
			duration = fadeInTime; 

		while (time < duration) {
			yield return null;
			time += Time.deltaTime;
			source.volume = Mathf.Lerp(startValue, music.volume, time/duration);	
		}

		source.volume = music.volume;
	}

    // Do Fade Outs

    private void FadeOutTrack(string trackName, float duration = -1f) {
		var music = GetMusic(trackName);

		if (music == null)
			return;

		lastTrack = currentTrack;
		StartCoroutine(DoFadeOutSpeaker(trackName, music, audioSourceMusic, duration));
	}

    private IEnumerator DoFadeOutSpeaker(string trackName, MusicCombo music, AudioSource source, float duration) {
		float time = 0f;
		float startValue = source.volume;

		if (duration < 0f)
			duration = fadeOutTime; 

		while (time < duration) {
			yield return null;
			time += Time.deltaTime;
            source.volume = Mathf.Lerp(startValue, 0f, time / duration);
		}

		source.volume = 0f;
		source.Stop();

		if (nextTrack != "") {
			FadeInTrack(nextTrack);
			nextTrack = "";
		}
	}

    // LIfecycle

    protected override void SetEnabled(bool turnOn) {
        if (this.isOn == turnOn)
            return;

        base.SetEnabled(turnOn);

        if (!isOn && currentTrack != "") {
            StopAllCoroutines();
            lastTrack = currentTrack;
            TurnOffAllAudio();
        }
        else if (isOn && lastTrack != "") { 
             StartTrack(lastTrack);
        }
    }

    public static void Reset() {
        if (Instance == null)
            return;

        Instance.TurnOffAllAudio();
    }

    private void TurnOffAllAudio() {
        audioSourceMusic.Stop();
        currentTrack = "";
    }

    private void OnDestroy() {
        TurnOffAllAudio();

        if (Instance == this)
            instance = null;
    }
}
