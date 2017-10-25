using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : AudioManager {

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

    // Audio Data
    [SerializeField]
    private SoundCombo[] sounds;
    private Dictionary<string, SoundCombo> soundsMapped = new Dictionary<string, SoundCombo>();
    private List<AudioSource> audioSourcePool = new List<AudioSource>();

    // Initalization

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;

        foreach (SoundCombo sound in sounds) {
            if (soundsMapped.ContainsKey(sound.name)) 
                Debug.LogError("There is already a sound with the name " + sound.name + " in the list. Please check your SoundManager settings!");

            soundsMapped[sound.name] = sound;
        }
    }

    // Delivery of Audio Clips & Sources

    private int GetFreeAudioSourceId(AudioClip clip) {
        string name = clip.name;

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

#if DEBUG
        Debug.Log("Playing SFX " + name);
#endif

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

        int id = GetFreeAudioSourceId(GetRandomClipFromSound(sound));
        AudioSource source = GetAudioSource(id);

        if (source == null)
            return -1;

        source.clip = GetRandomClipFromSound(sound);
        source.volume = (volume >= 0 ? volume : sound.volume);
        source.pitch = sound.GetPitch();
        source.loop = sound.loop;
        source.Play();

        return id;
    }

    private void StopSound(int id) {
        AudioSource source = GetAudioSource(id);

        if (source == null) {
            Debug.LogWarning("StopAudio: AudioSource with id " + id + " not found");
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

    private void OnDestroy() {        
        TurnOffAllAudio();

        if (Instance == this)
            instance = null;
    }
}