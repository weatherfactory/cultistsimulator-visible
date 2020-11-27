using System.Collections;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

public class OptionsButton : MonoBehaviour
{
    public void OnClick()
    {
        Registry.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
    }
}
