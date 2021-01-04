using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using SecretHistories.Infrastructure;

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


