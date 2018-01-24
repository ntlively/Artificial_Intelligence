using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class AI_Editor : Editor {
	void OnSceneGUI(){
		// Changes the editor to draw a field of view visualization on the selected target
		FieldOfView fov = (FieldOfView)target;

		// Set color of view visualization
		Handles.color = Color.white;

		// Draw the max range circle of the field of view for the target
		Handles.DrawWireArc(fov.transform.position,Vector3.up,Vector3.forward,360,fov.viewRadius);

		// Draw view radius lines
		Vector3 viewAngleA = fov.DirFromAngle (-fov.viewAngle / 2, false);
		Vector3 viewAngleB = fov.DirFromAngle (fov.viewAngle/2,false);
		Handles.DrawLine (fov.transform.position,fov.transform.position + viewAngleA*fov.viewRadius);
		Handles.DrawLine (fov.transform.position,fov.transform.position + viewAngleB*fov.viewRadius);

		Handles.color = Color.red;
		foreach (Transform visibleTarget in fov.visibleTargets) {
			Handles.DrawLine (fov.transform.position, visibleTarget.position);
		}
	}
}
