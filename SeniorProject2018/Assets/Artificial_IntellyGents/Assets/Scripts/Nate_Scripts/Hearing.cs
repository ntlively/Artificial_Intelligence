using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

// BUG REPORT
// 1) NEED TO STANDARDIZE UNITS FOR DECAY MEASUREMENTS AND DB LEVELS
// 2) NEED TO STORE ADJUSTED DB LEVEL BACK ON SOURCE?

// NOTES
// 1) visualizer does not accurately portray the actual field of sound. (plane vs cone math)
// 2) UPDATE TO IMPLEMENTATION
//   * SPHERE COLLIDER OF SOUNDER CHANGES SIZE BASED OF DB LEVEL
//   * HEARER CALCULATES DISTANCE FALL OFF AND WALL FALL OFF
//   * GET SOME SORT OF PERCENT HEARD/SOUND CUTOFF
//   * THIS ACCOUNTS FOR REALLY LOUD NOISES THAT ARE FAR AWAY

public class Hearing : MonoBehaviour {

	// Variable Declaration
	public float soundRadius;
	[Range(0,360)]
	public float soundAngle;
	[Range(0,1)]
	public float wallDecay;
	[Range(0,1)]
	public float hearingAngleDecay;
	public float hearingLimit;

	public LayerMask targetMask;
	public LayerMask obstacleMask;
	public float refreshDelay;

	public List<SoundInfo> hearableTargets = new List<SoundInfo>();
	public RaycastHit[] walls;
	public float meshResolution;

	public MeshFilter soundMeshFilter;
	Mesh soundMesh;

	public List<Collider> targetsInSoundRadius;

	public GameObject actor;
	public DecibelTracker decibelScript;
	void Start(){
		soundMesh = new Mesh();
		soundMesh.name = "Sound Mesh";
		soundMeshFilter.mesh = soundMesh;
		

		actor = this.gameObject;
		decibelScript = actor.GetComponent<DecibelTracker>();

		StartCoroutine ("FindSoundTargetsWithDelay",refreshDelay);
	}

	void LateUpdate(){
		DrawHearing();
	}


	IEnumerator FindSoundTargetsWithDelay(float delay){
		while (true) {
			yield return new WaitForSeconds (delay);
			FindHearableTargets ();
		}
	
	}




	void FindHearableTargets(){
		hearableTargets.Clear ();
		walls = new RaycastHit[0];

		// Targets update their sphere colliders to be larger based of their own decibel level
		// Collider[] targetsInSoundRadius = Physics.OverlapSphere (transform.position, soundRadius, targetMask);
		targetsInSoundRadius = new List<Collider>(Physics.OverlapSphere (transform.position, soundRadius));
		List<Collider> temp = new List<Collider>();
		foreach (Collider coll in targetsInSoundRadius) 
		{
			if(coll.GetType() == typeof(SphereCollider))
			{
			  if(coll.transform != this.transform)
			  {
				temp.Add(coll);
			  }
			}
		}

		targetsInSoundRadius = temp;

		for(int i=0;i<targetsInSoundRadius.Count;i++){
			Transform target = targetsInSoundRadius[i].transform;

			float decibel = targetsInSoundRadius[i].gameObject.GetComponent<DecibelTracker>().getCurrentDecibel();
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			float distToTarget = Vector3.Distance (this.transform.position, target.position);

			// Decay sound based of distance (Initial)/4pi R^2 technically.  This needs to be refined/fudged
			//float sourceDB = target.GetComponent<Hearing>().decibel;
			float sourceDB = decibel;
			sourceDB = sourceDB *(1/distToTarget);
			//Debug.Log("dist"+distToTarget);

			// Decay sound if it is outside hearing angle
			if(!(Vector3.Angle (transform.forward, dirToTarget) < soundAngle / 2)){					
				sourceDB = sourceDB*(hearingAngleDecay);
			}

			// Decay sound if there are walls in the way

        	walls = Physics.RaycastAll(transform.position,dirToTarget,distToTarget,obstacleMask);
			// Debug.Log(walls.Length);
			for(int j = 0;j < walls.Length;j++){
				sourceDB = sourceDB*(wallDecay);
				//Debug.Log("Wall Decay Applied");
			}
			//Debug.Log(sourceDB);
			
			// Might want to store the calculated decibel to decide between options?
			if(sourceDB >= hearingLimit){
				//target.GetComponent<Hearing>().calcDB = sourceDB;
				hearableTargets.Add (new SoundInfo(target,sourceDB));
			}

		}

	}

	public void DrawHearing(){
		int stepCount = Mathf.RoundToInt(soundAngle * meshResolution);
		float stepAngleSize = soundAngle/stepCount;
		List<Vector3> soundPoints = new List<Vector3>();


		for(int i=0;i<= stepCount;i++){
			float angle = transform.eulerAngles.y - soundAngle /2 + stepAngleSize*i;
			//Debug.DrawLine(transform.position,transform.position+DirFromAngle(angle,true)*soundRadius,Color.red);
			SoundCastInfo newSoundCast = SoundCast(angle);
			soundPoints.Add(newSoundCast.point);
		}

	// A note.  These raycasts will be used to create a simple mesh displaying the field of sound.
	// You need and array of vertices and also their triangle orientations.
	// this means that if you have V number of raycast points (including the origin at the character)
	// then your array for the mesh will be of length (V-2) * 3 --> (3 vertices per triangle, times V-2 triangles)

		int vertexCount = soundPoints.Count +1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount-2)*3];

		//vertices in local space to the character
		vertices[0] = Vector3.zero;
		for(int i=0; i< vertexCount-1;i++){
			vertices[i+1] = transform.InverseTransformPoint(soundPoints[i]);

			if(i<vertexCount-2){
				triangles[i*3  ] = 0;
				triangles[i*3+1] = i+1;
				triangles[i*3+2] = i+2;
			}
		}

		soundMesh.Clear();
		soundMesh.vertices = vertices;
		soundMesh.triangles = triangles;
		soundMesh.RecalculateNormals();
	}


	SoundCastInfo SoundCast(float globalAngle){
		Vector3 dir = DirFromAngle(globalAngle, true);
		RaycastHit hit;

		if(Physics.Raycast(transform.position,dir, out hit, soundRadius, obstacleMask))
		{	
			int numHits = Physics.RaycastAll(transform.position,dir, soundRadius, obstacleMask).Length;
			return new SoundCastInfo(true, numHits, transform.position+dir*soundRadius, soundRadius, globalAngle);
		}
		else
		{
			return new SoundCastInfo(false, 0, transform.position+dir*soundRadius, soundRadius, globalAngle);
		}	
	}

	//make serializable so it shows up in unity editor
	[System.Serializable]
	public struct SoundInfo{
		public Transform target;
		public float decibel;

		public SoundInfo(Transform _target, float _decibel){
			target = _target;
			decibel = _decibel;
		}
	}


	public struct SoundCastInfo{
		public bool hit;
		public int numHits;
		public Vector3 point;
		public float dst;
		public float angle;

		public SoundCastInfo(bool _hit,int _numHits, Vector3 _point, float _dst, float _angle){
			hit = _hit;
			numHits = _numHits;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}

	// Unity angles are different from default.  0 degrees is north in unity, and 90 is east. Swap sin and cosine to fix this.
	//Global angle means it does not rotate based off the target rotation.
	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal){
		if(!angleIsGlobal)
		{
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		
	}

}
