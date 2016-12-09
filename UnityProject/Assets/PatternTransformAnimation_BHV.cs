using UnityEngine;
using System.Collections;

public class PatternTransformAnimation_BHV : MonoBehaviour {

    public enum AnimAttrib {
        None,
        Position,
        Rotation,
        Scale
    }
    public enum AnimAxis {
        X, Y, Z
    }

    public Transform animationTarget;
    public AnimAttrib animationAttribute;
    public AnimAxis animationAxis;
    public AnimationCurve animationCurve;
    public float animationAmplitude = 1;
    public float animationStartOffset = 0;
    public float cycleTime = 1;

    protected float cycleCounter;
    protected bool isPlaying = true;

	// Use this for initialization
    void Start() {
        RestartCycle();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        UpdateAnimation();
	}

    protected void UpdateAnimation() {
        if (isPlaying) {
            cycleCounter += Time.fixedDeltaTime / cycleTime;
            cycleCounter -= Mathf.Floor(cycleCounter);
            float currentValue = animationCurve.Evaluate(cycleCounter) * animationAmplitude;
            Vector3 auxVec = Vector3.zero;
            if (animationAxis == AnimAxis.X) {
                auxVec.x = currentValue;
            }
            else if (animationAxis == AnimAxis.Y) {
                auxVec.y = currentValue;
            }
            else {
                auxVec.z = currentValue;
            }
            switch (animationAttribute) {
                case AnimAttrib.Position:
                    animationTarget.localPosition = auxVec;
                    break;
                case AnimAttrib.Rotation:
                    animationTarget.rotation = animationTarget.parent.rotation * Quaternion.Euler(auxVec);
                    break;
                case AnimAttrib.Scale:
                    animationTarget.lossyScale.Set(auxVec.x, auxVec.y, auxVec.z);
                    break;
            }
        }
    }

    public void RestartCycle() {
        cycleCounter = (int)(animationStartOffset * (cycleTime * 60));
    }

    public void StopCycle() {
        isPlaying = false;
    }

    public void PlayCycle() {
        isPlaying = true;
    }
}
