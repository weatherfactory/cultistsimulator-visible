using Assets.CS.TabletopUI;
using System.Collections;
using System.Collections.Generic;
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
        var compendium = new Compendium();
        var contentImporter = new ContentImporter();
        contentImporter.PopulateCompendium(compendium);

        registry.Register<ICompendium>(compendium);
	}
}
