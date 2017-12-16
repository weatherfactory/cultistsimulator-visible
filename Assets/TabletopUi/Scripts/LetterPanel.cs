#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;

public class LetterPanel : MonoBehaviour {

    [SerializeField]
    CanvasGroupFader canvasGroupFader;

    [SerializeField]
    GameObject[] letterObjects;

    [SerializeField]
    GameObject[] creditsObjects;

    public void ShowCredits() {
        if (canvasGroupFader.IsFading())
            return;

        for (int i = 0; i < letterObjects.Length; i++)
            letterObjects[i].SetActive(false);

        for (int i = 0; i < creditsObjects.Length; i++)
            creditsObjects[i].SetActive(true);

        canvasGroupFader.Show();
    }

    public void ShowLetter() {
        if (canvasGroupFader.IsFading())
            return;

        for (int i = 0; i < letterObjects.Length; i++) 
            letterObjects[i].SetActive(true);

        for (int i = 0; i < creditsObjects.Length; i++)
            creditsObjects[i].SetActive(false);

        canvasGroupFader.Show();
    }

    public void Hide() {
        if (canvasGroupFader.IsFading())
            return;

        canvasGroupFader.Hide();
    }


}
