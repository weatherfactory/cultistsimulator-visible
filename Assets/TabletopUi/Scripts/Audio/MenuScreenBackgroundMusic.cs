using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuScreenBackgroundMusic : BackgroundMusic
{



    public override void PlayNextClip()
    {
        
        PlayClip(currentTrackNumber);
    }

	
}
