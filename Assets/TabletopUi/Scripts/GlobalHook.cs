using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalHook : MonoBehaviour
{
	// Load scene before anything else calls Start()
	void Awake()
	{
		// Check if the Global Scene is loaded. If not, yoink it in!
		GameObject c = GameObject.Find( "Concursum" );
		if (c == null)
		{
			SceneManager.LoadScene( "Global", LoadSceneMode.Additive );
		}
	}
}
