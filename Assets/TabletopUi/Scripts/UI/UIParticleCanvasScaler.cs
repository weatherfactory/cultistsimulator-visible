using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/* UI Particle Scaling script by Tobi Tobasco, questions: tobias@lostmountaingames.com, http://tobiasnoller.com/
 * Just drag this script on any canvas which has child particle systems underneath. 
 * Keep in mind, you should run this script when you change resolutions.
 * Prewarmed particle system emitting on begin play, will probably not be affected by the script the first time they fire.
 * Feel free to use this script in any project. 
 * If you change/enhance this script, please share your optimizations with the community.
*/

[ExecuteInEditMode]
[RequireComponent(typeof(Canvas))]
public class UIParticleCanvasScaler : MonoBehaviour {

    private Canvas canvas;
    private ParticleSystem[] emitters;
    private Vector3 m_canvasScale;

#if !UNITY_EDITOR
    private bool m_isInitialized;
#endif

    // Lifecycle
    private void OnEnable()     { SetParticleScales(); }

#if UNITY_EDITOR
    private void Reset()        { SetParticleScales(); }
    private void Update()       { SetParticleScales(); }
#endif

    // Trigger Update

    private void SetParticleScales() {
        Init();
        ApplyScale();
    }

    // Initilaization

    private void Init() {
#if !UNITY_EDITOR
        if (!m_isInitialized) 
            return; 

        m_isInitialized = true;
#endif
        canvas = GetComponent<Canvas>();
        emitters = this.gameObject.GetComponentsInChildren<ParticleSystem>();

        m_canvasScale = GetCanvasScale();
    }

    // Set Particle Scale

    private Vector3 GetCanvasScale() {
        return new Vector3( canvas.scaleFactor * canvas.transform.localScale.x, 
                            canvas.scaleFactor * canvas.transform.localScale.y, 
                            canvas.scaleFactor * canvas.transform.localScale.z  );
    }

    private void ApplyScale() {
        foreach (ParticleSystem p in emitters)
            if (p != null)
                p.transform.localScale = m_canvasScale; 
    }

}