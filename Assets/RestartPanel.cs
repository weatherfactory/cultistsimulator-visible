using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.CS.TabletopUI;
using TMPro;
using UnityEngine;

public class RestartPanel : MonoBehaviour {

    [SerializeField]
    private TabletopManager tabletopManager;

    [SerializeField] private TextMeshProUGUI title;
    [SerializeField]
    private TextMeshProUGUI description;

    public void Display(Notification n)
    {
        gameObject.SetActive(true);
        title.text = n.Title;
        description.text = n.Description;
    }

    public void LoadGame()
    {
        tabletopManager.LoadGame();
        gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        tabletopManager.RestartGame();
        gameObject.SetActive(false);

    }
}
