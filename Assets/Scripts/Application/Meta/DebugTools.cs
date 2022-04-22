#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Constants.Modding;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;
using SecretHistories.Spheres;
using Steamworks;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebugTools : MonoBehaviour
{
    private const int MaxAutoCompletionSuggestions = 50;


    [SerializeField] private Heart heart;
    

    public List<GameObject> Panels;
    private GameObject CurrentPanel;



    public void Cycle()
    {
        if (!isActiveAndEnabled)        {
            gameObject.SetActive(true);
            SetCurrentPanel(Panels.First());
        }
        else
        {
            if (CurrentPanel == Panels.Last())
                gameObject.SetActive(false);
            else
            {
                var nextPanelIndex = Panels.IndexOf(CurrentPanel) + 1;
                SetCurrentPanel(Panels[nextPanelIndex]);
            }
        }

    }

    private void SetCurrentPanel(GameObject panel)
    {
        CurrentPanel = panel;
        foreach (var p in Panels)
        {
            if(p==CurrentPanel)
                p.SetActive(true);
            else
                p.SetActive(false);
        }
    }


public void Awake()
    {
        gameObject.SetActive(false); //start by hiding the panel. If it's not enabled at the beginning, this won't run
        var registry = new Watchman();
        registry.Register(this);

    }


    public void ToggleVisibleLog()
    {
        Watchman.Get<Concursum>().ToggleSecretHistory();
    }

   


   

   




   






}


