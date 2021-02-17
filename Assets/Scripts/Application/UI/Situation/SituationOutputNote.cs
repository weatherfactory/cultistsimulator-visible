using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;

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


