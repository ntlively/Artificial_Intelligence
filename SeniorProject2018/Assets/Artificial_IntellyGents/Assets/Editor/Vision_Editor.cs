using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Vision))]
public class FOV_Editor : Editor {
	void OnSceneGUI(){
		EnableFOV();
	}

	//Field of View Additional Details On Character Selection
	void EnableFOV(){
		// Changes the editor to draw a field of view visualization on the selected target
		Vision fov = (Vision)target;
		// Set color of view visualization
		Handles.color = Color.white;
		// Draw the max range circle of the field of view for the target
		Handles.DrawWireArc(fov.transform.position,Vector3.up,Vector3.forward,360,fov.viewRadius);
		// Draw view radius lines
		Vector3 viewAngleA = fov.DirFromAngle (-fov.viewAngle / 2, false);
		Vector3 viewAngleB = fov.DirFromAngle (fov.viewAngle/2,false);
		Handles.DrawLine (fov.transform.position,fov.transform.position + viewAngleA*fov.viewRadius);
		Handles.DrawLine (fov.transform.position,fov.transform.position + viewAngleB*fov.viewRadius);
		// Draw line between selected and visibleTarget
		Handles.color = Color.red;
		Vector3 modify = fov.transform.position + new Vector3(-1.0f,0,0);
		//fov.transform.position
		foreach (Transform visibleTarget in fov.visibleTargets) {
			Handles.DrawLine (modify, visibleTarget.position);
		}
	}

}
