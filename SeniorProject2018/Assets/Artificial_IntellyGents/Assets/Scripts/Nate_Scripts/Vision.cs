﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BUG REPORT
// 1) PHYSICS.RAYCAST REQUIRES A BOX COLLIDER OR SIMILAR TO DETECT COLLISIONS, MESH COLLIDERS ARE GARBO
// 2) UPDATE TO IMPLEMENTATION
//   * SPHERE COLLIDER OF AGENT CHANGES SIZE BASED OF BRIGHTNESS?
//   * SEE-ER CALCULATES DISTANCE FALL OFF AND WALL CUT OFF
//   * GET SOME SORT OF PERCENT SIGHT CUTOFF
//   * THIS ACCOUNTS FOR REALLY BRIGHT OBJECTS THAT ARE FAR AWAY

public class Vision : MonoBehaviour {

	// Variable Declarations
	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;
	// Anything in target layer can be seen/targeted
	public LayerMask targetMask;
	// Anything in obstacle layer blocks field of view raycast
	public LayerMask obstacleMask;
	// Refresh delay in case you want it slower to refresh
	public float refreshDelay;
	// A list of all targets visible in the FOV
	public List<Transform> visibleTargets = new List<Transform>();
	// Variables related to rendering the FOV
	public float meshResolution;
	public MeshFilter viewMeshFilter;
	Mesh viewMesh;


	void Start(){
		viewMesh = new Mesh();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;

		targetMask =   1 << 9 ;
		obstacleMask = 1 << 10;
		StartCoroutine ("FindViewTargetsWithDelay",refreshDelay);
	}

	// This update method draws the FOV after everything else in the scene has been drawn
	void LateUpdate(){
		DrawVision();
	}

	// Custom Update method for running with custom delay
	IEnumerator FindViewTargetsWithDelay(float delay){
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets ();
		}
	}




	void FindVisibleTargets(){
		visibleTargets.Clear ();
		Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

		for(int i=0;i<targetsInViewRadius.Length;i++){
			Transform target = targetsInViewRadius [i].transform;
			Vector3 dirToTarget = (target.position - transform.position).normalized;

			// if something in the target layer is in the view angle
			if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {					
				float dstToTarget = Vector3.Distance (transform.position, target.position);
				// Physics.Raycast(transform.position,dir, out hit, viewRadius, obstacleMask)
				//RaycastHit hit;
				if(!Physics.Raycast(transform.position,dirToTarget,dstToTarget,obstacleMask)){
					//Debug.Log("NO WALL");
					visibleTargets.Add (target);
				}
			}
		}

	}

	public void DrawVision(){
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle/stepCount;
		List<Vector3> viewPoints = new List<Vector3>();


		for(int i=0;i<= stepCount;i++){
			float angle = transform.eulerAngles.y - viewAngle /2 + stepAngleSize*i;
			//Debug.DrawLine(transform.position,transform.position+DirFromAngle(angle,true)*viewRadius,Color.red);
			ViewCastInfo newViewCast = ViewCast(angle);
			viewPoints.Add(newViewCast.point);
		}

	// A note.  These raycasts will be used to create a simple mesh displaying the field of view.
	// You need and array of vertices and also their triangle orientations.
	// this means that if you have V number of raycast points (including the origin at the character)
	// then your array for the mesh will be of length (V-2) * 3 --> (3 vertices per triangle, times V-2 triangles)

		int vertexCount = viewPoints.Count +1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount-2)*3];

		//vertices in local space to the character
		vertices[0] = Vector3.zero;
		for(int i=0; i< vertexCount-1;i++){
			vertices[i+1] = transform.InverseTransformPoint(viewPoints[i]);

			if(i<vertexCount-2){
				triangles[i*3  ] = 0;
				triangles[i*3+1] = i+1;
				triangles[i*3+2] = i+2;
			}
		}

		viewMesh.Clear();
		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals();
	}


	ViewCastInfo ViewCast(float globalAngle){
		Vector3 dir = DirFromAngle(globalAngle, true);
		RaycastHit hit;

		if(Physics.Raycast(transform.position,dir, out hit, viewRadius, obstacleMask))
		{
			return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
		}
		else
		{
			return new ViewCastInfo(false, transform.position+dir*viewRadius,viewRadius,globalAngle);
		}	
	}



	public struct ViewCastInfo{
		public bool hit;
		public Vector3 point;
		public float dst;
		public float angle;

		public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle){
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}

	// Unity angles are different from default.  0 degrees is north in unity, and 90 is east. Swap sin and cosine to fix this.
	// Global angle means it does not rotate based off the target(selected) rotation.
	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal){
		if(!angleIsGlobal)
		{
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		
	}
}
