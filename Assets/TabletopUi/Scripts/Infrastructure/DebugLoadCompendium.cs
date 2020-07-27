﻿using Assets.CS.TabletopUI;
using System.Collections;
using System.Collections.Generic;
using Noon;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using UnityEngine;

public class DebugLoadCompendium : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetupServices();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void SetupServices()
	{
        var registry = new Registry();
        registry.Register(new ModManager(false));
        var compendium = new Compendium();
        var contentImporter = new CompendiumLoader();
       var messages= contentImporter.PopulateCompendium(compendium);
       foreach (var m in messages.GetMessages())
           NoonUtility.Log(m.Description, m.MessageLevel);

        registry.Register<ICompendium>(compendium);
	}
}
