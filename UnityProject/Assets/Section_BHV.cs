using UnityEngine;
using System.Collections;

public class Section_BHV : MonoBehaviour {

    public PatternTransformAnimation_BHV animationPattern;
    public GameObject joint;
    public GameObject capsule;
	public MeshRenderer meshRef;

    private Rigidbody _parent;
    public Rigidbody parent {
        get {
            return _parent;
        }
        set {
            _parent = value;
            Joint rbJoint = capsule.GetComponent<Joint>();
            if (rbJoint) {
                rbJoint.connectedBody = _parent;
            }
        }
    }

    public void Reset() {
        animationPattern.RestartCycle();
    }

}
