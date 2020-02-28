using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDLCLegacyConfirm : MonoBehaviour
{
public string LegacyId { get; set; }
    public MenuScreenController msc;

    public void BeginGameWithRelevantLegacy()
    {
        msc.BeginNewGameWithSpecifiedLegacyAndPurgeOldSave(LegacyId);
    }
}
