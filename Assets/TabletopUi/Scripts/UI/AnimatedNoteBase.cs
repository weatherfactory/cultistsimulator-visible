using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimatedNoteBase : MonoBehaviour {

    protected delegate void AnimResponse();
    protected enum AnimType { None, MoveRight, MoveLeft }
#pragma warning disable 649
    [SerializeField] Animation anim;
	[SerializeField] string soundIn;
	[SerializeField] string soundOut;
#pragma warning restore 649
    private bool isBusy;

    protected virtual void OnDisable() {
        isBusy = false;
    }

    protected bool IsBusy() {
        return isBusy;
    }

    protected void TriggerAnim(AnimType animOut, AnimType animIn, AnimResponse onOutDone = null, AnimResponse onInDone = null) {
        StartCoroutine(DoAnim(animOut, animIn, onOutDone, onInDone));
    }

    private IEnumerator DoAnim(AnimType animOut, AnimType animIn, AnimResponse onOutDone, AnimResponse onInDone) {
        string clipName;
        isBusy = true;

        clipName = GetOutClip(animOut);
        if (clipName != null) { 
			SoundManager.PlaySfx(soundOut);
            anim.Play(clipName);

            while (anim.isPlaying)
                yield return null;
        }

        if (onOutDone != null)
            onOutDone();

        clipName = GetInClip(animIn);
        if (clipName != null) { 
			SoundManager.PlaySfx(soundIn);
            anim.Play(clipName);

            while (anim.isPlaying)
                yield return null;
        }

        if (onInDone != null)
            onInDone();

        isBusy = false;
    }

    private string GetOutClip(AnimType direction) {
        if (direction == AnimType.MoveRight)
            return "situation-note-move-out-r";
        else if (direction == AnimType.MoveLeft)
            return "situation-note-move-out-l";
        else
            return null;
    }

    private string GetInClip(AnimType direction) {
        if (direction == AnimType.MoveLeft)
            return "situation-note-move-in-l";
        else if (direction == AnimType.MoveRight)
            return "situation-note-move-in-r";
        else
            return null;
    }

}
