﻿using SecretHistories.UI;
using System.Collections;
using System.Collections.Generic;

using SecretHistories.Infrastructure.Modding;
using SecretHistories.Services;
using UnityEngine;

public class DebugLoadCompendium : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetupServices();
    }


	private void SetupServices()
	{
        var registry = new Registry();
        registry.Register(new ModManager());
        var compendium = new Compendium();
        var contentImporter = new CompendiumLoader();
        var messages =
            contentImporter.PopulateCompendium(compendium, Registry.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));
       foreach (var m in messages.GetMessages())
           NoonUtility.Log(m.Description, m.MessageLevel);

        registry.Register<Compendium>(compendium);
	}
}