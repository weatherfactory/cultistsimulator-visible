using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SecretHistories.UI;

public class FXTestController : MonoBehaviour {

    public CardEffect[] testEffects;
    public ElementStack Target;

    public float waitAfterFX = 3f;
    public float pauseDuration = 1f;    

    private CardEffect activeEffect;
    private ElementStack _active;

    private int effectNum = 0;

    void OnEnable () {
        Target.gameObject.SetActive(false);

        for (int i = 0; i < testEffects.Length; i++)
            testEffects[i].gameObject.SetActive(false);

        SpawnFX();
    }

    void SpawnFX() {
        var effect = testEffects[effectNum];

        activeEffect = Instantiate(effect, effect.transform.parent) as CardEffect;
        activeEffect.transform.position = effect.transform.position;
        activeEffect.transform.localScale = effect.transform.localScale;
        activeEffect.gameObject.SetActive(false);

        _active = Instantiate(Target, Target.transform.parent) as ElementStack;
        _active.transform.position = Target.transform.position;
        _active.transform.localScale = Target.transform.localScale;
        _active.gameObject.SetActive(true);

        if (effect is CardEffectCreate)
            StartAnim();
        else
            Invoke("StartAnim", 1f);
    }

    void StartAnim() {
        activeEffect.StartAnim(_active.transform);

        effectNum++;

        if (effectNum >= testEffects.Length)
            effectNum = 0;

        if (waitAfterFX > 0)
            Invoke("CleanUp", waitAfterFX);
        else
            CleanUp();
    }

    void CleanUp() {
        if (activeEffect != null)
            Destroy(activeEffect.gameObject);

        if (_active != null)
            Destroy(_active.gameObject);

        Invoke("SpawnFX", pauseDuration);
    }

}
