using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartDLCLegacyConfirm : MonoBehaviour
{
public string LegacyId { get; set; }
    public MenuScreenController msc;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;

    public void BeginGameWithRelevantLegacy()
    {
        msc.BeginNewSaveWithSpecifiedLegacy(LegacyId);
    }
}
