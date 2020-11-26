using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;
using UnityEngine.EventSystems;

public interface ISituationNote
{
    string Title { get; }
    string Description { get; }
}

public class SituationNote : ISituationNote
{
    /// <summary>
    /// Title is currently unused.
    /// </summary>
    public string Title { get; set; }
    public string Description { get; set; }

    public SituationNote()
    {
        
    }

    public SituationNote(string description)
    {
        Description = description;
    }

    public SituationNote(string title, string description)
    {
        Title = title;
        Description = description;
    }


}


