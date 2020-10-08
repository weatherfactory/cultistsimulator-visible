using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;
using Assets.CS.TabletopUI.Interfaces;

/// <summary>
/// Tracks and performs operations on tokens, considered as game model objects
/// Uses an ITokenPhysicalLocation (in Unity, a TokenTransformWrapper) to change the display
/// Referenced through gameobjects (Unity layer) which implement IContainsTokens
/// IContainsTokens objects should never have direct access to the ITokenPhysicalLocation (though it references them) because everything needs to be filtered through
/// the StacksManager for model management purposes
/// </summary>
public class ElementStacksManager {

    private readonly ITokenContainer _tokenContainer;

    private TokenContainersCatalogue _catalogue;

    public string Name { get; set; }


    public ElementStacksManager(ITokenContainer container, string name) {
        Name = name;
        _tokenContainer = container;

        _catalogue = Registry.Get<TokenContainersCatalogue>();
        _catalogue.RegisterTokenContainer(_tokenContainer);
    }

    public void Deregister() {
        var catalogue = Registry.Get<TokenContainersCatalogue>();
        if (catalogue != null)
            catalogue.DeregisterTokenContainer(_tokenContainer);
    }

 
    


  
}

