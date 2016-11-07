using UnityEngine;
using System.Collections;
using TMPro;
using TMPro.Examples;
using UnityEngine.UI;

/// <summary>
/// this is a crude first-pass way of displaying popups next to related objects.
/// suggestions on better implementations welcome!
/// </summary>
public class NotificationPanel : MonoBehaviour
{
    public float Lifespan;
    public float TimeDisplayed;
    private bool fading = false;
    [SerializeField]private TextMeshProUGUI txtBody;
    [SerializeField]private TextMeshProUGUI txtAside;


    // Update is called once per frame
    void Update ()
	{
	 
        if(TimeDisplayed <Lifespan)
        { 
	    TimeDisplayed = TimeDisplayed + Time.deltaTime;
        
        if((Lifespan-TimeDisplayed)<=1 && !fading)
            { 
            gameObject.GetComponent<Image>().CrossFadeAlpha(0,1,false);
                fading = true;
            }
            if (TimeDisplayed>Lifespan)
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 'aside' as in 'a thing you say while saying something else, like a comment'
    /// so this is 'set the value of the Aside field' not, like 'put aside'
    /// </summary>
    /// <param name="aside"></param>
    public void SetAside(string aside)
    {
        txtAside.text = aside;
    }
    public void SetBodyText(string bodyText)
    {
        txtBody.text = bodyText;
    }

    public void HastenDecay()
    {
        if(Lifespan-TimeDisplayed>1)
        TimeDisplayed = Lifespan - 1;
    }
}
