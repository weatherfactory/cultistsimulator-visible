using UnityEngine;

public abstract class AudioManager : MonoBehaviour {

    protected bool isOn = true;
    private float volume = 1f;

    public void SetVolume(float newVolume) {
        if (Mathf.Approximately(volume, newVolume))
            return;

        ApplyVolume(newVolume);
    }

    private void ApplyVolume(float newVolume) {
        if (Mathf.Approximately(volume, newVolume))
            return;

        volume = newVolume;
        SetEnabled(volume > 0f);
    }

    public float GetVolume() {
        return volume;
    }

    protected virtual void SetEnabled(bool turnOn) {
        if (this.isOn == turnOn)
            return;

        this.isOn = turnOn;
    }

    public bool IsOn() {
        return isOn;
    }
}