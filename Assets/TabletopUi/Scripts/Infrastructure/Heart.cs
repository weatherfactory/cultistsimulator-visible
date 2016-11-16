using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;

    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private float usualInterval;
  public void BeginHeartbeat(float startingInterval)
  {
      usualInterval = startingInterval;
        InvokeRepeating(METHODNAME_BEAT,0, usualInterval);
    }

    public void PauseHeartbeat()
    {
        CancelInvoke(METHODNAME_BEAT);
    }

    public void Beat()
    {
            Beat(usualInterval);
    }

    public void Beat(float interval)
    {
        //foreach existing active recipe window: run beat there
        //advance timer
        Debug.Log("beat");
        var verbBoxes = allContent.GetComponentsInChildren<VerbBox>();
        foreach (var v in verbBoxes)
        {
            v.ContinueSituation(interval);
        }
    }


}
