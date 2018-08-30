using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalHook : MonoBehaviour
{
	// Load scene before anything else calls Start()
	void Awake()
	{
		// Check if the Global Scene is loaded. If not, yoink it in!
		GameObject lm = GameObject.Find( "LanguageManager" );
		if (lm == null)
		{
			SceneManager.LoadScene( "Global", LoadSceneMode.Additive );
		}
	}
}
