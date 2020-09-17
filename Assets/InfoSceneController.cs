using System.Collections;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using UnityEngine;

public class InfoSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    public void BrowseToFiles()
    {
        OpenInFileBrowser.Open(Application.persistentDataPath);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
