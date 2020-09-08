#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine.Audio;

public class SoundManager : AudioManager, ISettingSubscriber
{

    [System.Serializable]
    public class SoundCombo {
        public string name;
        public AudioClip[] randomizedClips;
        public bool loop = false;
        [Range(0, 2f)]
        public float volume = 1f;
        [Range(0, 0.5f)]
        public float pitchVariation = 0f;

        public float GetPitch() {
            if (pitchVariation == 0)
                return 1f;

            return 1f + Random.Range(-pitchVariation, pitchVariation);
        }
    }

    // Singleton
    private static SoundManager instance = null;
    public static SoundManager Instance { get { return instance; } }

    [Tooltip("Sound volume uses DB ranges ")]
    public float soundDbMin = -40f;
    public float soundDbMax = 10f;


    [SerializeField]
    AudioMixerGroup mixerTarget;
    // Audio Data
    [SerializeField]
    private SoundCombo[] sounds;
    private Dictionary<string, SoundCombo> soundsMapped = new Dictionary<string, SoundCombo>();
    private List<AudioSource> audioSourcePool = new List<AudioSource>();

    // If a sound is called, we put it here.
    // This list is cleared at the end of each frame
    // Used to prevent multiple sounds started in the same frame.
    private List<string> soundsThisFrame = new List<string>();

    // Initalization

    void Awake() {
		// Instances in scenes were going to are being killed
        if (Instance != null) {
			DestroyImmediate(this.gameObject);
            return;
        }

		// make sure we set our instance and don't ever destroy it
		instance = this;
		//DontDestroyOnLoad(this.gameObject);

        foreach (SoundCombo sound in sounds) {
            if (soundsMapped.ContainsKey(sound.name)) 
                Debug.LogError("There is already a sound with the name " + sound.name + " in the list. Please check your SoundManager settings!");

            soundsMapped[sound.name] = sound;
        }

    }

    void Start()
    {
        var soundVolumeSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.SOUNDVOLUME);

        if (soundVolumeSetting == null)
        {
            NoonUtility.Log("Missing setting entity: " + NoonConstants.SOUNDVOLUME);
            return;
        }
        else
        {
            soundVolumeSetting.AddSubscriber(this);
            UpdateValueFromSetting(soundVolumeSetting.CurrentValue);
        }
        

    }


    public void UpdateValueFromSetting(object newValue)
    {
        SetVolumeInDbRange(newValue is float ? (float)newValue : 0);
    }

    private void SetVolumeInDbRange(float volume)
    {

        float dbVol = Mathf.Lerp(soundDbMin, soundDbMax, 1f - GetClampedVol(10f - volume));

        mixerTarget.audioMixer.SetFloat("masterVol", dbVol);

        if (volume == 0f && SoundManager.Instance.IsOn())
            SoundManager.Instance.SetVolume(0f);
        else if (volume > 0f && SoundManager.Instance.IsOn() == false)
            SoundManager.Instance.SetVolume(1f);

    }


    float GetClampedVol(float sliderValue)
    {
        return Mathf.Pow(sliderValue / 10f, 2f); // slider has whole numbers only and goes from 0 to 10
    }

    // Delivery of Audio Clips & Sources

    private int GetFreeAudioSourceId(AudioClip clip) {
        for (int i = 0; i < audioSourcePool.Count; i++) {
            if (audioSourcePool[i].isPlaying == false)
                return i;
        }

        return AddNewAudioSource(clip);
    }

    private int AddNewAudioSource(AudioClip clip) {
        int index = audioSourcePool.Count;
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.outputAudioMixerGroup = mixerTarget;
        audioSourcePool.Add(newSource);
        return index;
    }

    private AudioSource GetAudioSource(int id) {
        if (id < audioSourcePool.Count) {
            return audioSourcePool[id];
        }

        return null;
    }

    private SoundCombo GetSound(string name) {
        SoundCombo combo;

        if (soundsMapped.TryGetValue(name, out combo))
            return combo;

        Debug.LogWarning("No such SoundCombo " + name + " in SoundManager");
        return null;
    }

    private AudioClip GetRandomClipFromSound(SoundCombo sound) {
        if (sound.randomizedClips.Length == 1)
            return sound.randomizedClips[0];

        int clipNumber = Random.Range(0, sound.randomizedClips.Length);

        return sound.randomizedClips[clipNumber];
    }

    // Public Playback Methods

    public static int PlaySfx(string name) {
        if (Instance == null)
            return -1;

		if (string.IsNullOrEmpty(name))
			return -1;

        if (Instance.soundsThisFrame.Contains(name))
            return -1;


        return Instance.PlaySound(name, -1);
    }

    public static void StopSfx(int id) {
        if (Instance == null)
            return;

        Instance.StopSound(id);
    }

    // internal handling methods for playback

    private int PlaySound(string name, float volume) {
        if (!isOn)
            return -1;

        SoundCombo sound = GetSound(name);

        if (sound == null)
            return -1;

        var clip = GetRandomClipFromSound(sound);

        if (clip == null)
            return -1;

        int id = GetFreeAudioSourceId(clip);
        AudioSource source = GetAudioSource(id);

        if (source == null)
            return -1;

        source.clip = GetRandomClipFromSound(sound);
        source.volume = (volume >= 0 ? volume : sound.volume);
        source.pitch = sound.GetPitch();
        source.loop = sound.loop;
        source.Play();

        soundsThisFrame.Add(name);

        return id;
    }

    private void StopSound(int id) {
        AudioSource source = GetAudioSource(id);

        if (source == null) {
            //Debug.LogWarning("StopAudio: AudioSource with id " + id + " not found");
            return;
        }

        source.Stop();
    }

    // LIfecycle

    protected override void SetEnabled(bool turnOn) {
        if (this.isOn == turnOn)
            return;

        base.SetEnabled(turnOn);

        if (!turnOn)
            TurnOffAllAudio();
    }

    private void TurnOffAllAudio() {
        foreach (AudioSource audioSource in audioSourcePool) 
            audioSource.Stop();
    }

    private void LateUpdate() {
        soundsThisFrame.Clear();
    }

    private void OnDestroy() {        
        TurnOffAllAudio();

        if (Instance == this)
            instance = null;
    }

	#if UNITY_EDITOR
	public void SortSounds() {
		var soundList = new List<SoundCombo>(sounds);
		soundList.Sort((x, y) => x.name.CompareTo(y.name));

		sounds = soundList.ToArray();
	}
	#endif
}