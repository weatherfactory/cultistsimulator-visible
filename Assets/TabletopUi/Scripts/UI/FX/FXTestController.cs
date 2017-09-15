using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;

public class FXTestController : MonoBehaviour {

    public CardEffect effect;
    public ElementStackToken targetToken;


    private CardEffect activeEffect;
    private ElementStackToken activeToken;

    void OnEnable () {
        targetToken.gameObject.SetActive(false);
        effect.gameObject.SetActive(false);

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
    }

}
