using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;

public class NavigationAnimation : MonoBehaviour {

    public delegate void AnimationResponse(NavigationArgs args);
#pragma warning disable 649
    [SerializeField] Animation animations;
	[SerializeField] string soundIn;
	[SerializeField] string soundOut;
#pragma warning restore 649
    private bool isBusy;
    private Coroutine _currentAnimation;


    protected virtual void OnDisable() {
        isBusy = false;
    }

    protected bool IsBusy() {
        return isBusy;
    }

    public void TriggerAnimation(NavigationArgs args) {
        if(_currentAnimation!=null)
            StopCoroutine(_currentAnimation);

        _currentAnimation=StartCoroutine(DoAnimation(args));
    }

    private IEnumerator DoAnimation(NavigationArgs args) {
        string clipName;
        isBusy = true;

        args.OnBegin?.Invoke(args);
        args.OnBegin = null;

        clipName = GetOutClip(args.FirstNavigationDirection);
        if (clipName != null) { 
			SoundManager.PlaySfx(soundOut);
            animations.Play(clipName);

            while (animations.isPlaying)
                yield return null;
        }

        args.OnOutComplete?.Invoke(args);
        args.OnOutComplete = null;


        clipName = GetInClip(args.FinalNavigationDirection);
        if (clipName != null) { 
			SoundManager.PlaySfx(soundIn);
            animations.Play(clipName);

            while (animations.isPlaying)
                yield return null;
        }

        args.OnInComplete?.Invoke(args);
        args.OnInComplete = null;

        args.OnEnd?.Invoke(args);
        args.OnEnd = null;

        isBusy = false;
    }


    private string GetOutClip(NavigationAnimationDirection direction) {
        if (direction == NavigationAnimationDirection.MoveRight)
            return "situation-note-move-out-r";
        else if (direction == NavigationAnimationDirection.MoveLeft)
            return "situation-note-move-out-l";
        else
            return null;
    }

    private string GetInClip(NavigationAnimationDirection direction) {
        if (direction == NavigationAnimationDirection.MoveLeft)
            return "situation-note-move-in-r";
        else if (direction == NavigationAnimationDirection.MoveRight)
            return "situation-note-move-in-l";
        
        else
            return null;
    }

}
