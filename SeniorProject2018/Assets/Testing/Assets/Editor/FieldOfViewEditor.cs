using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor {
	void OnSceneGUI(){
		// Changes the editor to draw a field of view visualization on the selected target
		FieldOfView fow = (FieldOfView)target;

		// Set color of view visualization
		Handles.color = Color.white;

		// Draw the max range circle of the field of view for the target
		Handles.DrawWireArc(fow.transform.position,Vector3.up,Vector3.forward,360,fow.viewRadius);

		// Draw view radius lines
		Vector3 viewAngleA = fow.DirFromAngle (-fow.viewAngle / 2, false);
		Vector3 viewAngleB = fow.DirFromAngle (fow.viewAngle/2,false);
		Handles.DrawLine (fow.transform.position,fow.transform.position + viewAngleA*fow.viewRadius);
		Handles.DrawLine (fow.transform.position,fow.transform.position + viewAngleB*fow.viewRadius);

		Handles.color = Color.red;
		foreach (Transform visibleTarget in fow.visibleTargets) {
			Handles.DrawLine (fow.transform.position, visibleTarget.position);
		}
	}
}
