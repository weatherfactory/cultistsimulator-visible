using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/* UI Particle Scaling script by Tobi Tobasco, questions: tobias@lostmountaingames.com, http://tobiasnoller.com/
 * Just drag this script on any canvas which has child particle systems underneath. 
 * Keep in mind, you should run this script when you change resolutions.
 * Prewarmed particle system emitting on begin play, will probably not be affected by the script the first time they fire.
 * Feel free to use this script in any project. 
 * If you change/enhance this script, please share your optimizations with the community.
*/

[ExecuteInEditMode]
[RequireComponent(typeof(Canvas))]
public class UIParticleCanvasScaler : UIBehaviour {

    private Canvas canvas;
    private Vector3 m_canvasScale;

    // Lifecycle
    protected override void OnEnable()     { SetParticleScales(); }

#if UNITY_EDITOR
    protected override void Reset()        { SetParticleScales(); }
//    private void Update()       { SetParticleScales(); }
#endif

    // Trigger Update

    public void SetParticleScales() {
        var emitters = this.gameObject.GetComponentsInChildren<ParticleSystem>();
        ApplyScale(emitters);
    }

    // Initilaization

    private void Init() {
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        m_canvasScale = GetCanvasScale();
    }

    // Set Particle Scale

    private Vector3 GetCanvasScale() {
        return canvas.transform.localScale;
    }

    public void ApplyScale(ParticleSystem[] targets) {
        Init();

        if (targets != null)
            foreach (ParticleSystem p in targets)
                if (p != null)
                    p.transform.localScale = m_canvasScale; 
    }

    public void ApplyScale(Transform target) {
        ApplyScale(transform.GetComponentsInChildren<ParticleSystem>());
    }

}