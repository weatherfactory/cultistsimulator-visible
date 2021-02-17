using System.Collections;
using System.Collections.Generic;
using SecretHistories.UI;
using SecretHistories.Fucine;
using UnityEngine;

public class OptionsButton : MonoBehaviour
{
    public void OnClick()
    {
        Watchman.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
    }
}
