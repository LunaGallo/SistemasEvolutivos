using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Module_BHV : PatternTransformAnimation_BHV {

    public GameObject jointObject;
    public bool leaveTrail = true;
    public float trailLength = 1;
    public Material trailMaterial;
    
    private LineRenderer lineRenderer;
    private List<Vector3> trail;

    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = trailMaterial;
        lineRenderer.SetWidth(0, 0.5f);
        trail = new List<Vector3>();
        RestartCycle();
    }

    void Update() {
        UpdateAnimation();
        UpdateTrail();
    }


    private void UpdateTrail(){
        if (leaveTrail) {
            if(trail.Count > trailLength * 60){
                trail.RemoveAt(0);
            }
            trail.Add(jointObject.transform.position);
        }
        else {
            trail.Clear();
        }
        lineRenderer.SetVertexCount(trail.Count);
        for (int i = 0; i < trail.Count; i++) {
            lineRenderer.SetPosition(i, trail[i]);
        }
    }
}