﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;

public class FXTestController : MonoBehaviour {

    public CardEffect[] testEffects;
    public ElementStackToken targetToken;

    public float repeatSpeed = 3f;

    private CardEffect activeEffect;
    private ElementStackToken activeToken;

    private int effectNum = 0;

    void OnEnable () {
        targetToken.gameObject.SetActive(false);

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

        activeToken = Instantiate(targetToken, targetToken.transform.parent) as ElementStackToken;
        activeToken.transform.position = targetToken.transform.position;
        activeToken.transform.localScale = targetToken.transform.localScale;
        activeToken.gameObject.SetActive(true);

        Invoke("StartAnim", 1f);
    }

    void StartAnim() {
        activeEffect.StartAnim(activeToken);

        effectNum++;

        if (effectNum >= testEffects.Length)
            effectNum = 0;

        Invoke("SpawnFX", repeatSpeed);
    }

}
