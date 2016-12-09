using UnityEngine;
using System.Collections.Generic;

public class Base_BHV : MonoBehaviour {
    
    public static int sectionNum {
        get {
            return _sectionNum;
        }
        set {
            _sectionNum = value;
            foreach (Base_BHV instance in instanceList) {
                instance.childrenProperties = new SectionProperties[_sectionNum];
                instance.childrenSections = new Section_BHV[_sectionNum];
            }
            
        }
    }
    private static int _sectionNum = 5;
    private static List<Base_BHV> instanceList = new List<Base_BHV>();


    private Rigidbody rbRef;
    [System.NonSerialized] public SectionProperties[] childrenProperties;
    [System.NonSerialized] public Section_BHV[] childrenSections;

    public GameObject sectionPrefab;
    public List<AnimationCurve> animationCurvePool;
	public float drawLineInterval;

	private float drawTimer;

    [System.Serializable]
    public class SectionProperties {
        public int parentSection = -1;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;

        [System.Serializable]
        public class AnimationPatternProperties {
            public PatternTransformAnimation_BHV.AnimAttrib animationAttribute;
            public PatternTransformAnimation_BHV.AnimAxis animationAxis;
            public AnimationCurve animationCurve;
            public float animationAmplitude;
            public float animationStartOffset;
            public float cycleTime;
        }
        public AnimationPatternProperties animationProperties = new AnimationPatternProperties();

    }
	
	public Color color = new Color(1f,1f,1f);
	
    void Awake() {
        instanceList.Add(this);
        childrenProperties = new SectionProperties[_sectionNum];
		childrenSections = new Section_BHV[_sectionNum];
        for (int i = 0; i < _sectionNum; i++) {
            childrenProperties[i] = new SectionProperties();
        }
		drawTimer = 0f;
    }

    void Start() {
        rbRef = GetComponent<Rigidbody>();
        InstantiateSections();
		//ColorMeshes(new Color (0.5f, 0.5f, 0.5f));
		if ((color.r==1f) && (color.g==1f) && (color.b==1f)) {
			RandomColorMeshes();
		}
    }

    void OnDestroy() {
        instanceList.Remove(this);
    }
	
	public void RandomColorMeshes(){
		float r = Random.Range(0f,1f);
		float g = Random.Range(0f,1f);
		float b = Random.Range(0f,1f);
		Color newColor = new Color(r,g,b);
		ColorMeshes(newColor);
		color = newColor;
	}
	
	public void ColorMeshes(Color newColor){
		this.GetComponent<MeshRenderer>().material.color = newColor;
		foreach(Section_BHV section in childrenSections){
			if (section != null) {
				section.meshRef.material.color = newColor;
			}
		}
	}

    public float maxDistAchieved = 0f;

    private List<Vector3> path = new List<Vector3>();
    void FixedUpdate() {
		drawTimer -= Time.fixedDeltaTime;
		if (drawTimer <= 0f) {
			path.Add (this.transform.position);
			drawTimer = drawLineInterval;
		}
        float curDist = this.transform.position.magnitude;
        if (maxDistAchieved < curDist) {
            maxDistAchieved = curDist;
        }
		for(int i=0; i<path.Count-1; i++) {
			Debug.DrawLine(path[i], path[i + 1],Color.red);
		}
    }

    private void InstantiateSections() {
        for(int i=0; i< _sectionNum; i++) {
            if (childrenProperties[i].parentSection == -1) {
                InstantiateSection(i);
            }
        }
    }
    private void InstantiateSection(int sectionIndex) {
        SectionProperties section = childrenProperties[sectionIndex];

        GameObject obj;
        if (section.parentSection == -1) {
            obj = (GameObject)Instantiate(sectionPrefab, this.transform.position, Quaternion.Euler(section.rotation.x, section.rotation.y, section.rotation.z));
            obj.transform.SetParent(this.transform);
            obj.transform.localScale = section.scale;
        }
        else {
            obj = (GameObject)Instantiate(sectionPrefab, childrenSections[section.parentSection].joint.transform.position, Quaternion.Euler(section.rotation.x, section.rotation.y, section.rotation.z));
            obj.transform.SetParent(childrenSections[section.parentSection].joint.transform);
            obj.transform.localScale = section.scale;
        }
        childrenSections[sectionIndex] = obj.GetComponent<Section_BHV>();
        Section_BHV sectionBehaviour = childrenSections[sectionIndex];

        sectionBehaviour.animationPattern.animationAmplitude = section.animationProperties.animationAmplitude;
        sectionBehaviour.animationPattern.animationAttribute = section.animationProperties.animationAttribute;
        sectionBehaviour.animationPattern.animationAxis = section.animationProperties.animationAxis;
        sectionBehaviour.animationPattern.animationCurve = section.animationProperties.animationCurve;
        sectionBehaviour.animationPattern.animationStartOffset = section.animationProperties.animationStartOffset;
        sectionBehaviour.animationPattern.cycleTime = section.animationProperties.cycleTime;

        if (section.parentSection == -1) {
            sectionBehaviour.parent = rbRef;
        }
        else {
            sectionBehaviour.parent = childrenSections[section.parentSection].capsule.GetComponent<Rigidbody>();
        }

        for (int i=0; i< _sectionNum; i++) {
            if (childrenProperties[i].parentSection == sectionIndex) {
                InstantiateSection(i);
            }
        }

    }

    public void Randomize() {
        for (int i=0; i<childrenProperties.Length; i++) {
            childrenProperties[i].parentSection = Random.Range(-1, i);
            childrenProperties[i].rotation = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            //float radius = Random.Range(1f, 2f);
            //childrenProperties[i].scale = new Vector3(radius, Random.Range(1f, 2f), radius);
			childrenProperties[i].scale = Vector3.one;
			
            childrenProperties[i].animationProperties.animationAxis = (PatternTransformAnimation_BHV.AnimAxis)Random.Range(1,3);
            childrenProperties[i].animationProperties.animationStartOffset = Random.Range(0f,1f);
            childrenProperties[i].animationProperties.cycleTime = Random.Range(1f,5f);
            childrenProperties[i].animationProperties.animationCurve = animationCurvePool[Random.Range(0,animationCurvePool.Count)];
            childrenProperties[i].animationProperties.animationAttribute = PatternTransformAnimation_BHV.AnimAttrib.Rotation; //(PatternTransformAnimation_BHV.AnimAttrib)Random.Range(1,4);
            switch (childrenProperties[i].animationProperties.animationAttribute) {
                case PatternTransformAnimation_BHV.AnimAttrib.Position:
                    childrenProperties[i].animationProperties.animationAmplitude = Random.Range(-1f,1f);
                    break;
                case PatternTransformAnimation_BHV.AnimAttrib.Rotation:
                    childrenProperties[i].animationProperties.animationAmplitude = Random.Range(-360f,360f);
                    break;
                case PatternTransformAnimation_BHV.AnimAttrib.Scale:
                    childrenProperties[i].animationProperties.animationAmplitude = Random.Range(0.1f,10f);
                    break;
            }
            childrenProperties[i].animationProperties.animationAmplitude *= 0.5f;

        }
    }

    public void DefinePropertiesFrom(Base_BHV first, Base_BHV second) {
        for (int i = 0; i < childrenProperties.Length; i++) {
            SectionProperties firstProp = first.childrenProperties[i];
            SectionProperties secondProp = second.childrenProperties[i];
			
			int fp = firstProp.parentSection;
			int sp = secondProp.parentSection;
            childrenProperties[i].parentSection = Random.Range(((fp > sp)? sp:fp), ((fp < sp)? sp:fp)+1);
            childrenProperties[i].rotation = (firstProp.rotation + secondProp.rotation) / 2f;
            childrenProperties[i].scale = (firstProp.scale + secondProp.scale) / 2f;

            SectionProperties.AnimationPatternProperties firstAnim = firstProp.animationProperties;
            SectionProperties.AnimationPatternProperties secondAnim = secondProp.animationProperties;
            childrenProperties[i].animationProperties.animationAxis = (Random.value > 0.5f)? firstAnim.animationAxis : secondAnim.animationAxis;
            childrenProperties[i].animationProperties.animationStartOffset = (firstAnim.animationStartOffset + secondAnim.animationStartOffset) / 2f;
            childrenProperties[i].animationProperties.cycleTime = (firstAnim.cycleTime + secondAnim.cycleTime) / 2f;
            childrenProperties[i].animationProperties.animationCurve = (Random.value > 0.5f) ? firstAnim.animationCurve : secondAnim.animationCurve;
            childrenProperties[i].animationProperties.animationAttribute = PatternTransformAnimation_BHV.AnimAttrib.Rotation;
            childrenProperties[i].animationProperties.animationAmplitude = (firstAnim.animationAmplitude + secondAnim.animationAmplitude) / 2f;
        }
    }

    public void MutateProperties(float intensity) {
        int i = Random.Range(0, sectionNum);
        int propIndex;
        if (i == 0) {
            propIndex = Random.Range(1, 9);
        }
        else {
            propIndex = Random.Range(0, 8);
        }
        switch (propIndex) {
            case 0:
                childrenProperties[i].parentSection = Random.Range(-1, i);
                break;
            case 1:
                childrenProperties[i].rotation += new Vector3(Random.Range(-180f, 180f)* intensity, Random.Range(-180f, 180f) * intensity, Random.Range(-180f, 180f) * intensity);
                if (childrenProperties[i].rotation.x < 0f) {
                    childrenProperties[i].rotation.x = 0f;
                }
                else if (childrenProperties[i].rotation.x > 360f) {
                    childrenProperties[i].rotation.x = 360f;
                }
                if (childrenProperties[i].rotation.y < 0f) {
                    childrenProperties[i].rotation.y = 0f;
                }
                else if (childrenProperties[i].rotation.y > 360f) {
                    childrenProperties[i].rotation.y = 360f;
                }
                if (childrenProperties[i].rotation.z < 0f) {
                    childrenProperties[i].rotation.z = 0f;
                }
                else if (childrenProperties[i].rotation.z > 360f) {
                    childrenProperties[i].rotation.z = 360f;
                }
                break;
            case 2:
                /*childrenProperties[i].scale.y *= Random.Range(0.5f, 2f)* intensity;
                if (childrenProperties[i].scale.y > 2f) {
                    childrenProperties[i].scale.y = 2f;
                }
                else if (childrenProperties[i].scale.y < 1f) {
                    childrenProperties[i].scale.y = 1f;
                }*/
                break;
            case 3:
                childrenProperties[i].animationProperties.animationAxis = (PatternTransformAnimation_BHV.AnimAxis)Random.Range(1, 3);
                break;
            case 4:
                childrenProperties[i].animationProperties.animationStartOffset += (Random.Range(-0.5f, 0.5f) * intensity) % 1;
                break;
            case 5:
                childrenProperties[i].animationProperties.cycleTime += ((Random.Range(-5f, 5f) * intensity) % 9.9f) + 0.1f;
                break;
            case 6:
                childrenProperties[i].animationProperties.animationCurve = animationCurvePool[Random.Range(0, animationCurvePool.Count)];
                break;
            case 7:
                childrenProperties[i].animationProperties.animationAmplitude += (((Random.Range(-360f, 360f)*intensity) + 180f) % 360f) - 180f;
                break;
        }
    }

    public void Reset() {
        for (int i=0; i<sectionNum; i++) {
            childrenSections[i].Reset();
        }
        maxDistAchieved = 0f;
        path = new List<Vector3>();
		rbRef.velocity = Vector3.zero;
		rbRef.angularVelocity = Vector3.zero;
		drawTimer = 0f;
		//ColorMeshes(new Color(0.5f, 0.5f, 0.5f));
    }

    public static int CompareDist(Base_BHV first, Base_BHV second) {
        return ((int)first.maxDistAchieved) - ((int)second.maxDistAchieved);
    }

	public void CopyProperties(Base_BHV other){
		for (int i = 0; i < other.childrenProperties.Length; i++) {
            SectionProperties otherProp = other.childrenProperties[i];

            childrenProperties[i].parentSection = otherProp.parentSection;
            childrenProperties[i].rotation = otherProp.rotation;
            childrenProperties[i].scale = otherProp.scale;

            SectionProperties.AnimationPatternProperties otherAnim = otherProp.animationProperties;
            childrenProperties[i].animationProperties.animationAxis = otherAnim.animationAxis;
            childrenProperties[i].animationProperties.animationStartOffset = otherAnim.animationStartOffset;
            childrenProperties[i].animationProperties.cycleTime = otherAnim.cycleTime;
            childrenProperties[i].animationProperties.animationCurve = otherAnim.animationCurve;
            childrenProperties[i].animationProperties.animationAttribute = PatternTransformAnimation_BHV.AnimAttrib.Rotation;
            childrenProperties[i].animationProperties.animationAmplitude = otherAnim.animationAmplitude;
        }
	}
	
}
