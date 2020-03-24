using Assets.CS.TabletopUI;
using System.Collections;
using System.Collections.Generic;
using Noon;
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
#endif
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
#if MODS
        registry.Register(new ModManager(false));
#endif
        var compendium = new Compendium();
        var contentImporter = new ContentImporter();
       var problems= contentImporter.PopulateCompendium(compendium);
       foreach (var p in problems)
           NoonUtility.Log(p.Description, messageLevel: 2);

        registry.Register<ICompendium>(compendium);
	}
}
