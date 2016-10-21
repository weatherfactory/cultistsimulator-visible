using UnityEngine;
using System.Collections;
using TMPro;
using TMPro.Examples;
using UnityEngine.UI;

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
