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


[RequireComponent(typeof(Canvas))]
public class UIParticleScaler : MonoBehaviour {

    private Canvas refCanvas;
    private float scale;
    private ParticleSystem[] particleSystems;
    public bool useCameraOrthSize = false;
    //The default orthographic size of the gui camera
    public float refCameraOrthSize = 384f;

    //Initialization
    void Start() {
        refCanvas = GetComponent<Canvas>();
        // get particle systems
        particleSystems = this.gameObject.GetComponentsInChildren<ParticleSystem>();
        // apply scale
        ApplyScale();    
        // add resolution change listener here
        //e.g.ResChangeListener += ApplyScale;
     }

    //Scale all child particle systems
    public void ApplyScale() {
        float scaleOld = 1f;
        float scaleNew = 1f;
        // scale to 1
        scale = (1 / scaleOld);
        if (useCameraOrthSize) {
            Camera cam = refCanvas.GetComponent<Camera>();
            if (cam != null)
                scaleNew = (refCameraOrthSize / cam.orthographicSize);
        } else {
            scaleNew = refCanvas.transform.localScale.x;
        }
        // scale to new scale
        scale = scaleNew;
        scaleOld = scaleNew;
        foreach (ParticleSystem p in particleSystems) {
            if (p != null)
                ScaleParticleValues(p);
        }
    }

    //Scale individiual particle system values
    private void ScaleParticleValues(ParticleSystem ps) {
        //BASE MODULE
        var main = ps.main;
        //StartSize
        ParticleSystem.MinMaxCurve sSize = main.startSize;
        main.startSize = MultiplyMinMaxCurve(sSize, scale);
        //Gravity
        ParticleSystem.MinMaxCurve sGrav = main.gravityModifier;
        main.gravityModifier = MultiplyMinMaxCurve(sGrav, scale);
        //StartSpeed
        ParticleSystem.MinMaxCurve sSpeed = main.startSpeed;
        main.startSpeed = MultiplyMinMaxCurve(sSpeed, scale);

        //MODULES
        //Shape (divided instead of multiplied)
        var shape = ps.shape;
        if (shape.enabled) {
            shape.radius /= scale;
            shape.scale = shape.scale / scale;
        }
        //Emisison Rate Time (divided instead of multiplied)
        ParticleSystem.EmissionModule em = ps.emission;
        if (em.enabled) {
            //Time
            ParticleSystem.MinMaxCurve emRateT = em.rateOverTime;
            em.rateOverTime = MultiplyMinMaxCurve(emRateT, scale, false);
            //Distance
            ParticleSystem.MinMaxCurve emRateD = em.rateOverDistance;
            em.rateOverDistance = MultiplyMinMaxCurve(emRateD, scale, false);
        }

        //Velocities
        ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
        if(vel.enabled) {
            vel.x = MultiplyMinMaxCurve(vel.x, scale);
            vel.y = MultiplyMinMaxCurve(vel.y, scale);
            vel.z = MultiplyMinMaxCurve(vel.z, scale);         
        }

        //ClampVelocities
        ParticleSystem.LimitVelocityOverLifetimeModule clampVel = ps.limitVelocityOverLifetime;
        if(clampVel.enabled) {
            clampVel.limitX = MultiplyMinMaxCurve(clampVel.limitX, scale);
            clampVel.limitY = MultiplyMinMaxCurve(clampVel.limitY, scale);
            clampVel.limitZ = MultiplyMinMaxCurve(clampVel.limitZ, scale);
        }

        //Forces
        ParticleSystem.ForceOverLifetimeModule force = ps.forceOverLifetime;
        if (force.enabled) {
            force.x = MultiplyMinMaxCurve(force.x, scale);
            force.y = MultiplyMinMaxCurve(force.y, scale);
            force.z = MultiplyMinMaxCurve(force.z, scale);
        }
    }

    //Multiply or divide ParticleSystem.MinMaxCurve with given value
    private ParticleSystem.MinMaxCurve MultiplyMinMaxCurve(ParticleSystem.MinMaxCurve curve, float value, bool multiply = true) {
        if (multiply) {
            curve.curveMultiplier *= value;
            curve.constantMin *= value;
            curve.constantMax *= value;
        } else {
            curve.curveMultiplier /= value;
            curve.constantMin /= value;
            curve.constantMax /= value;
        }
        MultiplyCurveKeys(curve.curveMin, value, multiply);
        MultiplyCurveKeys(curve.curveMax, value, multiply);
        return curve;
    }

    //Multiply or divide all keys of an AnimationCurve with given value
    private void MultiplyCurveKeys(AnimationCurve curve, float value, bool multiply = true) {
        if (curve == null) { return; }
        if (multiply)
            for (int i = 0; i < curve.keys.Length; i++) { curve.keys[i].value *= value; }
        else
            for (int i = 0; i < curve.keys.Length; i++) { curve.keys[i].value /= value; }
    }
}