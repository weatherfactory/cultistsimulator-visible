using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEditor;

/// <summary>
/// Top-level object
/// </summary>
public class Heart : MonoBehaviour
{
    [SerializeField] private Transform allContent;

    private const string METHODNAME_BEAT="Beat"; //so we don't get a tiny daft typo with the Invoke
    private float interval;
  public void BeginHeartbeat(float startingInterval)
  {
      interval = startingInterval;
        InvokeRepeating(METHODNAME_BEAT,0, interval);
    }

    public void PauseHeartbeat()
    {
        CancelInvoke(METHODNAME_BEAT);
    }

    public void Beat()
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
