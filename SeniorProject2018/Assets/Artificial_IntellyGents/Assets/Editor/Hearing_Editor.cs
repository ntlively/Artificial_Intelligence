using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Hearing))]
public class FOS_Editor : Editor {
	void OnSceneGUI(){
		EnableFOS();
	}

	//Field of Sound Additional Details On Character Selection
	void EnableFOS(){
		// Changes the editor to draw a field of sound visualization on the selected target
		Hearing fos = (Hearing)target;
		// Set color of sound visualization
		Handles.color = Color.white;
		// Draw the max range circle of the field of sound for the target
		Handles.DrawWireArc(fos.transform.position,Vector3.up,Vector3.forward,360,fos.soundRadius);
		// Draw sound radius lines
		Vector3 soundAngleA = fos.DirFromAngle (-fos.soundAngle / 2, false);
		Vector3 soundAngleB = fos.DirFromAngle (fos.soundAngle/2,false);
		Handles.DrawLine (fos.transform.position,fos.transform.position + soundAngleA*fos.soundRadius);
		Handles.DrawLine (fos.transform.position,fos.transform.position + soundAngleB*fos.soundRadius);
		// Draw line between selected and hearableTarget
		Handles.color = Color.blue;
		Vector3 modify = fos.transform.position + new Vector3(1.0f,0,0);

		foreach (Hearing.SoundInfo hearableTarget in fos.hearableTargets) {
			Handles.DrawLine (modify, hearableTarget.target.position);
		}
	}

}